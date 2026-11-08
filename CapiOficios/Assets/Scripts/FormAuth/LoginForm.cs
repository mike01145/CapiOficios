using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginForm : MonoBehaviour
{
    [Header("Campos")]
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;

    [Header("UI")]
    [SerializeField] private Button _submit;
    [SerializeField] private TextMeshProUGUI _errorText;

    [Header("Eventos")]
    [SerializeField] private UnityEvent<string, string> _onValid; // email, password

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    void OnEnable() => ClearFields();
    private void Start()
    {
        if (_errorText != null) _errorText.text = "";
        _submit.onClick.AddListener(Submit);
        _email.onValueChanged.AddListener(_ => ClearError());
        _password.onValueChanged.AddListener(_ => ClearError());
    }

    private void Submit()
    {
        string email = _email.text.Trim();
        string pass = _password.text;

        if (!EmailRegex.IsMatch(email)) { ShowError("El email no es válido"); return; }
        if (string.IsNullOrEmpty(pass)) { ShowError("Escribí tu contraseña"); return; }

        _onValid?.Invoke(email, pass);
    }
    private void ClearFields()
    {
        _email.text = "";
        _password.text = "";
        if (_errorText != null) _errorText.text = "";
    }
    public void ShowError(string msg)
    {
        if (_errorText != null) _errorText.text = msg;
    }

    private void ClearError()
    {
        if (_errorText != null && _errorText.text != "") _errorText.text = "";
    }
}