using System;
using UnityEngine;

public enum Category { Barismo, Carpinteria }

public class ProgressManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool unlockAll = false;

    [SerializeField] private int LevelsPerCategory = 3;

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
    public static int GetLevelsPerCategory => Instance.LevelsPerCategory;
    public int GetCurrentLevel { get; private set; } = 1;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        // Cuando Firebase devuelve el progreso, lo mezclamos con el local
        FirebaseManager.Instance.ProgressLoaded += ApplyRemote;
    }
    private void OnDestroy()
    {
        if (_instance == this && FirebaseManager.Instance != null)
            FirebaseManager.Instance.ProgressLoaded -= ApplyRemote;
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
        SyncToServer(c, level);
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
    // Devuelve el nivel más alto desbloqueado (1 a LevelsPerCategory)
    public int HighestUnlocked(Category c)
    {
        int highest = 1; // el nivel 1 siempre está abierto
        for (int i = 1; i <= LevelsPerCategory; i++)
            if (IsUnlocked(c, i)) highest = i;
        return highest;
    }

    // Cantidad de niveles desbloqueados (coincide con HighestUnlocked)
    public int UnlockedCount(Category c) => HighestUnlocked(c);

    private void ApplyRemote(ProgressData data)
    {
        if (!string.IsNullOrEmpty(data.completados))
        {
            foreach (var token in data.completados.Split(','))
            {
                var parts = token.Split('_');
                if (parts.Length != 2) continue;
                if (!Enum.TryParse(parts[0], out Category c)) continue;
                if (!int.TryParse(parts[1], out int level)) continue;
                PlayerPrefs.SetInt(Key(c, level), 1);
            }
            PlayerPrefs.Save();
        }
        OnProgressChanged?.Invoke();
    }
    private string BuildCompleted()
    {
        var list = new System.Collections.Generic.List<string>();
        foreach (Category c in Enum.GetValues(typeof(Category)))
            for (int i = 1; i <= LevelsPerCategory; i++)
                if (IsCompleted(c, i)) list.Add($"{c}_{i}");
        return string.Join(",", list);
    }
    private void SyncToServer(Category c, int level)
    {
        var fm = FirebaseManager.Instance;
        if (fm.GetCurrentUser == null) return;

        int total = 0;
        foreach (Category cat in Enum.GetValues(typeof(Category))) total += CompletedCount(cat);

        fm.UpdateProgress(total, 0, HighestUnlocked(c), 0, BuildCompleted());
        fm.AddQuestCompleted("completada", $"{c} nivel {level}", $"{c}_{level}", 0);
    }
}