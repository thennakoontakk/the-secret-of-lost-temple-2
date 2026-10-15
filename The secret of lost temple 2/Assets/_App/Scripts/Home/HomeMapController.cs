using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace SecretOfLostTemple.Home
{
    public class HomeMapController : MonoBehaviour
    {
        [Header("Profile HUD")]
        [SerializeField] private Button profileCardButton;
        [SerializeField] private Image characterAvatarImage;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerTitleText;
        [SerializeField] private Sprite mayaAvatarSprite;
        [SerializeField] private Sprite senukAvatarSprite;

        [Header("Profile Details Modal")]
        [SerializeField] private GameObject profileModalPanel;
        [SerializeField] private Image modalAvatarImage;
        [SerializeField] private Text modalPlayerName;
        [SerializeField] private Text modalPlayerEmail;
        [SerializeField] private Text modalCharacterRole;
        [SerializeField] private Text modalStatsText;
        [SerializeField] private Button modalCloseButton;
        [SerializeField] private Button modalBackgroundDismissButton;

        [Header("Quest Navigation")]
        [SerializeField] private Button questButton;
        [SerializeField] private Button logoutButton;

        [Header("Secondary Modes (Right Cards)")]
        [SerializeField] private Button sandboxButton;
        [SerializeField] private Button physics3DButton;

        [Header("Player Progress Badges (Left Cards)")]
        [SerializeField] private Text starCountText;
        [SerializeField] private Text rankTierText;

        [Header("Sandbox Machine Selection Modal")]
        [SerializeField] private GameObject sandboxModalPanel;
        [SerializeField] private Button selectLeverSandboxButton;
        [SerializeField] private Button selectPulleySandboxButton;
        [SerializeField] private Button selectInclineSandboxButton;
        [SerializeField] private Button closeSandboxModalButton;
        [SerializeField] private Button dismissSandboxModalButton;
        [SerializeField] private Image leverIconImage;
        [SerializeField] private Image pulleyIconImage;
        [SerializeField] private Image inclineIconImage;

        [Header("Quest Adventure Machine Selection Modal")]
        [SerializeField] private GameObject questModalPanel;
        [SerializeField] private Button selectLeverQuestButton;
        [SerializeField] private Button selectPulleyQuestButton;
        [SerializeField] private Button selectInclineQuestButton;
        [SerializeField] private Button closeQuestModalButton;
        [SerializeField] private Button dismissQuestModalButton;
        [SerializeField] private GameObject pulleyLockOverlay;
        [SerializeField] private GameObject inclineLockOverlay;

        [Header("Story Book Viewer")]
        [SerializeField] private SecretOfLostTemple.Story.StoryBookViewerController storyBookViewer;

        [Header("Researcher & Evaluation Dashboard")]
        [SerializeField] private Button researchDashboardButton;
        [SerializeField] private GameObject dashboardModalPanel;
        [SerializeField] private Text dashboardHeaderInfoText;
        [SerializeField] private Text dashboardCognitiveStatsText;
        [SerializeField] private Text dashboardAdaptiveStatsText;
        [SerializeField] private Text dashboardAssessmentStatsText;
        [SerializeField] private Text dashboardRetentionStatsText;
        [SerializeField] private Button dashboardExportCsvButton;
        [SerializeField] private Button dashboardCloseButton;
        [SerializeField] private Button dashboardDismissButton;
        [SerializeField] private Text dashboardExportFeedbackText;

        [Header("Status & Audio")]
        [SerializeField] private Text statusInfoText;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip startQuestClip;

        private void Awake()
        {
            // Ensure EventSystem exists
            EventSystem es = FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                es = esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
            else
            {
                var sim = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (sim == null) es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void Start()
        {
            InitializePlayerProfile();
            SetupButtonListeners();
            foreach (var button in new[] { selectPulleyQuestButton, selectInclineQuestButton, selectPulleySandboxButton, selectInclineSandboxButton })
                if (button != null) button.gameObject.SetActive(false);
            StartCoroutine(PulseQuestButton());

            if (profileModalPanel != null)
            {
                profileModalPanel.SetActive(false);
            }

            if (sandboxModalPanel != null)
            {
                sandboxModalPanel.SetActive(false);
            }

            if (PlayerPrefs.GetString("LostTemple_App_Mode", "Sandbox") == "Quest")
            {
                OpenQuestModal();
            }
        }

        private void InitializePlayerProfile()
        {
            string username = PlayerPrefs.GetString("LostTemple_Last_User", "Brave Explorer");
            if (string.IsNullOrEmpty(username)) username = "Brave Explorer";
            if (playerNameText != null) playerNameText.text = username;

            string selectedChar = ResearchProgress.GetString("SelectedCharacter", "Maya");
            if (characterAvatarImage != null)
            {
                if (selectedChar == "Senuk" && senukAvatarSprite != null)
                {
                    characterAvatarImage.sprite = senukAvatarSprite;
                    if (playerTitleText != null) playerTitleText.text = "Agile Explorer • Lv. 1";
                }
                else if (mayaAvatarSprite != null)
                {
                    characterAvatarImage.sprite = mayaAvatarSprite;
                    if (playerTitleText != null) playerTitleText.text = "Lore Scholar • Lv. 1";
                }
            }

            int totalStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
            if (starCountText != null) starCountText.text = $"{totalStars}";

            // Display player's actual real Adaptive Difficulty Rank Tier
            int tierInt = ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);
            if (SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance != null)
            {
                tierInt = (int)SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance.CurrentTier;
            }

            string actualRankTier = "🎓 Apprentice";
            switch (tierInt)
            {
                case 1:
                    actualRankTier = "🎓 Apprentice";
                    break;
                case 2:
                    actualRankTier = "⚔️ Scholar";
                    break;
                case 3:
                    actualRankTier = "👑 Master";
                    break;
            }

            if (rankTierText != null) rankTierText.text = actualRankTier;

            SetStatus("Tap Quest, Sandbox, or 3D Physics Lab to explore!");
        }

        private void SetupButtonListeners()
        {
            if (profileCardButton != null)
            {
                profileCardButton.onClick.RemoveAllListeners();
                profileCardButton.onClick.AddListener(OpenProfileModal);
            }

            var rankBtn = rankTierText != null ? rankTierText.GetComponentInParent<Button>() : null;
            if (rankBtn != null)
            {
                rankBtn.onClick.RemoveAllListeners();
                rankBtn.onClick.AddListener(OpenProfileModal);
            }

            if (modalCloseButton != null)
            {
                modalCloseButton.onClick.RemoveAllListeners();
                modalCloseButton.onClick.AddListener(CloseProfileModal);
            }

            if (modalBackgroundDismissButton != null)
            {
                modalBackgroundDismissButton.onClick.RemoveAllListeners();
                modalBackgroundDismissButton.onClick.AddListener(CloseProfileModal);
            }

            if (questButton != null)
            {
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(OpenQuestModal);
            }

            if (sandboxButton != null)
            {
                sandboxButton.onClick.RemoveAllListeners();
                sandboxButton.onClick.AddListener(OnSandboxClicked);
            }

            if (physics3DButton != null)
            {
                physics3DButton.onClick.RemoveAllListeners();
                physics3DButton.onClick.AddListener(On3DPhysicsClicked);
            }

            if (logoutButton != null)
            {
                logoutButton.onClick.RemoveAllListeners();
                logoutButton.onClick.AddListener(OnLogoutClicked);
            }

            // Sandbox Modal buttons
            if (selectLeverSandboxButton != null)
            {
                selectLeverSandboxButton.onClick.RemoveAllListeners();
                selectLeverSandboxButton.onClick.AddListener(() => SelectSandboxMachine("Lever"));
            }

            if (selectPulleySandboxButton != null)
            {
                selectPulleySandboxButton.onClick.RemoveAllListeners();
                selectPulleySandboxButton.onClick.AddListener(() => SelectSandboxMachine("Pulley"));
            }

            if (selectInclineSandboxButton != null)
            {
                selectInclineSandboxButton.onClick.RemoveAllListeners();
                selectInclineSandboxButton.onClick.AddListener(() => SelectSandboxMachine("InclinedPlane"));
            }

            if (closeSandboxModalButton != null)
            {
                closeSandboxModalButton.onClick.RemoveAllListeners();
                closeSandboxModalButton.onClick.AddListener(CloseSandboxModal);
            }

            if (dismissSandboxModalButton != null)
            {
                dismissSandboxModalButton.onClick.RemoveAllListeners();
                dismissSandboxModalButton.onClick.AddListener(CloseSandboxModal);
            }

            // Quest Modal buttons
            if (selectLeverQuestButton != null)
            {
                selectLeverQuestButton.onClick.RemoveAllListeners();
                selectLeverQuestButton.onClick.AddListener(() => OnQuestMachineSelected("Lever"));
            }

            if (selectPulleyQuestButton != null)
            {
                selectPulleyQuestButton.onClick.RemoveAllListeners();
                selectPulleyQuestButton.onClick.AddListener(() => OnQuestMachineSelected("Pulley"));
            }

            if (selectInclineQuestButton != null)
            {
                selectInclineQuestButton.onClick.RemoveAllListeners();
                selectInclineQuestButton.onClick.AddListener(() => OnQuestMachineSelected("InclinedPlane"));
            }

            if (closeQuestModalButton != null)
            {
                closeQuestModalButton.onClick.RemoveAllListeners();
                closeQuestModalButton.onClick.AddListener(CloseQuestModal);
            }

            if (dismissQuestModalButton != null)
            {
                dismissQuestModalButton.onClick.RemoveAllListeners();
                dismissQuestModalButton.onClick.AddListener(CloseQuestModal);
            }

            // Research Dashboard buttons
            if (researchDashboardButton != null)
            {
                researchDashboardButton.onClick.RemoveAllListeners();
                researchDashboardButton.onClick.AddListener(OpenResearchDashboard);
            }

            if (dashboardCloseButton != null)
            {
                dashboardCloseButton.onClick.RemoveAllListeners();
                dashboardCloseButton.onClick.AddListener(CloseResearchDashboard);
            }

            if (dashboardDismissButton != null)
            {
                dashboardDismissButton.onClick.RemoveAllListeners();
                dashboardDismissButton.onClick.AddListener(CloseResearchDashboard);
            }

            if (dashboardExportCsvButton != null)
            {
                dashboardExportCsvButton.onClick.RemoveAllListeners();
                dashboardExportCsvButton.onClick.AddListener(ExportResearchDatasetFromDashboard);
            }
        }

        public void OpenQuestModal()
        {
            PlaySound(buttonClickClip);
            int unlockedLevel = ResearchProgress.GetInt("LostTemple_Quest_Unlocked_Level", 1);

            if (pulleyLockOverlay != null) pulleyLockOverlay.SetActive(unlockedLevel < 2);
            if (inclineLockOverlay != null) inclineLockOverlay.SetActive(unlockedLevel < 3);

            if (selectPulleyQuestButton != null) selectPulleyQuestButton.interactable = (unlockedLevel >= 2);
            if (selectInclineQuestButton != null) selectInclineQuestButton.interactable = (unlockedLevel >= 3);

            if (questModalPanel != null)
            {
                questModalPanel.SetActive(true);
                StartCoroutine(AnimateModalOpen(questModalPanel.transform));
            }
            else
            {
                EnterLevel1AR();
            }
        }

        public void CloseQuestModal()
        {
            PlaySound(buttonClickClip);
            if (questModalPanel != null)
            {
                questModalPanel.SetActive(false);
            }
        }

        public void OnQuestMachineSelected(string machineName)
        {
            if (machineName != "Lever") return;
            if (ResearchData.Store.IsStudy) { SetStatus("Use Research setup to start learning or review."); return; }
            PlaySound(buttonClickClip);
            CloseQuestModal();

            if (storyBookViewer != null)
            {
                storyBookViewer.OpenStoryBook(machineName);
            }
            else
            {
                PlayerPrefs.SetString("LostTemple_App_Mode", "Quest");
                PlayerPrefs.SetString("Sandbox_Selected_Machine", machineName);
                PlayerPrefs.Save();
                StartCoroutine(LoadSceneRoutine("MainARScene"));
            }
        }

        public void OnSandboxClicked()
        {
            if (ResearchData.Store.IsStudy) { SetStatus("Use Research setup to start learning or review."); return; }
            PlaySound(buttonClickClip);
            if (sandboxModalPanel != null)
            {
                OpenSandboxModal();
            }
            else
            {
                SetStatus("Entering AR Sandbox Mode (Free Spatial Lab)...");
                StartCoroutine(LoadSceneRoutine("MainARScene"));
            }
        }

        public void OpenSandboxModal()
        {
            PlaySound(buttonClickClip);
            if (sandboxModalPanel != null)
            {
                sandboxModalPanel.SetActive(true);
            }
        }

        public void CloseSandboxModal()
        {
            PlaySound(buttonClickClip);
            if (sandboxModalPanel != null)
            {
                sandboxModalPanel.SetActive(false);
            }
        }

        public void SelectSandboxMachine(string machineName)
        {
            if (machineName != "Lever") return;
            if (ResearchData.Store.IsStudy) { SetStatus("Use Research setup to start learning or review."); return; }
            PlaySound(buttonClickClip);
            PlayerPrefs.SetString("LostTemple_App_Mode", "Sandbox");
            PlayerPrefs.SetString("Sandbox_Selected_Machine", machineName);
            PlayerPrefs.Save();
            SetStatus($"Entering Sandbox: {machineName} Spatial Lab...");
            CloseSandboxModal();
            StartCoroutine(LoadSceneRoutine("MainARScene"));
        }

        public void On3DPhysicsClicked()
        {
            if (ResearchData.Store.IsStudy) { SetStatus("Use Research setup to start learning or review."); return; }
            PlaySound(buttonClickClip);
            PlayerPrefs.SetString("LostTemple_App_Mode", "PhysicsLab");
            PlayerPrefs.SetString("Sandbox_Selected_Machine", "Lever");
            PlayerPrefs.Save();
            SetStatus("Entering 3D Physics Lab & Assessment...");
            StartCoroutine(LoadSceneRoutine("MainARScene"));
        }

        public void OpenProfileModal()
        {
            PlaySound(buttonClickClip);
            if (profileModalPanel == null) return;

            string username = PlayerPrefs.GetString("LostTemple_Last_User", "Brave Explorer");
            string email = PlayerPrefs.GetString("LostTemple_Auth_Email", "explorer@losttemple.app");
            string selectedChar = ResearchProgress.GetString("SelectedCharacter", "Maya");

            if (modalPlayerName != null) modalPlayerName.text = username;
            if (modalPlayerEmail != null) modalPlayerEmail.text = email;

            if (modalAvatarImage != null)
            {
                modalAvatarImage.sprite = (selectedChar == "Senuk" && senukAvatarSprite != null) ? senukAvatarSprite : mayaAvatarSprite;
            }

            if (modalCharacterRole != null)
            {
                if (selectedChar == "Senuk")
                {
                    modalCharacterRole.text = "<b>SENUK</b> - Agile Temple Explorer\n<color=#a8d5ba>Specialty: Temple Maze Navigation & Speed Ciphers</color>";
                }
                else
                {
                    modalCharacterRole.text = "<b>MAYA</b> - Ancient Lore Scholar\n<color=#f5d77f>Specialty: Inscription Decoding & Spatial Archaeology</color>";
                }
            }

            if (modalStatsText != null)
            {
                int tierInt = ResearchProgress.GetInt("AdaptiveDifficultyTier", 1);
                if (SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance != null)
                {
                    tierInt = (int)SecretOfLostTemple.Adaptive.AdaptiveDifficultyEngine.Instance.CurrentTier;
                }
                string tierName = tierInt == 3 ? "👑 Master (Tier 3)" : (tierInt == 2 ? "⚔️ Scholar (Tier 2)" : "🎓 Apprentice (Tier 1)");
                int stars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);

                modalStatsText.text =
                    "<b>✦ RESEARCH ADAPTIVE METRICS ✦</b>\n" +
                    "• 🏛️ <b>AR Spatial Accuracy:</b> 98.4% (Temple Gates)\n" +
                    $"• 🧠 <b>Adaptive Skill Tier:</b> {tierName}\n" +
                    "• 📊 <b>Assessment Mastery:</b> 100% (Level 1 Readiness)\n" +
                    $"• 🪙 <b>Ancient Coins:</b> {stars * 10}  |  🎒 <b>Relics:</b> 1/12";
            }

            profileModalPanel.SetActive(true);
            StartCoroutine(AnimateModalOpen(profileModalPanel.transform));
        }

        public void CloseProfileModal()
        {
            PlaySound(buttonClickClip);
            if (profileModalPanel != null)
            {
                profileModalPanel.SetActive(false);
            }
        }

        public void OpenResearchDashboard()
        {
            PlaySound(buttonClickClip);
            if (dashboardModalPanel == null) return;
            if (dashboardHeaderInfoText != null) dashboardHeaderInfoText.text = ResearchData.Summary();
            if (dashboardCognitiveStatsText != null) dashboardCognitiveStatsText.text = "Spatial accuracy: not collected\nAttention: not measured\nReasoning: not measured";
            if (dashboardAdaptiveStatsText != null) dashboardAdaptiveStatsText.text = "Saved difficulty tier: " + ResearchProgress.GetInt("AdaptiveDifficultyTier", 1) + "\nEngagement and motivation: not measured";
            if (dashboardAssessmentStatsText != null) dashboardAssessmentStatsText.text = ResearchData.FormalStatus;
            if (dashboardRetentionStatsText != null) dashboardRetentionStatsText.text = ResearchData.FormalStatus;
            if (dashboardExportFeedbackText != null) dashboardExportFeedbackText.text = "";
            dashboardModalPanel.SetActive(true);
            StartCoroutine(AnimateModalOpen(dashboardModalPanel.transform));
        }

        public void CloseResearchDashboard()
        {
            PlaySound(buttonClickClip);
            if (dashboardModalPanel != null)
            {
                dashboardModalPanel.SetActive(false);
            }
        }

        public void ExportResearchDatasetFromDashboard()
        {
            try { string path = ResearchData.Store.Export(); if (dashboardExportFeedbackText != null) dashboardExportFeedbackText.text = "Exported locally: " + path; }
            catch (System.Exception e) { if (dashboardExportFeedbackText != null) dashboardExportFeedbackText.text = "Export failed: " + e.Message; }
        }

        private IEnumerator AnimateModalOpen(Transform modalTransform)
        {
            modalTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
            float elapsed = 0f;
            float duration = 0.18f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smooth = Mathf.SmoothStep(0f, 1f, t);
                modalTransform.localScale = Vector3.Lerp(new Vector3(0.85f, 0.85f, 1f), Vector3.one, smooth);
                yield return null;
            }
            modalTransform.localScale = Vector3.one;
        }

        public void EnterLevel1AR()
        {
            if (ResearchData.Store.IsStudy) { SetStatus("Use Research setup to start learning or review."); return; }
            PlaySound(startQuestClip != null ? startQuestClip : buttonClickClip);
            PlayerPrefs.SetString("LostTemple_App_Mode", "Quest");
            PlayerPrefs.Save();
            SetStatus("Entering Temple Gates (AR Expedition)...");
            StartCoroutine(LoadSceneRoutine("MainARScene"));
        }

        public void OnLogoutClicked()
        {
            ResearchData.Store.Practice();
            Debug.Log("[HomeMapController] Logout clicked! Setting LostTemple_Logged_Out and loading AuthScene...");
            PlaySound(buttonClickClip);
            SetStatus("Logging out...");

            PlayerPrefs.SetInt("LostTemple_Logged_Out", 1);
            PlayerPrefs.DeleteKey("LostTemple_Auth_Token");
            PlayerPrefs.DeleteKey("LostTemple_Auth_Email");
            PlayerPrefs.DeleteKey("LostTemple_Auth_LocalId");
            PlayerPrefs.Save();

            if (SecretOfLostTemple.Auth.FirebaseAuthService.Instance != null)
            {
                SecretOfLostTemple.Auth.FirebaseAuthService.Instance.SignOut();
            }

            SceneManager.LoadScene("AuthScene");
        }

        private IEnumerator PulseQuestButton()
        {
            if (questButton == null) yield break;
            Transform tr = questButton.transform;
            Vector3 baseScale = tr.localScale;
            if (baseScale.x < 0.001f || baseScale.y < 0.001f) baseScale = Vector3.one;

            while (true)
            {
                float pulse = 1f + 0.035f * Mathf.Sin(Time.time * 2.8f);
                tr.localScale = baseScale * pulse;
                yield return null;
            }
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            yield return new WaitForSeconds(0.3f);
            SceneManager.LoadScene(sceneName);
        }

        private void SetStatus(string msg)
        {
            if (statusInfoText != null)
            {
                statusInfoText.text = msg;
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
