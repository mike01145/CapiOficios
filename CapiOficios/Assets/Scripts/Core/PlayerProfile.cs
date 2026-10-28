using System;
using UnityEngine;

public static class PlayerProfile
{
    private const string NameKey = "player_name";

    public static string Name => PlayerPrefs.GetString(NameKey, "");
    public static bool HasName => !string.IsNullOrWhiteSpace(Name);
    public static event EventHandler<NameConfirmedEventArgs> OnNameConfirmed;
    public class NameConfirmedEventArgs : EventArgs
    {
        public string Name { get; set; }
    }
    public static void SetName(string name)
    {
        PlayerPrefs.SetString(NameKey, name.Trim());
        PlayerPrefs.Save(); // importante en WebGL
        OnNameConfirmed?.Invoke(null, new NameConfirmedEventArgs { Name = name });
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.Save();
    }
}
