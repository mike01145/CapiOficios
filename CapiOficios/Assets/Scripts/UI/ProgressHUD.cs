using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Barra de progreso / temperatura. Se usa desde cualquier script:
//   ProgressHUD.Display("Moliendo", 0.5f, "50%");   ProgressHUD.Clear();
// Poné este script en el Canvas (activo) y asigná un panel HIJO que se muestra y se oculta.
public class ProgressHUD : MonoBehaviour
{
    public static ProgressHUD Instance { get; private set; }

    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private Image _fill;             // Image Type = Filled, Fill Method = Horizontal
    [SerializeField] private RectTransform _spinner;  // opcional: ícono que gira mientras espera
    [SerializeField] private float _spinSpeed = 360f;
    [SerializeField] private Color _defaultColor = new(0.30f, 0.65f, 0.35f);

    private void Awake()
    {
        Instance = this;
        if (_panel != null) _panel.SetActive(false);
    }

    public static void Display(string title, float progress, string value, Color? color = null)
    {
        if (Instance == null || Instance._panel == null) return;
        Instance._panel.SetActive(true);
        if (Instance._titleText != null) Instance._titleText.text = title;
        if (Instance._valueText != null) Instance._valueText.text = value;
        if (Instance._fill != null)
        {
            Instance._fill.fillAmount = Mathf.Clamp01(progress);
            Instance._fill.color = color ?? Instance._defaultColor;
        }
    }

    public static void Clear()
    {
        if (Instance != null && Instance._panel != null) Instance._panel.SetActive(false);
    }

    private void Update()
    {
        if (_spinner != null && _panel != null && _panel.activeSelf)
            _spinner.Rotate(0f, 0f, -_spinSpeed * Time.unscaledDeltaTime);
    }
}