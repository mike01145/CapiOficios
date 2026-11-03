using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MilkTempBarUI : MonoBehaviour
{
    [SerializeField] private MilkJug _jug;
    [SerializeField] private Transform _owner;          // la varilla (vacío = este objeto)
    [SerializeField] private GameObject _bar;           // el Canvas MilkTempBar
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _value;
    [SerializeField] private Image _fill;
    [SerializeField] private RectTransform _zone;
    [SerializeField] private Vector3 _offset = new(0f, 0.35f, 0f);
    [SerializeField] private Camera _cam;

    [Header("Mismos valores que SteamWand")]
    [SerializeField] private float _minGood = 60f;
    [SerializeField] private float _maxGood = 90f;
    private bool _showing;   // la barra está visible (se calentó la leche)
    private bool _live;      // la varilla está calentando ahora mismo
    private void Awake()
    {
        if (_owner == null) _owner = transform;
        if (_cam == null) _cam = Camera.main;
        _bar.SetActive(false);
    }
    private void OnEnable()
    {
        GameEvents.OnProgressChanged += MilkTempBarUI_OnProgress;
        GameEvents.OnProgressCleared += MilkTempBarUI_OnCleared;
    }

    private void OnDisable()
    {
        GameEvents.OnProgressChanged -= MilkTempBarUI_OnProgress;
        GameEvents.OnProgressCleared -= MilkTempBarUI_OnCleared;
    }

    private void LateUpdate()
    {
        if (!_showing || _jug == null) { SetVisible(false); return; }

        // Se apaga cuando se vierte, se descarta o se vuelve a llenar
        bool empty = _jug.GetMilk == MilkQuality.None;
        bool fresh = !_live && _jug.GetTemperature <= 5.01f;
        if (empty || fresh || !_jug.gameObject.activeInHierarchy)
        {
            _showing = false;
            SetVisible(false);
            return;
        }

        SetVisible(true);

        // Ya no calienta: mostrar lo que guarda la jarra
        if (!_live)
        {
            float t = _jug.GetTemperature;
            _value.text = Mathf.RoundToInt(t) + "°C";
            _fill.fillAmount = Mathf.Clamp01(t / _maxGood);
            _fill.color = t < _minGood ? new Color(0.35f, 0.6f, 1f)
                        : t <= _maxGood ? new Color(0.3f, 0.75f, 0.35f)
                        : new Color(0.9f, 0.25f, 0.25f);
            SetZone(_minGood / _maxGood, 1f);
        }

        // Sigue a la jarra, encima
        _bar.transform.position = _jug.transform.position + _offset;
        if (_cam != null) _bar.transform.rotation = _cam.transform.rotation;
    }

    private void MilkTempBarUI_OnProgress(object sender, GameEvents.ProgressEventArgs e)
    {
        if (e.Info.Source != _owner) return;
        _showing = true;
        _live = true;

        if (_title != null) _title.text = e.Info.Title;
        _value.text = e.Info.Text;
        _fill.fillAmount = Mathf.Clamp01(e.Info.Value);
        _fill.color = e.Info.Color;

        SetZone(e.Info.ZoneMin, e.Info.ZoneMax);
    }
    private void MilkTempBarUI_OnCleared(object sender, GameEvents.ProgressClearedEventArgs e)
    {
        if (e.Source == _owner) _bar.SetActive(false);
        _live = false;
        _bar.SetActive(false);

    }
    private void SetVisible(bool v)
    {
        if (_bar.activeSelf != v) _bar.SetActive(v);
    }
    private void SetZone(float min, float max)
    {
        bool has = min >= 0f && max > min;
        _zone.gameObject.SetActive(has);
        if (!has) return;
        _zone.anchorMin = new Vector2(Mathf.Clamp01(min), 0f);
        _zone.anchorMax = new Vector2(Mathf.Clamp01(max), 1f);
        _zone.offsetMin = Vector2.zero;
        _zone.offsetMax = Vector2.zero;
    }
}