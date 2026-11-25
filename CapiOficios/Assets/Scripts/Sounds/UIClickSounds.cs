using UnityEngine;
using UnityEngine.UI;

public class UiClickSounds : MonoBehaviour
{
    private void Start()
    {
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (b.GetComponent<ButtonSuccessFeedback>() != null) continue;
            if (b.GetComponent<ButtonWrongFeedback>() != null) continue;
            b.onClick.AddListener(() => AudioManager.PlayUi(UiSound.Click));
        }
    }
}