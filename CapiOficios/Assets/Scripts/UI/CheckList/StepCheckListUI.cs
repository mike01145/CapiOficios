
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StepChecklistUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TutorialManager _tutorialManager;
    [Header("Settings")]
    [SerializeField] private bool _startOpen = false;
    [SerializeField] private bool _closeOnStepChange = false;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private Button _toggleButton;       // la pestaña / flecha
    [SerializeField] private Button _closeButton;       // la pestaña / flecha
    [SerializeField] private GameObject _listPanel;      // el "papel" con la lista
    [SerializeField] private RectTransform _arrow;       // opcional: gira al abrir
    [SerializeField] private Transform _listRoot;        // donde se crean las filas
    [SerializeField] private ChecklistRow _rowPrefab;

    private readonly List<ChecklistRow> _rows = new();
    private readonly List<int> _rowStep = new();
    private bool _open;

    private void Awake()
    {
        _toggleButton.onClick.AddListener(() => SetOpen(!_open));
        _closeButton.onClick.AddListener(() => SetOpen(false));

        SetOpen(_startOpen);
    }

    private void OnEnable()
    {
        _tutorialManager.OnSequenceStarted += StepCheckListUI_OnSequenceStartedBuild;
        _tutorialManager.OnStepChanged += StepCheckListUI_OnStepChangedRefresh;

        // por si el manager ya arrancó antes de que esto se active
        if (_tutorialManager.GetCurrentSequence != null)
        {
            StepCheckListUI_OnSequenceStartedBuild(this, new TutorialManager.SequenceStartedArgs { sequence = _tutorialManager.GetCurrentSequence, stepIndex = _tutorialManager.GetCurrentStepIndex });
            StepCheckListUI_OnStepChangedRefresh(this, new TutorialManager.SequenceStartedArgs { sequence = _tutorialManager.GetCurrentSequence, stepIndex = _tutorialManager.GetCurrentStepIndex });
        }
    }
    private void OnDisable()
    {
        _tutorialManager.OnSequenceStarted -= StepCheckListUI_OnSequenceStartedBuild;
        _tutorialManager.OnStepChanged -= StepCheckListUI_OnStepChangedRefresh;
    }

    private void SetOpen(bool open)
    {
        _open = open;
        _listPanel.SetActive(open);
    }

    private void StepCheckListUI_OnStepChangedRefresh(object sender, TutorialManager.SequenceStartedArgs e)
    {
        if (_closeOnStepChange) SetOpen(false);

        bool currentAssigned = false;
        for (int r = 0; r < _rows.Count; r++)
        {
            ChecklistRow.State state;
            if (e.stepIndex < 0 || _rowStep[r] < e.stepIndex) state = ChecklistRow.State.Done;
            else if (!currentAssigned)
            {
                state = ChecklistRow.State.Current;
                currentAssigned = true;
            }
            else state = ChecklistRow.State.Pending;
            _rows[r].SetState(state);
        }
    }
    private void StepCheckListUI_OnSequenceStartedBuild(object sender, TutorialManager.SequenceStartedArgs e)
    {
        if (_title != null) _title.text = e.sequence.name;

        foreach (var r in _rows) if (r != null) Destroy(r.gameObject);
        _rows.Clear();
        _rowStep.Clear();

        for (int i = 0; i < e.sequence.steps.Length; i++)
        {
            var st = e.sequence.steps[i];
            if (string.IsNullOrEmpty(st.listLabel)) continue;
            if (_tutorialManager.GetIsAssisted && st.onlyGuided) continue;

            var row = Instantiate(_rowPrefab, _listRoot);
            row.Setup(st.listLabel);
            _rows.Add(row);
            _rowStep.Add(i);

        }
    }
}