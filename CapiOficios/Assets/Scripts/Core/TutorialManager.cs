using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum StepCompletion { Button, GameStep, Feedback }

[System.Serializable]
public class TutorialStep
{
    [TextArea(2, 4)] public string text;
    [Tooltip("Texto del capibara. Vacío = usa 'text'.")]
    [TextArea(1, 3)] public string helpText;
    [Tooltip("Si está tildado, este paso se saltea en modo Assisted.")]
    public bool onlyGuided;

    public StepCompletion completion;
    public GameStep gameStep;          // si completion = GameStep
    public Feedback feedback;          // si completion = Feedback (ej. Pickup, Place)
    [Tooltip("Opcional: el Feedback solo cuenta si viene de este objeto (o un hijo).")]
    public Transform feedbackSource;

    [Tooltip("Se activan al empezar este paso.")]
    public GameObject[] reveal;
    [Tooltip("Se resaltan y se pueden clickear.")]
    public Highlightable[] targets;
    [Tooltip("Se pueden clickear pero no se resaltan.")]
    public Highlightable[] alsoAllowed;
    [Tooltip("Elemento de UI al que apunta la flecha (ej. el panel de receta).")]
    public RectTransform uiTarget;
}

[System.Serializable]
public class TutorialSequence
{
    public string name;
    public Drink drink;
    [TextArea(2, 5)] public string recipe;

    [Header("Reutilizar pasos de otra secuencia")]
    public int baseSequence = -1;       // -1 = no reutiliza
    public int baseFrom = 1;            // primer paso a copiar (salteando la bienvenida)
    public int baseTo = 13;             // hasta acá, sin incluir (hasta Extract)
    public TutorialStep[] introSteps;
    public TutorialStep[] steps;
}

public class TutorialManager : MonoBehaviour
{
    public enum Mode { Guided, Assisted }

    [SerializeField] private Mode _mode = Mode.Guided;
    [SerializeField] private TutorialSequence[] _sequences;
    [Tooltip("Solo Assisted: qué secuencia usar. -1 = al azar.")]
    [SerializeField] private int _assistedSequence = -1;
    [SerializeField] private ServePoint _servePoint;
    [SerializeField] private StepTracker _tracker;                 // opcional
    [Tooltip("Siempre clickeables en modo Guided (ej. el tacho).")]
    [SerializeField] private Highlightable[] _alwaysAllowed;

    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _nextButton;
    [SerializeField] private TextMeshProUGUI _recipeText;
    [SerializeField] private RectTransform _uiArrow;
    [SerializeField] private Vector2 _arrowOffset = new(0f, -70f);

    [Header("Capibara")]
    [SerializeField] private GameObject _helpBubble;
    [SerializeField] private Button _helpButton;
    [SerializeField] private TextMeshProUGUI _helpText;
    [SerializeField] private float _helpSeconds = 4f;

    [Header("Eventos")]
    [SerializeField] private UnityEvent _onFinished;

    private int _seqIndex;
    private int _stepIndex = -1;
    private bool _running;
    private bool _restored;
    private Highlightable[] _guided = new Highlightable[0];
    private Coroutine _helpRoutine;
    private Vector3 _arrowBase;

    private TutorialStep Current => _running ? _sequences[_seqIndex].steps[_stepIndex] : null;

    private void OnEnable()
    {
        GameEvents.OnStepCompleted += TutorialManager_OnStepCompleted;
        GameEvents.OnFeedbackRaised += TutorialManager_OnFeedback;
    }

    private void OnDisable()
    {
        GameEvents.OnStepCompleted -= TutorialManager_OnStepCompleted;
        GameEvents.OnFeedbackRaised -= TutorialManager_OnFeedback;
    }
    private void Awake()
    {
        if (_sequences == null) return;

        for (int i = 0; i < _sequences.Length; i++)
        {
            var s = _sequences[i];
            if (s.baseSequence < 0 || s.baseSequence >= i) continue;

            var b = _sequences[s.baseSequence].steps;
            var list = new System.Collections.Generic.List<TutorialStep>(s.introSteps);
            for (int k = s.baseFrom; k < Mathf.Min(s.baseTo, b.Length); k++) list.Add(b[k]);
            list.AddRange(s.steps);
            s.steps = list.ToArray();
        }
    }
    private void Start()
    {
        if (_sequences == null || _sequences.Length == 0) return;

        Highlightable.ClickFilter = IsClickAllowed;
        if (_nextButton != null) _nextButton.onClick.AddListener(OnNextClicked);
        if (_helpBubble != null) _helpBubble.SetActive(false);
        if (_uiArrow != null) _uiArrow.gameObject.SetActive(false);
        if (_panel != null) _panel.SetActive(_mode == Mode.Guided);
        if (_helpButton != null) _helpButton.onClick.AddListener(ShowHelp);

        if (_mode == Mode.Guided)
        {
            // todo lo que se "revela" arranca oculto
            foreach (var seq in _sequences)
                foreach (var st in seq.steps)
                    foreach (var go in st.reveal) if (go != null) go.SetActive(false);
            BeginSequence(0);
        }
        else
        {
            BeginSequence(_assistedSequence < 0 ? Random.Range(0, _sequences.Length) : _assistedSequence);
        }
    }

    private void OnDestroy()
    {
        if (Highlightable.ClickFilter == IsClickAllowed) Highlightable.ClickFilter = null;
    }

    private void Update()
    {
        if (_uiArrow != null && _uiArrow.gameObject.activeSelf)
            _uiArrow.position = _arrowBase + Vector3.up * (Mathf.Sin(Time.unscaledTime * 6f) * 10f);
    }

    private void TutorialManager_OnStepCompleted(object sender, GameEvents.StepCompletedEventArgs e)
    {
        var s = Current;
        if (s != null && s.completion == StepCompletion.GameStep && s.gameStep == e.Step) Advance();
    }

    private void TutorialManager_OnFeedback(object sender, GameEvents.FeedbackEventArgs e)
    {
        if (_mode == Mode.Guided && _running)
        {
            if (e.Feedback == Feedback.Return) { RestoreGuide(); return; }
            if (e.Feedback == Feedback.Pickup && _restored)
            {
                _restored = false;
                ClearGuides();
                SetGuides(Current.targets);   // vuelve a señalar el paso actual
            }
        }

        var s = Current;
        if (s == null || s.completion != StepCompletion.Feedback || s.feedback != e.Feedback) return;
        if (s.feedbackSource != null &&
            (e.Source == null || !(e.Source == s.feedbackSource || e.Source.IsChildOf(s.feedbackSource)))) return;
        Advance();
    }
    private void RestoreGuide()
    {
        if (_stepIndex < 1) return;
        var cur = Current;
        if (cur.completion == StepCompletion.Feedback && cur.feedback == Feedback.Pickup) return;

        var prev = _sequences[_seqIndex].steps[_stepIndex - 1];
        if (prev.completion != StepCompletion.Feedback || prev.feedback != Feedback.Pickup) return;

        ClearGuides();
        SetGuides(prev.targets);
        _restored = true;
        GameEvents.ShowMessage("Volvé a agarrarlo.");
    }
    // ---------- Flujo ----------
    private void BeginSequence(int index)
    {
        _seqIndex = index;
        _running = true;
        var seq = _sequences[index];

        if (_recipeText != null) _recipeText.text = seq.recipe;
        if (_servePoint != null) _servePoint.SetOrder((int)seq.drink);
        if (_tracker != null) _tracker.ResetAll();

        if (_mode == Mode.Assisted)
            foreach (var st in seq.steps)
                foreach (var go in st.reveal) if (go != null) go.SetActive(true);

        GoToStep(0);
    }

    private void GoToStep(int index)
    {
        ClearGuides();
        HideHelpBubble();

        var steps = _sequences[_seqIndex].steps;

        // En Assisted los pasos de "Siguiente" (introducción) no existen
        if (_mode == Mode.Assisted)
            while (index < steps.Length && (steps[index].completion == StepCompletion.Button || steps[index].onlyGuided)) index++;

        if (index >= steps.Length) { EndSequence(); return; }

        _stepIndex = index;
        if (_mode != Mode.Guided) return;

        var s = steps[index];
        foreach (var go in s.reveal) if (go != null) go.SetActive(true);
        SetGuides(s.targets);

        _panel.SetActive(true);
        _text.text = s.text;
        if (_nextButton != null) _nextButton.gameObject.SetActive(s.completion == StepCompletion.Button);

        if (_uiArrow != null)
        {
            bool hasArrow = s.uiTarget != null;
            _uiArrow.gameObject.SetActive(hasArrow);
            if (hasArrow) _arrowBase = s.uiTarget.position + (Vector3)_arrowOffset;
        }
    }

    private void EndSequence()
    {
        if (_mode == Mode.Guided && _seqIndex + 1 < _sequences.Length)
        {
            BeginSequence(_seqIndex + 1);
            return;
        }

        _running = false;
        ClearGuides();
        if (_panel != null) _panel.SetActive(false);
        if (_uiArrow != null) _uiArrow.gameObject.SetActive(false);
        _onFinished?.Invoke();
    }

    private void Advance() => GoToStep(_stepIndex + 1);

    private void OnNextClicked()
    {
        if (Current != null && Current.completion == StepCompletion.Button) Advance();
    }


    // ---------- Bloqueo de clicks (solo Guided) ----------
    private bool IsClickAllowed(Highlightable h)
    {
        if (_mode != Mode.Guided || !_running) return true;
        if (Contains(_alwaysAllowed, h)) return true;

        // lo de los pasos actuales y anteriores sigue permitido (para poder re-agarrarlo)
        for (int q = 0; q <= _seqIndex; q++)
        {
            var steps = _sequences[q].steps;
            int last = (q == _seqIndex) ? _stepIndex : steps.Length - 1;
            for (int i = 0; i <= last; i++)
                if (Contains(steps[i].targets, h) || Contains(steps[i].alsoAllowed, h)) return true;
        }

        GameEvents.ShowMessage("Seguí el paso de la guía.");
        return false;
    }

    private static bool Contains(Highlightable[] list, Highlightable h)
    {
        if (list == null) return false;
        foreach (var x in list) if (x == h) return true;
        return false;
    }

    // ---------- Resaltado ----------
    private void SetGuides(Highlightable[] list)
    {
        _guided = list ?? new Highlightable[0];
        foreach (var h in _guided) if (h != null) h.SetGuide(true);
    }

    private void ClearGuides()
    {
        foreach (var h in _guided) if (h != null) h.SetGuide(false);
        _guided = new Highlightable[0];
    }

    // ---------- Capibara: conectalo al OnClick del botón del capibara ----------
    private void ShowHelp()
    {
        if (_helpBubble == null || _helpText == null) return;

        var s = Current;
        _helpBubble.SetActive(true);

        if (s == null)
        {
            _helpText.text = "¡Ya terminaste todo!";
        }
        else
        {
            _helpText.text = string.IsNullOrEmpty(s.helpText) ? s.text : s.helpText;
            SetGuides(s.targets);
        }

        if (_helpRoutine != null) StopCoroutine(_helpRoutine);
        _helpRoutine = StartCoroutine(HelpTimer());
    }

    private IEnumerator HelpTimer()
    {
        yield return new WaitForSecondsRealtime(_helpSeconds);
        _helpRoutine = null;
        _helpBubble.SetActive(false);
        if (_mode == Mode.Assisted) ClearGuides();
    }

    private void HideHelpBubble()
    {
        if (_helpRoutine != null)
        {
            StopCoroutine(_helpRoutine);
            _helpRoutine = null;
        }
        if (_helpBubble != null) _helpBubble.SetActive(false);
    }
}