using UnityEngine;

public class WelcomeGate : MonoBehaviour
{
    [SerializeField] private GameObject _welcomePanel;
    [SerializeField] private GameObject _homePanel;

    private void Start()
    {
        bool _done = PlayerProfile.GetHasWelcomeBeenDone;
        _welcomePanel.SetActive(!_done);
        _homePanel.SetActive(_done);
    }
}