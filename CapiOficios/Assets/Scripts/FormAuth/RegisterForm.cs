using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RegisterForm : MonoBehaviour
{
    [Header("Campos")]
    [SerializeField] private TMP_InputField _name;
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _repeatPassword;

    [Header("UI")]
    [SerializeField] private Button _submit;
    [SerializeField] private TextMeshProUGUI _errorText;

    [Header("Reglas")]
    [SerializeField] private int _minPasswordLength = 6;
    [SerializeField] private UnityEvent<string, string, string> _onValid; // name, email, password

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);


    void OnEnable() => ClearFields();
    private void Start()
    {
        if (_errorText != null) _errorText.text = "";
        _submit.onClick.AddListener(Submit);

        // Tab / Enter para pasar al siguiente campo es opcional; esto limpia el error al escribir
        _name.onValueChanged.AddListener(_ => ClearError());
        _email.onValueChanged.AddListener(_ => ClearError());
        _password.onValueChanged.AddListener(_ => ClearError());
        _repeatPassword.onValueChanged.AddListener(_ => ClearError());
    }

    private void Submit()
    {
        string name = _name.text.Trim();
        string email = _email.text.Trim();
        string pass = _password.text;
        string repeat = _repeatPassword.text;

        if (string.IsNullOrEmpty(name)) { ShowError("Escribí tu nombre"); return; }
        if (!EmailRegex.IsMatch(email)) { ShowError("El email no es válido"); return; }
        if (pass.Length < _minPasswordLength) { ShowError($"La contraseña debe tener al menos {_minPasswordLength} caracteres"); return; }
        if (pass != repeat) { ShowError("Las contraseñas no coinciden"); return; }

        _onValid?.Invoke(name, email, pass);
    }
    private void ClearFields()
    {
        _name.text = "";
        _email.text = "";
        _password.text = "";
        _repeatPassword.text = "";
        if (_errorText != null) _errorText.text = "";
    }
    private void ShowError(string msg)
    {
        if (_errorText != null) _errorText.text = msg;
    }

    private void ClearError()
    {
        if (_errorText != null && _errorText.text != "") _errorText.text = "";
    }
}