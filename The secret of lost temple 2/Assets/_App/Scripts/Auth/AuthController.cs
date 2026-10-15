using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace SecretOfLostTemple.Auth
{
    public class AuthController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject signupPanel;
        [SerializeField] private GameObject characterSelectPanel;

        [Header("Login Form")]
        [SerializeField] private InputField loginUsernameInput;
        [SerializeField] private InputField loginPasswordInput;
        [SerializeField] private Button loginSubmitButton;
        [SerializeField] private Button loginToSignupButton; // Top right "SIGN UP" stone button on login card

        [Header("Signup Form")]
        [SerializeField] private InputField signupUsernameInput;
        [SerializeField] private InputField signupEmailInput;
        [SerializeField] private InputField signupPasswordInput;
        [SerializeField] private Button signupSubmitButton;
        [SerializeField] private Button signupToLoginButton; // "Already have an account? Log In" button
        [SerializeField] private Button signupBackButton;    // Top-left round stone back button

        [Header("Character Select Form")]
        [SerializeField] private Button mayaCardButton;
        [SerializeField] private Button senukCardButton;
        [SerializeField] private GameObject mayaSelectedGlow;
        [SerializeField] private GameObject senukSelectedGlow;
        [SerializeField] private Button confirmCharacterButton;
        [SerializeField] private Text selectedCharacterInfoText;

        [Header("Feedback")]
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Scene Navigation")]
        [SerializeField] private string targetSceneName = "HomeMapScene";
        [SerializeField] private float transitionDelay = 0.8f;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip errorSound;

        private bool isSubmitting = false;
        private string selectedCharacter = "Maya"; // Default selection

        private void Awake()
        {
            // Ensure EventSystem is active with a working StandaloneInputModule for mouse/touch clicks
            EventSystem es = FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                es = esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
            else
            {
                // If no input module or broken module, add StandaloneInputModule
                if (es.GetComponent<StandaloneInputModule>() == null && es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<StandaloneInputModule>();
                }
            }

            // Optimize TouchScreenKeyboard and mobile input field behavior
            ConfigureInputField(loginUsernameInput);
            ConfigureInputField(loginPasswordInput);
            ConfigureInputField(signupUsernameInput);
            ConfigureInputField(signupEmailInput);
            ConfigureInputField(signupPasswordInput);
        }

        private void ConfigureInputField(InputField input)
        {
            if (input == null) return;
            input.shouldHideMobileInput = true;
            input.shouldActivateOnSelect = true;
        }

        private void Start()
        {
            Debug.Log("[AuthController] Initializing Login, Signup & Character Selection...");

            // Bind Login buttons
            if (loginSubmitButton != null)
            {
                loginSubmitButton.onClick.RemoveAllListeners();
                loginSubmitButton.onClick.AddListener(OnLoginSubmit);
            }

            if (loginToSignupButton != null)
            {
                loginToSignupButton.onClick.RemoveAllListeners();
                loginToSignupButton.onClick.AddListener(ShowSignupPanel);
            }

            // Bind Signup buttons
            if (signupSubmitButton != null)
            {
                signupSubmitButton.onClick.RemoveAllListeners();
                signupSubmitButton.onClick.AddListener(OnSignupSubmit);
            }

            if (signupToLoginButton != null)
            {
                signupToLoginButton.onClick.RemoveAllListeners();
                signupToLoginButton.onClick.AddListener(ShowLoginPanel);
            }

            if (signupBackButton != null)
            {
                signupBackButton.onClick.RemoveAllListeners();
                signupBackButton.onClick.AddListener(ShowLoginPanel);
            }

            // Bind Character Select buttons
            if (mayaCardButton != null)
            {
                mayaCardButton.onClick.RemoveAllListeners();
                mayaCardButton.onClick.AddListener(SelectMaya);
            }

            if (senukCardButton != null)
            {
                senukCardButton.onClick.RemoveAllListeners();
                senukCardButton.onClick.AddListener(SelectSenuk);
            }

            if (confirmCharacterButton != null)
            {
                confirmCharacterButton.onClick.RemoveAllListeners();
                confirmCharacterButton.onClick.AddListener(ConfirmCharacterAndStart);
            }

            // Load last username if saved
            if (PlayerPrefs.HasKey("LostTemple_Last_User") && loginUsernameInput != null)
            {
                loginUsernameInput.text = PlayerPrefs.GetString("LostTemple_Last_User");
            }

            // Check if user explicitly logged out
            if (PlayerPrefs.GetInt("LostTemple_Logged_Out", 0) == 1)
            {
                PlayerPrefs.SetInt("LostTemple_Logged_Out", 0);
                PlayerPrefs.DeleteKey("LostTemple_Auth_Token");
                PlayerPrefs.DeleteKey("LostTemple_Auth_Email");
                PlayerPrefs.DeleteKey("LostTemple_Auth_LocalId");
                PlayerPrefs.Save();
                if (FirebaseAuthService.Instance != null)
                {
                    FirebaseAuthService.Instance.SignOut();
                }
                ShowLoginPanel();
                SetStatus("Logged out successfully.");
                return;
            }

            // Auto-login: If already authenticated, navigate directly without asking to login again!
            if (FirebaseAuthService.Instance.IsLoggedIn || PlayerPrefs.HasKey("LostTemple_Auth_Token"))
            {
                string user = PlayerPrefs.GetString("LostTemple_Last_User", "Explorer");
                Debug.Log($"[AuthController] Auto-logging in saved session for: {user}");
                SetStatus($"Welcome back, {user}!", new Color(0.4f, 1f, 0.5f));

                if (ResearchProgress.HasKey("SelectedCharacter"))
                {
                    if (loginPanel != null) loginPanel.SetActive(false);
                    if (signupPanel != null) signupPanel.SetActive(false);
                    if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
                    StartCoroutine(TransitionRoutine());
                }
                else
                {
                    ShowCharacterSelectPanel();
                }
                return;
            }

            // Otherwise, start on Login Panel
            ShowLoginPanel();
            SetStatus("");
        }

        private void Update()
        {
            // Dynamic golden aura pulsing behind selected character
            if (characterSelectPanel != null && characterSelectPanel.activeInHierarchy)
            {
                float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * 3.5f);
                if (mayaSelectedGlow != null && mayaSelectedGlow.activeSelf)
                {
                    var img = mayaSelectedGlow.GetComponent<Image>();
                    if (img != null) img.color = new Color(1f, 0.88f, 0.35f, pulse * 0.95f);
                }
                if (senukSelectedGlow != null && senukSelectedGlow.activeSelf)
                {
                    var img = senukSelectedGlow.GetComponent<Image>();
                    if (img != null) img.color = new Color(1f, 0.88f, 0.35f, pulse * 0.95f);
                }
            }
        }

        public void ShowLoginPanel()
        {
            if (isSubmitting) return;
            Debug.Log("[AuthController] Switching to Login Panel");
            if (loginPanel != null) loginPanel.SetActive(true);
            if (signupPanel != null) signupPanel.SetActive(false);
            if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
            SetStatus("");
            PlaySound(clickSound);
        }

        public void ShowSignupPanel()
        {
            if (isSubmitting) return;
            Debug.Log("[AuthController] Switching to Signup Panel");
            if (loginPanel != null) loginPanel.SetActive(false);
            if (signupPanel != null) signupPanel.SetActive(true);
            if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
            SetStatus("");
            PlaySound(clickSound);
        }

        public void ShowCharacterSelectPanel()
        {
            if (isSubmitting) return;
            Debug.Log("[AuthController] Opening Character Selection Panel");
            if (loginPanel != null) loginPanel.SetActive(false);
            if (signupPanel != null) signupPanel.SetActive(false);
            if (characterSelectPanel != null) characterSelectPanel.SetActive(true);
            
            // Default select Maya
            SelectMaya();
            SetStatus("");
            PlaySound(clickSound);
        }

        private Coroutine mayaPopRoutine;
        private Coroutine senukPopRoutine;

        public void SelectMaya()
        {
            selectedCharacter = "Maya";
            Debug.Log("[AuthController] Character Selected: MAYA");
            if (mayaSelectedGlow != null) mayaSelectedGlow.SetActive(true);
            if (senukSelectedGlow != null) senukSelectedGlow.SetActive(false);
            if (selectedCharacterInfoText != null)
            {
                selectedCharacterInfoText.text = "<b>MAYA SELECTED</b>\nAncient Lore Scholar & Temple Inscription Decoder";
            }

            // Animate Pop Scale for Maya (Enlarged) and Senuk (Standard)
            if (mayaCardButton != null)
            {
                if (mayaPopRoutine != null) StopCoroutine(mayaPopRoutine);
                mayaPopRoutine = StartCoroutine(PopAnimation(mayaCardButton.transform, 1.12f, true));
            }
            if (senukCardButton != null)
            {
                if (senukPopRoutine != null) StopCoroutine(senukPopRoutine);
                senukPopRoutine = StartCoroutine(PopAnimation(senukCardButton.transform, 0.95f, false));
            }

            PlaySound(clickSound);
        }

        public void SelectSenuk()
        {
            selectedCharacter = "Senuk";
            Debug.Log("[AuthController] Character Selected: SENUK");
            if (mayaSelectedGlow != null) mayaSelectedGlow.SetActive(false);
            if (senukSelectedGlow != null) senukSelectedGlow.SetActive(true);
            if (selectedCharacterInfoText != null)
            {
                selectedCharacterInfoText.text = "<b>SENUK SELECTED</b>\nAgile Explorer & Temple Maze Navigator";
            }

            // Animate Pop Scale for Senuk (Enlarged) and Maya (Standard)
            if (senukCardButton != null)
            {
                if (senukPopRoutine != null) StopCoroutine(senukPopRoutine);
                senukPopRoutine = StartCoroutine(PopAnimation(senukCardButton.transform, 1.12f, true));
            }
            if (mayaCardButton != null)
            {
                if (mayaPopRoutine != null) StopCoroutine(mayaPopRoutine);
                mayaPopRoutine = StartCoroutine(PopAnimation(mayaCardButton.transform, 0.95f, false));
            }

            PlaySound(clickSound);
        }

        private IEnumerator PopAnimation(Transform target, float targetScale, bool isSelected)
        {
            if (target == null) yield break;
            Vector3 startScale = target.localScale;

            if (isSelected)
            {
                // Dynamic punch pop overshoot
                float elapsed = 0f;
                float duration = 0.14f;
                Vector3 overshoot = new Vector3(targetScale * 1.08f, targetScale * 1.08f, 1f);

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    target.localScale = Vector3.Lerp(startScale, overshoot, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }

                // Settle to target scale
                elapsed = 0f;
                duration = 0.12f;
                Vector3 finalScale = new Vector3(targetScale, targetScale, 1f);
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    target.localScale = Vector3.Lerp(overshoot, finalScale, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
                target.localScale = finalScale;
            }
            else
            {
                // Smooth resting scale
                float elapsed = 0f;
                float duration = 0.18f;
                Vector3 finalScale = new Vector3(targetScale, targetScale, 1f);

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    target.localScale = Vector3.Lerp(startScale, finalScale, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
                target.localScale = finalScale;
            }
        }

        public void ConfirmCharacterAndStart()
        {
            if (isSubmitting) return;

            Debug.Log($"[AuthController] Confirmed Character: {selectedCharacter}");
            ResearchProgress.SetString("SelectedCharacter", selectedCharacter);
            PlayerPrefs.Save();

            // Update character selection in Firestore Database
            string userId = FirebaseAuthService.Instance.CurrentUserId;
            string idToken = FirebaseAuthService.Instance.CurrentIdToken;
            if (!string.IsNullOrEmpty(userId))
            {
                FirestoreService.Instance.UpdateCharacterSelection(userId, idToken, selectedCharacter);
            }

            PlaySound(successSound);
            SetStatus($"Explorer {selectedCharacter} Ready! Entering Adventure Map...", new Color(0.4f, 1f, 0.5f));
            StartCoroutine(TransitionRoutine());
        }

        public void OnLoginSubmit()
        {
            if (isSubmitting) return;

            string user = loginUsernameInput != null ? loginUsernameInput.text.Trim() : "";
            string pass = loginPasswordInput != null ? loginPasswordInput.text : "";

            Debug.Log($"[AuthController] Login submitted for user: {user}");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                SetStatus("Please enter your Explorer Name and Password.", new Color(1f, 0.4f, 0.4f));
                PlaySound(errorSound);
                return;
            }

            SetLoading(true);
            SetStatus("Connecting to Firebase... Logging in...", new Color(1f, 0.85f, 0.4f));
            PlayerPrefs.SetString("LostTemple_Last_User", user);
            PlayerPrefs.Save();

            FirebaseAuthService.Instance.SignIn(user, pass,
                (response) => {
                    SetLoading(false);
                    PlaySound(successSound);
                    SetStatus($"Welcome back, {user}!", new Color(0.4f, 1f, 0.5f));

                    // If character already selected before, enter Map Scene directly; otherwise pick character
                    if (ResearchProgress.HasKey("SelectedCharacter"))
                    {
                        StartCoroutine(TransitionRoutine());
                    }
                    else
                    {
                        ShowCharacterSelectPanel();
                    }
                },
                (error) => {
                    SetLoading(false);
                    PlaySound(errorSound);
                    SetStatus(error, new Color(1f, 0.35f, 0.35f));
                }
            );
        }

        public void OnSignupSubmit()
        {
            if (isSubmitting) return;

            string user = signupUsernameInput != null ? signupUsernameInput.text.Trim() : "";
            string email = signupEmailInput != null ? signupEmailInput.text.Trim() : "";
            string pass = signupPasswordInput != null ? signupPasswordInput.text : "";

            Debug.Log($"[AuthController] Signup submitted for user: {user}, email: {email}");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                SetStatus("Please fill in Username and Password.", new Color(1f, 0.4f, 0.4f));
                PlaySound(errorSound);
                return;
            }

            if (pass.Length < 6)
            {
                SetStatus("Password must be at least 6 characters.", new Color(1f, 0.4f, 0.4f));
                PlaySound(errorSound);
                return;
            }

            string identifier = !string.IsNullOrEmpty(email) ? email : user;

            SetLoading(true);
            SetStatus("Connecting to Firebase... Creating Account...", new Color(1f, 0.85f, 0.4f));
            PlayerPrefs.SetString("LostTemple_Last_User", user);
            PlayerPrefs.Save();

            FirebaseAuthService.Instance.SignUp(identifier, pass,
                (response) => {
                    SetStatus("Initializing Research Database...", new Color(1f, 0.88f, 0.45f));

                    // Automatically write all 3 Research Components to Firestore Database
                    FirestoreService.Instance.InitializeUserResearchProfile(
                        response.localId,
                        response.idToken,
                        user,
                        response.email,
                        "Maya",
                        () => {
                            SetLoading(false);
                            PlaySound(successSound);
                            SetStatus($"Account & Research Profile Ready! Choose your Explorer...", new Color(0.4f, 1f, 0.5f));
                            ShowCharacterSelectPanel();
                        },
                        (firestoreError) => {
                            SetLoading(false);
                            PlaySound(successSound);
                            SetStatus($"Account Ready! Choose your Explorer...", new Color(0.4f, 1f, 0.5f));
                            ShowCharacterSelectPanel();
                        }
                    );
                },
                (error) => {
                    SetLoading(false);
                    PlaySound(errorSound);
                    SetStatus(error, new Color(1f, 0.35f, 0.35f));
                }
            );
        }

        private void SetLoading(bool loading)
        {
            isSubmitting = loading;
            if (loginSubmitButton != null) loginSubmitButton.interactable = !loading;
            if (signupSubmitButton != null) signupSubmitButton.interactable = !loading;
            if (confirmCharacterButton != null) confirmCharacterButton.interactable = !loading;
            if (loadingIndicator != null) loadingIndicator.SetActive(loading);
        }

        private void SetStatus(string message, Color? color = null)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color ?? Color.white;
            }
        }

        private IEnumerator TransitionRoutine()
        {
            yield return new WaitForSeconds(transitionDelay);

            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                if (SceneManager.sceneCountInBuildSettings > 2)
                {
                    SceneManager.LoadScene(2);
                }
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
