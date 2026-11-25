using UnityEngine;

public class AuthButton : MonoBehaviour
{
    [SerializeField] private GameObject _loggedInPanel;
    [SerializeField] private GameObject _loginPanel;

    // Conectalo al OnClick del botón
    public void Open()
    {
        bool logged = AuthState.IsReady && AuthState.IsLoggedIn;
        _loggedInPanel.SetActive(logged);
        _loginPanel.SetActive(!logged);
    }
    public void Logout()
    {
        AuthState.SignOut();
        _loggedInPanel.SetActive(false);
        _loginPanel.SetActive(true);
    }
}