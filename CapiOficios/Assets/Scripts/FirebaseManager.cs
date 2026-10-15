public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    #if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")]
        private static extern void FirebaseLogin(
            string email,
            string password
        );
    #endif
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public void Login(string email, string password)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseLogin(email, password);
            #else
                Debug.Log("Firebase funciona mediante WebGL.");
            #endif
        }
}