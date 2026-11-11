using System;
using UnityEngine;

[Serializable]
public class UserData
{
    public string userId;
    public string nombre;
    public string email;
    public string token;
}
public static class SessionStore
{
    private const string Key = "session_user";

    public static bool HasSession => PlayerPrefs.HasKey(Key);

    public static void Save(UserData user)
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(user));
        PlayerPrefs.Save();
    }

    public static UserData Load() =>
        HasSession ? JsonUtility.FromJson<UserData>(PlayerPrefs.GetString(Key)) : null;

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}