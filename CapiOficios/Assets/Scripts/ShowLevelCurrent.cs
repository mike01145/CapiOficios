using System;
using UnityEngine;

public class ShowLevelCurrent : MonoBehaviour
{
    [SerializeField] private GameObject gameObjectToShow_1;
    [SerializeField] private GameObject gameObjectToShow_2;
    [SerializeField] private GameObject gameObjectToShow_3;
    int _currentLevel = 1;
    private void Start()
    {
        ProgressManager.Instance.OnProgressChanged += ShowLevelCurrent_Refresh;

        _currentLevel = ProgressManager.Instance.HighestUnlocked(Category.Barismo);
        ShowLevelCurrent_Refresh();
    }
    void OnDestroy()
    {
        ProgressManager.Instance.OnProgressChanged -= ShowLevelCurrent_Refresh;
    }

    private void ShowLevelCurrent_Refresh()
    {
        gameObjectToShow_1.SetActive(_currentLevel == 1);
        gameObjectToShow_2.SetActive(_currentLevel == 2);
        gameObjectToShow_3.SetActive(_currentLevel == 3);
    }
}