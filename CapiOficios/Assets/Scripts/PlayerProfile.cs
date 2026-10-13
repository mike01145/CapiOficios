using UnityEngine;

public static class PlayerProfile
{
    private const string NameKey = "player_name";

    public static string Name => PlayerPrefs.GetString(NameKey, "");
    public static bool HasName => !string.IsNullOrWhiteSpace(Name);

    public static void SetName(string name)
    {
        PlayerPrefs.SetString(NameKey, name.Trim());
        PlayerPrefs.Save(); // importante en WebGL
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.Save();
    }
}
