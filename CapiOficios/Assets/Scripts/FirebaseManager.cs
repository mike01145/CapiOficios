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

        public void Registro(string email, string password, string nombre)
        {
            usuario = FirebaseRegister(email, password, nombre);
            FirebaseCreateProgressDocument(usuario.userId);
            return usuario;
        }

        public void ActualizarProgreso(string progressId, int mCumplidas, int mFallidas, int nAlcanzado, int tJugado)
        {
            FirebaseUpdateProgress(progressId, mCumplidas, mFallidas, nAlcanzado, tJugado);
        }
        
        public void ObtenerProgreso(int userId)
        {
            progreso = FirebaseGetProgress(userId);
        }

        public void AgregarMisionCompletada(string resultado, string descripcion, string objetivo, int tiempo, int userId)
        {
            FirebaseAddCompletedMission(resultado, descripcion, objetivo, tiempo, userId);
        }
        
}