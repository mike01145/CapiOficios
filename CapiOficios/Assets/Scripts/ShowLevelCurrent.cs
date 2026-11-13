using System;
using UnityEngine;

public class ShowLevelCurrent : MonoBehaviour
{
    [SerializeField] private Category _category = Category.Barismo;
    [SerializeField] private GameObject[] _objects;   // 0 = nada completado, 1 = completó el 1, etc.

    private void OnEnable()
    {
        ProgressManager.Instance.OnProgressChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= Refresh;
    }

    private void Refresh()
    {
        int completed = ProgressManager.Instance.CompletedCount(_category);
        int active = Mathf.Min(completed, _objects.Length - 1);   // tope: el último objeto

        for (int i = 0; i < _objects.Length; i++)
            if (_objects[i] != null) _objects[i].SetActive(i == active);
    }
}