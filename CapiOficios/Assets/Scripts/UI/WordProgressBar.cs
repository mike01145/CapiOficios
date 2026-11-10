using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldProgressBar : MonoBehaviour
{
    [Tooltip("La estación cuyo progreso se muestra. Vacío = este objeto.")]
    [SerializeField] private Transform _owner;
    [SerializeField] private Vector3 _offset = new(0f, 0.6f, 0f);
    [Tooltip("Ancho de la barra en el mundo (metros).")]
    [SerializeField] private float _worldWidth = 0.6f;
    [Tooltip("Qué tanto se acerca a la cámara, para que no la tape el modelo.")]
    [SerializeField] private float _towardCamera = 0.1f;
    [SerializeField] private Camera _cam;

    private const float _W = 220f;
    private const float _H = 64f;

    private GameObject _root;
    private RectTransform _fillRect;
    private Image _fillImage;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _valueText;

    private void Awake()
    {
        if (_owner == null) _owner = transform;
        Build();
    }

    private void Start()
    {
        if (_cam == null) _cam = Camera.main;
    }

    private void OnEnable()
    {
        GameEvents.OnProgressChanged += WordProgressBar_OnProgressChanged;
        GameEvents.OnProgressCleared += WordProgressBar_OnProgressCleared;
    }


    private void OnDisable()
    {
        GameEvents.OnProgressChanged -= WordProgressBar_OnProgressChanged;
        GameEvents.OnProgressCleared -= WordProgressBar_OnProgressCleared;
    }
    private void OnDestroy()
    {
        if (_root != null) Destroy(_root);
    }
    private void LateUpdate()
    {
        if (_root == null || !_root.activeSelf || _cam == null) return;
        _root.transform.position = _owner.position + _offset - _cam.transform.forward * _towardCamera;
        _root.transform.rotation = _cam.transform.rotation;   // siempre mira a la cámara
    }
    private static RectTransform MakeChild(string name, Transform parent, params System.Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        foreach (var t in components) go.AddComponent(t);
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void Build()
    {
        _root = new GameObject("WorldProgressBar", typeof(RectTransform), typeof(Canvas));
        var canvas = _root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rootRect = (RectTransform)_root.transform;
        rootRect.sizeDelta = new Vector2(_W, _H);
        _root.transform.localScale = Vector3.one * (_worldWidth / _W);

        // Fondo
        var bg = MakeChild("Background", rootRect, typeof(Image));
        SetAnchors(bg, Vector2.zero, Vector2.one);
        var bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.65f);
        bgImg.raycastTarget = false;

        // Título
        var title = MakeChild("Title", rootRect, typeof(TextMeshProUGUI));
        SetAnchors(title, new Vector2(0.04f, 0.5f), new Vector2(0.96f, 1f));
        _titleText = title.GetComponent<TextMeshProUGUI>();
        _titleText.fontSize = 22f;
        _titleText.color = Color.white;
        _titleText.alignment = TextAlignmentOptions.Center;
        _titleText.raycastTarget = false;

        // Riel y relleno de la barra
        var track = MakeChild("Track", rootRect, typeof(Image));
        SetAnchors(track, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.45f));
        var trackImg = track.GetComponent<Image>();
        trackImg.color = new Color(1f, 1f, 1f, 0.25f);
        trackImg.raycastTarget = false;

        _fillRect = MakeChild("Fill", track, typeof(Image));
        SetAnchors(_fillRect, Vector2.zero, new Vector2(0f, 1f));
        _fillImage = _fillRect.GetComponent<Image>();
        _fillImage.color = GameEvents.DefaultProgressColor;
        _fillImage.raycastTarget = false;

        // Valor ("45%" o "72°C") sobre la barra
        var value = MakeChild("Value", track, typeof(TextMeshProUGUI));
        SetAnchors(value, Vector2.zero, Vector2.one);
        _valueText = value.GetComponent<TextMeshProUGUI>();
        _valueText.fontSize = 18f;
        _valueText.color = Color.white;
        _valueText.alignment = TextAlignmentOptions.Center;
        _valueText.raycastTarget = false;

        _root.SetActive(false);
    }


    private void WordProgressBar_OnProgressChanged(object sender, GameEvents.ProgressEventArgs e)
    {
        if (e.Info.Source != _owner) return;
        _root.SetActive(true);
        _titleText.text = e.Info.Title;
        _valueText.text = e.Info.Text;
        _fillRect.anchorMax = new Vector2(Mathf.Clamp01(e.Info.Value), 1f);
        _fillImage.color = e.Info.Color;
    }
    private void WordProgressBar_OnProgressCleared(object sender, GameEvents.ProgressClearedEventArgs e)
    {
        if (e.Source == _owner) _root.SetActive(false);
    }
}