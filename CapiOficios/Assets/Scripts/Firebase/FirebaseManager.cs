using System;
using UnityEngine;
using System.Runtime.InteropServices;

[Serializable]
public class ProgressData
{
    public int misionesCumplidas;
    public int misionesFalladas;
    public int nivelAlcanzado;
    public int tiempoJugado;
    public string completados;
}
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public UserData GetCurrentUser { get; private set; }

    public string GetCompletados { get; private set; }

    public event Action<UserData> OnAuthLoginSuccess;
    public event Action<UserData> OnAuthRegisterSuccess;
    public event Action<string> AuthFailed;
    public event Action<ProgressData> ProgressLoaded;
#if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")] private static extern void FirebaseLogin(string email,string password);
        [DllImport("__Internal")] private static extern void FirebaseRegister(string email,string password,string nombre);
        [DllImport("__Internal")] private static extern void FirebaseLogout();
        [DllImport("__Internal")] private static extern void FirebaseUpdateProgress(string userId, int mCompleted, int mFailed, int nReached, int tPlayed);
        [DllImport("__Internal")] private static extern void FirebaseGetProgress(string userId);
        [DllImport("__Internal")] private static extern void FirebaseAddCompletedMission(string result, string description, string target, int time, string userId);
        [DllImport("__Internal")] private static extern void FirebaseUpdateProgress(string userId, int mCompleted, int mFailed, int nReached, int tPlayed, string completados);
#endif
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        gameObject.name = "FirebaseManager";
        DontDestroyOnLoad(gameObject);
        GetCurrentUser = SessionStore.Load();
    }
    // Auth
    public void Login(string email, string password)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
             FirebaseLogin(email, password);
#else
        OnLoginSuccess(JsonUtility.ToJson(new UserData { userId = "editor", email = email, nombre = "Editor" }));
#endif
    }

    public void Register(string email, string password, string nombre)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseRegister(email, password, nombre);
#else
        OnRegisterSuccess(JsonUtility.ToJson(new UserData { userId = "editor", email = email, nombre = nombre }));
#endif
    }
    public void Logout()
    {
        GetCurrentUser = null;
        SessionStore.Clear();

#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseLogout();
#endif
    }
    // Progreso
    public void UpdateProgress(int mCompleted, int mFailed, int nReached, int tPlayed, string completados)
    {
        if (GetCurrentUser == null) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseUpdateProgress(GetCurrentUser.userId, mCompleted, mFailed, nReached, tPlayed, GetCompletados);
#endif
    }

    public void GetProgress()
    {
        if (GetCurrentUser == null) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseGetProgress(GetCurrentUser.userId);
#endif
    }

    public void AddQuestCompleted(string result, string description, string target, int time)
    {
        if (GetCurrentUser == null) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseAddCompletedMission(result, description, target, time, GetCurrentUser.userId);
#endif
    }

    // TODO Checkear esto Lo llama JavaScript
    public void OnProgressLoaded(string json)
    {
        ProgressLoaded?.Invoke(JsonUtility.FromJson<ProgressData>(json));
    }
    // ---- Los llama JavaScript (SendMessage) ----
    public void OnLoginSuccess(string json)
    {
        var user = SaveUser(json);
        Debug.Log(json);
        Debug.Log($"Login OK: {GetCurrentUser.nombre} ({GetCurrentUser.email})");
        OnAuthLoginSuccess?.Invoke(user);
    }

    public void OnRegisterSuccess(string json)
    {
        var user = SaveUser(json);
        OnAuthRegisterSuccess?.Invoke(user);
    }

    // Lógica común
    private UserData SaveUser(string json)
    {
        GetCurrentUser = JsonUtility.FromJson<UserData>(json);
        SessionStore.Save(GetCurrentUser);
        return GetCurrentUser;
    }
    public void OnAuthError(string code)
    {
        AuthFailed?.Invoke(Translate(code));
    }

    private static string Translate(string code) => code switch
    {
        "auth/invalid-credential" or "auth/wrong-password" or "auth/user-not-found"
            => "Email o contraseña incorrectos",
        "auth/invalid-email" => "El email no es válido",
        "auth/email-already-in-use" => "Ese email ya está registrado",
        "auth/weak-password" => "La contraseña es muy débil",
        "auth/too-many-requests" => "Demasiados intentos, probá más tarde",
        "auth/network-request-failed" => "Sin conexión",
        _ => "Ocurrió un error, intentá de nuevo"
    };
}