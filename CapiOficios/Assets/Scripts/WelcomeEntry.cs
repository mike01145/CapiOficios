using UnityEngine;
using UnityEngine.UI;

public class WelcomeEntry : MonoBehaviour
{
    private Button _buttonNext;
    private void Start()
    {
        _buttonNext = GetComponent<Button>();
        if (_buttonNext != null)
        {
            _buttonNext.onClick.AddListener(SetWelcomeDone);
        }
    }
    private void SetWelcomeDone()
    {
        PlayerProfile.SetWelcomeDone();
    }
}