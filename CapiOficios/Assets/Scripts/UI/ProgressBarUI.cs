using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform _owner;
    [SerializeField] private GameObject _bar;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _value;
    [SerializeField] private Image _fill;

    [Header("Posición")]
    [SerializeField] private Vector3 _offset = new(0f, 0.6f, 0f);
    [SerializeField] private Camera _cam;

    private void Awake()
    {
        if (_owner == null)
            _owner = transform;

        if (_cam == null)
            _cam = Camera.main;

        _bar.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnProgressChanged += ProgressBarUI_OnProgressChanged;
        GameEvents.OnProgressCleared += ProgressBarUI_OnProgressCleared;
    }

    private void OnDisable()
    {
        GameEvents.OnProgressChanged -= ProgressBarUI_OnProgressChanged;
        GameEvents.OnProgressCleared -= ProgressBarUI_OnProgressCleared;
    }

    private void LateUpdate()
    {
        if (!_bar.activeSelf || _cam == null)
            return;

        _bar.transform.position = _owner.position + _offset;
        _bar.transform.rotation = _cam.transform.rotation;
    }

    private void ProgressBarUI_OnProgressChanged(object sender, GameEvents.ProgressEventArgs e)
    {
        if (e.Info.Source != _owner) return;

        _bar.SetActive(true);

        _title.text = e.Info.Title;
        _value.text = e.Info.Text;

        _fill.fillAmount = Mathf.Clamp01(e.Info.Value);
        _fill.color = e.Info.Color;
    }

    private void ProgressBarUI_OnProgressCleared(object sender, GameEvents.ProgressClearedEventArgs e)
    {
        if (e.Source == _owner) _bar.SetActive(false);
    }
}