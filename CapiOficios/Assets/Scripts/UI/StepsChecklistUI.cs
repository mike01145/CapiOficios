using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StepChecklistUI : MonoBehaviour
{
    [SerializeField] private StepTracker _stepTracker;
    [Tooltip("Panel donde se crean las filas (conviene con una Image de fondo).")]
    [SerializeField] private RectTransform _listRoot;

    [Header("Estilo")]
    [SerializeField] private float _fontSize = 22f;
    [SerializeField] private float _boxSize = 26f;
    [SerializeField] private Sprite _tickSprite;   // opcional: un tilde; si está vacío se usa un cuadrado verde
    [SerializeField] private Color _boxColor = Color.white;
    [SerializeField] private Color _doneColor = new(0.30f, 0.65f, 0.35f);
    [SerializeField] private Color _textColor = new(0.15f, 0.12f, 0.10f);
    [SerializeField] private Color _doneTextColor = new(0.45f, 0.45f, 0.45f);
    [SerializeField] private Color _nextTextColor = new(0.10f, 0.35f, 0.65f);

    private class Row
    {
        public GameObject tick;
        public TextMeshProUGUI label;
    }

    private Row[] _listRows;

    private void Start()
    {
        Build();
        _stepTracker.OnChanged += StepsChecklistUI_OnRefresh;
        StepsChecklistUI_OnRefresh(this, EventArgs.Empty);
    }
    private void OnDestroy()
    {
        if (_stepTracker != null) _stepTracker.OnChanged -= StepsChecklistUI_OnRefresh;
    }

    private void Build()
    {
        var v = _listRoot.GetComponent<VerticalLayoutGroup>();
        if (v == null) v = _listRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 6f;
        v.padding = new RectOffset(14, 14, 10, 10);
        v.childAlignment = TextAnchor.UpperLeft;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = false;
        v.childForceExpandHeight = false;

        if (_listRoot.GetComponent<ContentSizeFitter>() == null)
        {
            var f = _listRoot.gameObject.AddComponent<ContentSizeFitter>();
            f.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        var steps = _stepTracker.Steps;
        _listRows = new Row[steps.Count];

        for (int i = 0; i < steps.Count; i++)
        {
            var row = new GameObject("Step_" + (i + 1), typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(_listRoot, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 10f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            // Cuadradito
            var box = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            box.transform.SetParent(row.transform, false);
            var boxImg = box.GetComponent<Image>();
            boxImg.color = _boxColor;
            boxImg.raycastTarget = false;
            var le = box.GetComponent<LayoutElement>();
            le.preferredWidth = _boxSize;
            le.preferredHeight = _boxSize;

            // Tilde (adentro del cuadradito)
            var tick = new GameObject("Tick", typeof(RectTransform), typeof(Image));
            tick.transform.SetParent(box.transform, false);
            var tr = (RectTransform)tick.transform;
            tr.anchorMin = new Vector2(0.15f, 0.15f);
            tr.anchorMax = new Vector2(0.85f, 0.85f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            var tickImg = tick.GetComponent<Image>();
            tickImg.color = _doneColor;
            tickImg.raycastTarget = false;
            if (_tickSprite != null) { tickImg.sprite = _tickSprite; tickImg.preserveAspect = true; }

            // Texto
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(row.transform, false);
            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = (i + 1) + ". " + steps[i].label;
            tmp.fontSize = _fontSize;
            tmp.color = _textColor;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            _listRows[i] = new Row { tick = tick, label = tmp };
        }
    }
    private void StepsChecklistUI_OnRefresh(object sender, EventArgs e)
    {
        int next = _stepTracker.NextIndex();
        for (int i = 0; i < _listRows.Length; i++)
        {
            bool done = _stepTracker.IsDone(i);
            _listRows[i].tick.SetActive(done);
            _listRows[i].label.fontStyle = done ? FontStyles.Strikethrough : (i == next ? FontStyles.Bold : FontStyles.Normal);
            _listRows[i].label.color = done ? _doneTextColor : (i == next ? _nextTextColor : _textColor);
        }
    }
}