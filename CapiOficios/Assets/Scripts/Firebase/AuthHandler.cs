using UnityEngine;
using UnityEngine.UI;

public class AuthHandler : MonoBehaviour
{
    [SerializeField] private LoginForm _loginForm;
    [SerializeField] private RegisterForm _registerForm;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _registerButton;
    [Header("UI")]
    [SerializeField] private GameObject _circularPI;
    [SerializeField] private GameObject _panelSuccessLogin;
    [SerializeField] private GameObject _panelSuccessRegister;
    private void OnEnable()
    {
        FirebaseManager.Instance.OnAuthLoginSuccess += AuthHandler_OnLoginSuccess;
        FirebaseManager.Instance.OnAuthRegisterSuccess += AuthHandler_OnRegisterSuccess;
        FirebaseManager.Instance.AuthFailed += AuthHandler_OnFail;
    }

    private void OnDisable()
    {
        if (FirebaseManager.Instance == null) return;
        FirebaseManager.Instance.OnAuthLoginSuccess -= AuthHandler_OnLoginSuccess;
        FirebaseManager.Instance.OnAuthRegisterSuccess -= AuthHandler_OnRegisterSuccess;
        FirebaseManager.Instance.AuthFailed -= AuthHandler_OnFail;
    }

    // Conectalo al _onValid de LoginForm (Dynamic)
    public void OnLogin(string email, string password)
    {
        SetBusy(true);
        FirebaseManager.Instance.Login(email, password);
    }

    // Conectalo al _onValid de RegisterForm (Dynamic): name, email, password
    public void OnRegister(string name, string email, string password)
    {
        SetBusy(true);
        FirebaseManager.Instance.Register(email, password, name);
    }

    private void AuthHandler_OnLoginSuccess(UserData user)
    {
        SetBusy(false);
        if (_loginForm != null) _loginForm.ClearFields();
        PlayerProfile.SetName(user.nombre);
        PlayerProfile.SetWelcomeDone();
        _panelSuccessLogin.SetActive(true);
    }

    private void AuthHandler_OnRegisterSuccess(UserData user)
    {
        SetBusy(false);
        if (_registerForm != null) _registerForm.ClearFields();
        PlayerProfile.SetName(user.nombre);
        PlayerProfile.SetWelcomeDone();
        _panelSuccessRegister.SetActive(true);
    }
    private void AuthHandler_OnFail(string message)
    {
        SetBusy(false);
        if (_loginForm != null && _loginForm.isActiveAndEnabled) _loginForm.ShowError(message);
        if (_registerForm != null && _registerForm.isActiveAndEnabled) _registerForm.ShowError(message);
    }

    // Evita doble click mientras Firebase responde
    private void SetBusy(bool busy)
    {
        if (_loginButton != null) _loginButton.interactable = !busy;
        if (_registerButton != null) _registerButton.interactable = !busy;
        if (_circularPI != null) _circularPI.SetActive(busy);
    }
}