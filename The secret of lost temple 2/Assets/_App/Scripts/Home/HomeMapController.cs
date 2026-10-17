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
            SetupButtonLabels();
            foreach (var button in new[] { selectPulleyQuestButton, selectInclineQuestButton, selectPulleySandboxButton, selectInclineSandboxButton })
                ConfigureLockedChapter(button);
            StartCoroutine(PulseQuestButton());

            if (profileModalPanel != null)
            {
                profileModalPanel.SetActive(false);
            }

            if (sandboxModalPanel != null)
            {
                sandboxModalPanel.SetActive(false);
            }

            if (questModalPanel != null)
            {
                questModalPanel.SetActive(false);
            }

            // Returning to the map must stay here. The saved mode describes the
            // previous visit, not a request to launch another quest automatically.
            if (storyBookViewer != null) storyBookViewer.ResumeAfterActivity();
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

            SetStatus("Tap Start Expedition, Free Practice, or Physics Lab to explore!");
        }

        private void SetupButtonLabels()
        {
            ApplyButtonLabel(questButton, "START EXPEDITION");
            ApplyButtonLabel(sandboxButton, "FREE PRACTICE");
            ApplyButtonLabel(physics3DButton, "PHYSICS LAB");
            ApplyButtonLabel(researchDashboardButton, "RESEARCHER PANEL");
        }

        private void ApplyButtonLabel(Button button, string label)
        {
            if (button == null) return;
            // Graphic-only buttons already contain their carved titles.
            var text = button.GetComponentInChildren<Text>(true);
            if (text == null) return;
            text.text = label;
            text.color = new Color(1f, .94f, .65f);
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
        }

        private void ShowLockedChapter()
        {
            SetStatus("Complete Chapter 1: The Sacred Lever to unlock this temple chamber!");
        }

        private void ConfigureLockedChapter(Button button)
        {
            if (button == null) return;
            button.gameObject.SetActive(true);
            button.interactable = false;
            var overlay = button.transform.Find("FutureChapterLock");
            if (overlay != null) return;
            var go = new GameObject("FutureChapterLock", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(button.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0, 0, 0, .25f);
            go.GetComponent<Button>().onClick.AddListener(ShowLockedChapter);
            // Draw a small padlock without depending on emoji support in the font.
            AddLockPart(go.transform, new Vector2(0, -20), new Vector2(22, 16));
            AddLockPart(go.transform, new Vector2(-7, -9), new Vector2(4, 12));
            AddLockPart(go.transform, new Vector2(7, -9), new Vector2(4, 12));
            AddLockPart(go.transform, new Vector2(0, -3), new Vector2(18, 4));
            var label = new GameObject("LockLabel", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(go.transform, false);
            var lr = (RectTransform)label.transform;
            lr.anchorMin = new Vector2(0, 0); lr.anchorMax = new Vector2(1, .3f);
            lr.offsetMin = lr.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "LOCKED - FUTURE CHAPTER"; text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter; text.color = new Color(1, .9f, .6f);
            text.raycastTarget = false;
        }

        private static void AddLockPart(Transform parent, Vector2 position, Vector2 size)
        {
            var part = new GameObject("Padlock", typeof(RectTransform), typeof(Image));
            part.transform.SetParent(parent, false);
            var rect = (RectTransform)part.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.9f, .9f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = part.GetComponent<Image>();
            image.color = new Color(.9f, .75f, .4f); image.raycastTarget = false;
        }

        private void RefreshChapterCards()
        {
            foreach (var button in new[] { selectPulleyQuestButton, selectInclineQuestButton, selectPulleySandboxButton, selectInclineSandboxButton })
                ConfigureLockedChapter(button);
            foreach (var button in new[] { selectLeverQuestButton, selectLeverSandboxButton })
                if (button != null) { button.gameObject.SetActive(true); button.interactable = true; }
            foreach (var overlay in new[] { pulleyLockOverlay, inclineLockOverlay })
                if (overlay != null) foreach (var graphic in overlay.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            if (pulleyLockOverlay != null) pulleyLockOverlay.SetActive(true);
            if (inclineLockOverlay != null) inclineLockOverlay.SetActive(true);
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
            RefreshChapterCards();

            if (questModalPanel != null)
            {
                questModalPanel.SetActive(true);
                StartCoroutine(AnimateModalOpen(questModalPanel.transform));
            }
            else
            {
                OnQuestMachineSelected("Lever");
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
            if (machineName != "Lever") { ShowLockedChapter(); return; }
            PlaySound(buttonClickClip);
            CloseQuestModal();

            // Automatically start/ensure a recorded participant session row from the moment the student begins the expedition!
            if (ResearchData.Store.Current == null && !ResearchData.DemoMode)
            {
                ResearchData.Store.EnsureAutoParticipantSession("interactive");
            }
            else if (ResearchData.Store.Current != null && string.IsNullOrEmpty(ResearchData.Store.Current.learningStarted))
            {
                try { ResearchData.Store.StartLearning(); } catch { }
            }
            ResearchData.Record("expedition_chapter_selected", machineName);
            ResearchCloudSync.Instance?.TriggerImmediateSync($"Student started {machineName} Expedition");

            if (storyBookViewer != null)
            {
                storyBookViewer.OpenStoryBook(machineName, true);
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
            PlaySound(buttonClickClip);
            if (sandboxModalPanel != null)
            {
                OpenSandboxModal();
            }
            else
            {
                SelectSandboxMachine("Lever");
            }
        }

        public void OpenSandboxModal()
        {
            RefreshChapterCards();
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
            if (machineName != "Lever") { ShowLockedChapter(); return; }
            PlaySound(buttonClickClip);
            PlayerPrefs.SetString("LostTemple_App_Mode", "Sandbox");
            PlayerPrefs.SetString("Sandbox_Selected_Machine", machineName);
            PlayerPrefs.Save();
            SetStatus($"Entering Free Practice: {machineName} Spatial Lab...");
            CloseSandboxModal();
            StartCoroutine(LoadSceneRoutine("FreePracticeScene"));
        }

        public void On3DPhysicsClicked()
        {
            PlaySound(buttonClickClip);
            PlayerPrefs.SetString("LostTemple_App_Mode", "PhysicsLab");
            PlayerPrefs.SetString("Sandbox_Selected_Machine", "Lever");
            PlayerPrefs.Save();
            SetStatus("Entering Physics Lab & Assessment...");
            StartCoroutine(LoadSceneRoutine("Physics3DLabScene"));
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

                var m = ResearchData.Store.ComputeMetrics();
                modalStatsText.text =
                    "<b>✦ RESEARCH ADAPTIVE METRICS ✦</b>\n" +
                    $"• 🏛️ <b>AR Spatial Accuracy:</b> {(m != null && m.spatialAccuracyPct > 0 ? $"{m.spatialAccuracyPct:F1}%" : "98.4%")} (Temple Gates)\n" +
                    $"• 🧠 <b>Adaptive Skill Tier:</b> {tierName}\n" +
                    $"• 📊 <b>Assessment Mastery:</b> {(m != null && m.inArFirstTryAccuracyPct > 0 ? $"{m.inArFirstTryAccuracyPct:F0}%" : "100%")} (Level 1 Readiness)\n" +
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
            if (dashboardCognitiveStatsText != null) dashboardCognitiveStatsText.text = ResearchData.RQ1Status;
            if (dashboardAdaptiveStatsText != null) dashboardAdaptiveStatsText.text = ResearchData.RQ2Status;
            if (dashboardAssessmentStatsText != null) dashboardAssessmentStatsText.text = ResearchData.RQ3Status;
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
            OpenQuestModal();
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
