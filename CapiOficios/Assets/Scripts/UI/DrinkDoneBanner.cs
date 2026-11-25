using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrinkDoneBanner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject _banner;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private RectTransform _popTarget;       // normalmente el mismo Banner
    [SerializeField] private TextMeshProUGUI _drinkName;
    [SerializeField] private Button _closeButton;             // para cerrar el banner
    [SerializeField] private RectTransform[] _bars;          // las barritas decorativas
    [SerializeField] private ServePoint _servePoint;         // para saber qué bebida era
    [Header("Tutorial")]
    [SerializeField] private TutorialManager _tutorial;
    [SerializeField] private bool _hideOnLastDrink = true;

    [Header("Tiempos")]
    [SerializeField] private float _fadeIn = 0.2f;
    [SerializeField] private float _holdSeconds = 1.2f;
    [SerializeField] private float _fadeOut = 0.3f;
    [Header("Barritas")]
    [SerializeField] private float _barMaxHeight = 60f;
    [SerializeField] private float _barMinHeight = 8f;
    [SerializeField] private float _barSpeed = 9f;
    [Header("Sonido (opcional)")]
    [SerializeField] private AudioClip _sound;

    private Coroutine _routine;
    private bool _open;

    private void Awake()
    {
        if (_banner != null) _banner.SetActive(false);
        if (_closeButton != null) _closeButton.onClick.AddListener(Out);
    }

    private void OnEnable() => GameEvents.OnFeedbackRaised += OnFeedback;
    private void OnDisable() => GameEvents.OnFeedbackRaised -= OnFeedback;

    private void OnFeedback(object sender, GameEvents.FeedbackEventArgs e)
    {
        if (e.Feedback != Feedback.Delivered) return;
        if (_hideOnLastDrink && _tutorial != null && _tutorial.GetIsLastSequence) return;
        Show();
    }

    // También podés llamarlo desde un UnityEvent
    public void Show()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine());
    }
    public void Out()
    {
        if (!_open) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(OutRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (_drinkName != null && _servePoint != null)
            _drinkName.text = "Café " + _servePoint.GetCurrentOrder.ToString();

        _open = true;
        _banner.SetActive(true);
        _group.alpha = 0f;
        _group.interactable = false;
        AudioManager.PlaySfx(_sound);

        // Entrada: fade y pop
        for (float t = 0f; t < _fadeIn; t += Time.unscaledDeltaTime)
        {
            float p = t / _fadeIn;
            _group.alpha = p;
            _popTarget.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, Mathf.SmoothStep(0f, 1f, p));
            AnimateBars(1f);
            yield return null;
        }
        _group.alpha = 1f;
        _popTarget.localScale = Vector3.one;
        _group.interactable = true;

        // Queda abierto: las barritas siguen bailando suave hasta que se toque el botón
        while (true)
        {
            AnimateBars(0.5f);
            yield return null;
        }
    }

    private IEnumerator OutRoutine()
    {
        _group.interactable = false;

        for (float t = 0f; t < _fadeOut; t += Time.unscaledDeltaTime)
        {
            _group.alpha = 1f - t / _fadeOut;
            yield return null;
        }

        _banner.SetActive(false);
        _open = false;
        _routine = null;
    }
    private void AnimateBars(float intensity)
    {
        for (int i = 0; i < _bars.Length; i++)
        {
            float wave = Mathf.PerlinNoise(Time.unscaledTime * _barSpeed, i * 3.1f);
            float h = Mathf.Lerp(_barMinHeight, _barMaxHeight, wave) * Mathf.Lerp(0.3f, 1f, intensity);
            _bars[i].sizeDelta = new Vector2(_bars[i].sizeDelta.x, h);
        }
    }
}