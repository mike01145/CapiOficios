using System;
using UnityEngine;

public enum Category { Barismo, Carpinteria }

public class ProgressManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool unlockAll = false;

    public const int LevelsPerCategory = 3;

    private static ProgressManager _instance;

    // Se crea solo la primera vez que alguien lo usa: no hace falta ponerlo en una escena
    public static ProgressManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ProgressManager");
                _instance = go.AddComponent<ProgressManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    // Los botones se suscriben para refrescarse cuando cambia el progreso
    public event Action OnProgressChanged;
    // Nivel que se está jugando (para saber cuál marcar como completado)
    public Category GetCurrentCategory { get; private set; }
    public int GetCurrentLevel { get; private set; } = 1;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Niveles numerados del 1 al 3
    private static string Key(Category c, int level) => $"progress_{c}_{level}";

    public bool IsCompleted(Category c, int level) => PlayerPrefs.GetInt(Key(c, level), 0) == 1;

    // El nivel 1 siempre está abierto; los demás se abren al completar el anterior
    public bool IsUnlocked(Category c, int level)
    {
        if (unlockAll || level <= 1) return true;
        return IsCompleted(c, level - 1);
    }

    public int CompletedCount(Category c)
    {
        int n = 0;
        for (int i = 1; i <= LevelsPerCategory; i++) if (IsCompleted(c, i)) n++;
        return n;
    }

    public bool IsCategoryCompleted(Category c) => CompletedCount(c) == LevelsPerCategory;

    public void SetCurrent(Category c, int level)
    {
        GetCurrentCategory = c;
        GetCurrentLevel = level;
    }

    public void CompleteLevel(Category c, int level)
    {
        if (IsCompleted(c, level)) return;
        PlayerPrefs.SetInt(Key(c, level), 1);
        PlayerPrefs.Save(); // importante en WebGL para que se escriba ya
        OnProgressChanged?.Invoke();
    }

    public void CompleteCurrentLevel() => CompleteLevel(GetCurrentCategory, GetCurrentLevel);

    public void ResetProgress()
    {
        foreach (Category c in Enum.GetValues(typeof(Category)))
            for (int i = 1; i <= LevelsPerCategory; i++)
                PlayerPrefs.DeleteKey(Key(c, i));
        PlayerPrefs.Save();
        OnProgressChanged?.Invoke();
    }
}