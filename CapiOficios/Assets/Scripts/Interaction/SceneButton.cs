using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneButton : MonoBehaviour
{

    [SerializeField] private string _sceneName;
    [Space]
    [Header("Transition Options")]
    [SerializeField] private UnityEvent _midAction; // acción a ejecutar en medio de la transición
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => SceneChanger.Instance.GoTo(_sceneName, _midAction));
    }
}