using System;
using UnityEngine;

public static class PlayerProfile
{
    private const string NameKey = "player_name";
    private const string WelcomeKey = "welcome_done";

    public static string Name => PlayerPrefs.GetString(NameKey, "");
    public static bool GetHasName => !string.IsNullOrWhiteSpace(Name);
    public static bool GetHasWelcomeBeenDone => PlayerPrefs.GetInt(WelcomeKey, 0) == 1;
    public static event EventHandler<NameConfirmedEventArgs> OnNameConfirmed;
    public static event EventHandler OnWelcomeDone;
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
    public static void SetWelcomeDone()
    {
        PlayerPrefs.SetInt(WelcomeKey, 1);
        PlayerPrefs.Save(); // importante en WebGL
        OnWelcomeDone?.Invoke(null, EventArgs.Empty);
    }
    public static void Clear()
    {
        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.DeleteKey(WelcomeKey);
        PlayerPrefs.Save();
    }
}
