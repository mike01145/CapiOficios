using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PasswordToggle : MonoBehaviour
{
    [SerializeField] private TMP_InputField[] _fields;   // Password y, si querés, Repeat Password
    [SerializeField] private Image _icon;                // la Image del botón
    [SerializeField] private Sprite _eyeOpen;            // contraseña visible
    [SerializeField] private Sprite _eyeClosed;          // contraseña oculta

    private bool _visible;
    private Button _button;
    private void Awake()
    {
        if (_icon == null) _icon = GetComponent<Image>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Toggle);

        Apply();
    }

    public void Toggle()
    {
        _visible = !_visible;
        Apply();
    }

    private void Apply()
    {
        foreach (var f in _fields)
        {
            if (f == null) continue;
            f.contentType = _visible
                ? TMP_InputField.ContentType.Standard
                : TMP_InputField.ContentType.Password;
            f.ForceLabelUpdate();
        }

        if (_icon != null)
            _icon.sprite = _visible ? _eyeOpen : _eyeClosed;
    }
}