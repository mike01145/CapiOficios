using System.Runtime.InteropServices;
using UnityEngine;

public static class AuthState
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern int Auth_IsReady();
    [DllImport("__Internal")] private static extern int Auth_IsLoggedIn(); 
    [DllImport("__Internal")] private static extern void Auth_SignOut();
    public static bool IsReady => Auth_IsReady() == 1;
    public static bool IsLoggedIn => Auth_IsLoggedIn() == 1;
    public static void SignOut() => Auth_SignOut();
#else
    // En el editor no hay navegador: usá este valor para probar
    public static void SignOut() => EditorLoggedIn = false;
    public static bool EditorLoggedIn = false;
    public static bool IsReady => true;
    public static bool IsLoggedIn => EditorLoggedIn;
#endif
}