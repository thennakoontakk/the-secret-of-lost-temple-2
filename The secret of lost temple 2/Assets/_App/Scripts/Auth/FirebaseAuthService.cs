using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SecretOfLostTemple.Auth
{
    [Serializable]
    public class FirebaseAuthResponse
    {
        public string idToken;
        public string email;
        public string refreshToken;
        public string expiresIn;
        public string localId;
        public string displayName;
        public bool registered;
    }

    [Serializable]
    public class FirebaseErrorResponse
    {
        public FirebaseErrorDetail error;
    }

    [Serializable]
    public class FirebaseErrorDetail
    {
        public int code;
        public string message;
    }

    public class FirebaseAuthService : MonoBehaviour
    {
        private static FirebaseAuthService instance;
        public static FirebaseAuthService Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("FirebaseAuthService");
                    instance = go.AddComponent<FirebaseAuthService>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Firebase Config")]
        [SerializeField] private string apiKey = "AIzaSyBMUo0u0C0mzhQ2WaZZgc43X3HWc0b-ULE";
        [SerializeField] private string projectId = "the-secret-of-lost-temple";

        private const string SignUpUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
        private const string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";

        public string ProjectId => projectId;
        public string CurrentUserId { get; private set; }
        public string CurrentUserEmail { get; private set; }
        public string CurrentIdToken { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentIdToken);

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSavedSession();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void SignUp(string identifier, string password, Action<FirebaseAuthResponse> onSuccess, Action<string> onError)
        {
            string email = FormatEmail(identifier);
            StartCoroutine(AuthRequestRoutine(SignUpUrl + apiKey, email, password, onSuccess, onError));
        }

        public void SignIn(string identifier, string password, Action<FirebaseAuthResponse> onSuccess, Action<string> onError)
        {
            string email = FormatEmail(identifier);
            StartCoroutine(AuthRequestRoutine(SignInUrl + apiKey, email, password, onSuccess, onError));
        }

        public void SignOut()
        {
            CurrentUserId = null;
            CurrentUserEmail = null;
            CurrentIdToken = null;
            PlayerPrefs.DeleteKey("LostTemple_Auth_Token");
            PlayerPrefs.DeleteKey("LostTemple_Auth_Email");
            PlayerPrefs.DeleteKey("LostTemple_Auth_LocalId");
            PlayerPrefs.Save();
        }

        private IEnumerator AuthRequestRoutine(string url, string email, string password, Action<FirebaseAuthResponse> onSuccess, Action<string> onError)
        {
            string payload = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        FirebaseAuthResponse response = JsonUtility.FromJson<FirebaseAuthResponse>(request.downloadHandler.text);
                        SaveSession(response);
                        onSuccess?.Invoke(response);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FirebaseAuth] Parse error: {ex.Message}");
                        onError?.Invoke("Unexpected response from Firebase.");
                    }
                }
                else
                {
                    string friendlyError = ParseFirebaseError(request.downloadHandler.text, request.error);
                    onError?.Invoke(friendlyError);
                }
            }
        }

        private void SaveSession(FirebaseAuthResponse response)
        {
            CurrentIdToken = response.idToken;
            CurrentUserEmail = response.email;
            CurrentUserId = response.localId;

            PlayerPrefs.SetString("LostTemple_Auth_Token", response.idToken);
            PlayerPrefs.SetString("LostTemple_Auth_Email", response.email);
            PlayerPrefs.SetString("LostTemple_Auth_LocalId", response.localId);
            PlayerPrefs.Save();
        }

        private void LoadSavedSession()
        {
            if (PlayerPrefs.HasKey("LostTemple_Auth_Token"))
            {
                CurrentIdToken = PlayerPrefs.GetString("LostTemple_Auth_Token");
                CurrentUserEmail = PlayerPrefs.GetString("LostTemple_Auth_Email");
                CurrentUserId = PlayerPrefs.GetString("LostTemple_Auth_LocalId");
            }
        }

        public static string FormatEmail(string identifier)
        {
            string clean = identifier.Trim();
            if (clean.Contains("@") && clean.Contains("."))
            {
                return clean;
            }
            // Sanitize username for email format
            string safeUser = System.Text.RegularExpressions.Regex.Replace(clean, @"[^a-zA-Z0-9_\.]", "");
            if (string.IsNullOrEmpty(safeUser)) safeUser = "explorer";
            return $"{safeUser.ToLowerInvariant()}@losttemple.app";
        }

        private string ParseFirebaseError(string jsonResponse, string defaultError)
        {
            if (string.IsNullOrEmpty(jsonResponse)) return defaultError;

            try
            {
                FirebaseErrorResponse errorObj = JsonUtility.FromJson<FirebaseErrorResponse>(jsonResponse);
                if (errorObj != null && errorObj.error != null)
                {
                    string msg = errorObj.error.message;
                    if (msg.StartsWith("EMAIL_EXISTS")) return "An explorer account with this name already exists! Please Log In.";
                    if (msg.StartsWith("INVALID_LOGIN_CREDENTIALS") || msg.StartsWith("INVALID_PASSWORD") || msg.StartsWith("EMAIL_NOT_FOUND"))
                        return "Invalid Explorer name or password. Please try again.";
                    if (msg.StartsWith("WEAK_PASSWORD")) return "Password is too weak! Must be at least 6 characters.";
                    if (msg.StartsWith("USER_DISABLED")) return "This account has been disabled.";
                    if (msg.StartsWith("TOO_MANY_ATTEMPTS_TRY_LATER")) return "Too many failed attempts. Please try again later.";
                    return $"Firebase Error: {msg}";
                }
            }
            catch {}

            return defaultError;
        }
    }
}
