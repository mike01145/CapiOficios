using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Ponelo en cada botón de nivel del menú
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [SerializeField] private Category _category;
    [SerializeField, Range(1, ProgressManager.LevelsPerCategory)] private int _level = 1;
    [SerializeField] private string _sceneToLoad;       // escena del nivel (vacío = no carga nada)

    [Header("Opcionales")]
    [SerializeField] private GameObject _lockedIcon;    // candado
    [SerializeField] private GameObject _completedIcon; // tilde / estrella

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        ProgressManager.Instance.OnProgressChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= Refresh;
    }

    private void Refresh()
    {
        var pm = ProgressManager.Instance;
        bool unlocked = pm.IsUnlocked(_category, _level);
        _button.interactable = unlocked;
        if (_lockedIcon != null) _lockedIcon.SetActive(!unlocked);
        if (_completedIcon != null) _completedIcon.SetActive(pm.IsCompleted(_category, _level));
    }

    private void OnClick()
    {
        ProgressManager.Instance.SetCurrent(_category, _level);
        if (!string.IsNullOrEmpty(_sceneToLoad)) SceneManager.LoadScene(_sceneToLoad);
    }
}