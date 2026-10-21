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

        [Header("Audio Settings")]
        [SerializeField] private Button musicToggleButton;
        [SerializeField] private Image musicToggleIcon;
        [SerializeField] private Sprite musicOnSprite;
        [SerializeField] private Sprite musicMutedSprite;

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

        [Header("Background Video")]
        [SerializeField] private UnityEngine.Video.VideoPlayer backgroundVideoPlayer;
        [SerializeField] private RawImage backgroundRawImage;
        [SerializeField] private UnityEngine.Video.VideoClip backgroundVideoClip;

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

        private void OnEnable()
        {
            UpdateMusicButtonUI();
        }

        private void Start()
        {
            if (physics3DButton != null) physics3DButton.gameObject.SetActive(false);
            InitializePlayerProfile();
            SetupButtonListeners();
            SetupButtonLabels();
            RefreshChapterCards();
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

            EnsureQuestModalBackgroundApplied();
            InitializeBackgroundVideo();
            EnsureMusicMuteButtonBuilt();
            UpdateMusicButtonUI();
        }

        private void InitializeBackgroundVideo()
        {
            if (backgroundVideoPlayer == null)
            {
                var bg = GameObject.Find("MapBackground");
                if (bg != null)
                {
                    backgroundVideoPlayer = bg.GetComponent<UnityEngine.Video.VideoPlayer>();
                    if (backgroundRawImage == null) backgroundRawImage = bg.GetComponent<RawImage>();
                }
            }

#if UNITY_EDITOR
            if (backgroundVideoClip == null)
            {
                backgroundVideoClip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>("Assets/Art/Animations/Tropical_birds_flying_in_jungle_20261020001704.mp4");
            }
#endif

            if (backgroundVideoPlayer != null)
            {
                backgroundVideoPlayer.isLooping = true;
                backgroundVideoPlayer.playOnAwake = true;
                backgroundVideoPlayer.aspectRatio = UnityEngine.Video.VideoAspectRatio.FitOutside;
                backgroundVideoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;

                if (backgroundVideoClip != null)
                {
                    backgroundVideoPlayer.clip = backgroundVideoClip;
                }

                if (backgroundVideoPlayer.targetTexture != null && backgroundRawImage != null)
                {
                    backgroundRawImage.texture = backgroundVideoPlayer.targetTexture;
                }
                else if (backgroundVideoPlayer.targetTexture == null && backgroundRawImage != null)
                {
                    RenderTexture rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
                    rt.name = "HomeMap_Runtime_RT";
                    backgroundVideoPlayer.targetTexture = rt;
                    backgroundRawImage.texture = rt;
                }

                var audioSrc = backgroundVideoPlayer.GetComponent<AudioSource>();
                if (audioSrc != null)
                {
                    audioSrc.playOnAwake = true;
                    audioSrc.loop = true;
                    audioSrc.volume = 0.35f;
                    backgroundVideoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
                    backgroundVideoPlayer.EnableAudioTrack(0, true);
                    backgroundVideoPlayer.SetTargetAudioSource(0, audioSrc);
                }

                if (!backgroundVideoPlayer.isPlaying)
                {
                    backgroundVideoPlayer.Play();
                }
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

            string actualRankTier = "🎒 Explorer";
            switch (tierInt)
            {
                case 1:
                    actualRankTier = "🎒 Explorer";
                    break;
                case 2:
                    actualRankTier = "⚡ Expert";
                    break;
                case 3:
                    actualRankTier = "🏆 Champion";
                    break;
            }

            if (rankTierText != null) rankTierText.text = actualRankTier;

            SetStatus("Tap Start Expedition or Sandbox to explore!");
        }

        private void SetupButtonLabels()
        {
            Font montserrat = Resources.Load<Font>("MontserratFont");

            ApplyCarvedGoldenButtonLabel(questButton, "START EXPEDITION", 38, montserrat);
            ApplyCarvedGoldenButtonLabel(sandboxButton, "FREE PRACTICE", 36, montserrat);
            if (physics3DButton != null) ApplyCarvedGoldenButtonLabel(physics3DButton, "PHYSICS LAB", 36, montserrat);
            ApplyCarvedGoldenButtonLabel(researchDashboardButton, "RESEARCHER PANEL", 24, montserrat);

            // Apply to Chapter buttons (e.g. Lever AR)
            if (selectLeverQuestButton != null)
                ApplyCarvedGoldenButtonLabel(selectLeverQuestButton, "LEVER AR", 34, montserrat);
            if (selectLeverSandboxButton != null)
                ApplyCarvedGoldenButtonLabel(selectLeverSandboxButton, "LEVER AR", 34, montserrat);

            // Top HUD Profile Card
            if (playerNameText != null)
            {
                if (montserrat != null) playerNameText.font = montserrat;
                playerNameText.fontSize = 32; // 30-34px bold
                playerNameText.fontStyle = FontStyle.Bold;
            }

            if (playerTitleText != null)
            {
                if (montserrat != null) playerTitleText.font = montserrat;
                playerTitleText.fontSize = 22; // bold
                playerTitleText.fontStyle = FontStyle.Bold;
            }

            // Star & Rank Badges
            if (starCountText != null)
            {
                if (montserrat != null) starCountText.font = montserrat;
                starCountText.fontSize = 32; // 30-34px bold
                starCountText.fontStyle = FontStyle.Bold;
            }

            if (rankTierText != null)
            {
                if (montserrat != null) rankTierText.font = montserrat;
                rankTierText.fontSize = 30; // 30-34px bold
                rankTierText.fontStyle = FontStyle.Bold;
            }

            // Status text / instructions (28-32px bold)
            if (statusInfoText != null)
            {
                if (montserrat != null) statusInfoText.font = montserrat;
                statusInfoText.fontSize = 28;
                statusInfoText.fontStyle = FontStyle.Bold;
                statusInfoText.color = new Color(1f, 0.949f, 0.659f, 1f);
                var stOutline = statusInfoText.GetComponent<Outline>() ?? statusInfoText.gameObject.AddComponent<Outline>();
                stOutline.effectColor = new Color(0.165f, 0.086f, 0.031f, 1f);
                stOutline.effectDistance = new Vector2(2f, -2f);
                var stShadow = statusInfoText.GetComponent<Shadow>() ?? statusInfoText.gameObject.AddComponent<Shadow>();
                stShadow.effectColor = new Color(0.043f, 0.020f, 0.008f, 0.85f);
                stShadow.effectDistance = new Vector2(3f, -4f);
            }

            // Apply Montserrat & 3D Carved effect to Modal Headers
            ApplyModalHeaderTypography(questModalPanel, montserrat);
            ApplyModalHeaderTypography(profileModalPanel, montserrat);
            ApplyModalHeaderTypography(sandboxModalPanel, montserrat);
            ApplyModalHeaderTypography(dashboardModalPanel, montserrat);
        }

        private void ApplyModalHeaderTypography(GameObject modalPanel, Font montserrat)
        {
            if (modalPanel == null) return;
            var texts = modalPanel.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.gameObject.name.IndexOf("Title", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    t.gameObject.name.IndexOf("Header", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ApplyGolden3DCarvedEffect(t, 34, montserrat);
                }
            }
        }

        public static void ApplyGolden3DCarvedEffect(Text text, int fontSize, Font customFont)
        {
            if (text == null) return;
            if (customFont != null) text.font = customFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 0.949f, 0.659f, 1f); // #FFF2A8 Rich Warm Gold

            // Attach Outline: Color #2A1608 (Dark carved border), Effect Distance (2f, -2f)
            var outline = text.GetComponent<Outline>();
            if (outline == null) outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.165f, 0.086f, 0.031f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Attach Shadow: Color #0B0502 with 85% alpha (Deep 3D cast shadow), Effect Distance (4f, -5f)
            var shadow = text.GetComponent<Shadow>();
            if (shadow == null) shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.043f, 0.020f, 0.008f, 0.85f);
            shadow.effectDistance = new Vector2(4f, -5f);
        }

        private void ApplyCarvedGoldenButtonLabel(Button button, string label, int fontSize, Font customFont)
        {
            if (button == null) return;
            var text = button.GetComponentInChildren<Text>(true);
            if (text == null)
            {
                var labelObj = new GameObject("LabelText", typeof(RectTransform), typeof(Text));
                labelObj.transform.SetParent(button.transform, false);
                var rt = (RectTransform)labelObj.transform;
                rt.anchorMin = new Vector2(0.05f, 0.03f);
                rt.anchorMax = new Vector2(0.95f, 0.28f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                text = labelObj.GetComponent<Text>();
            }
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            ApplyGolden3DCarvedEffect(text, fontSize, customFont);
        }

        private void ShowLockedChapter()
        {
            SetStatus("Complete Chapter 1: The Sacred Lever to unlock this temple chamber!");
        }

        private void EnsureChapterCardsArtworkApplied()
        {
            Sprite leverSprite = Resources.Load<Sprite>("card_lever_lvl1");
            Sprite pulleySprite = Resources.Load<Sprite>("card_pulley_lvl2");
            Sprite inclineSprite = Resources.Load<Sprite>("card_incline_lvl3");

#if UNITY_EDITOR
            if (leverSprite == null) leverSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_lever_lvl1.png");
            if (pulleySprite == null) pulleySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_pulley_lvl2.png");
            if (inclineSprite == null) inclineSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_incline_lvl3.png");
#endif

            ApplyCardSpriteAndCleanText(selectLeverQuestButton, leverSprite);
            ApplyCardSpriteAndCleanText(selectPulleyQuestButton, pulleySprite);
            ApplyCardSpriteAndCleanText(selectInclineQuestButton, inclineSprite);
        }

        private void ApplyCardSpriteAndCleanText(Button cardButton, Sprite sprite)
        {
            if (cardButton == null) return;
            if (sprite != null)
            {
                var img = cardButton.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = sprite;
                    img.color = Color.white;
                    img.preserveAspect = true;
                }
            }

            // Hide old procedural text so the custom level artwork shows cleanly
            var txt = cardButton.transform.Find("Text")?.GetComponent<Text>();
            if (txt != null)
            {
                txt.text = "";
                txt.gameObject.SetActive(false);
            }
        }

        private void ConfigureLockedChapter(Button button, bool isLocked, string hintMessage, UnityEngine.Events.UnityAction onUnlockedClick = null)
        {
            if (button == null) return;
            button.gameObject.SetActive(true);

            // Clean up any old procedural lock boxes
            var oldProcLock = button.transform.Find("FutureChapterLock");
            if (oldProcLock != null)
            {
                if (Application.isPlaying) Destroy(oldProcLock.gameObject);
                else DestroyImmediate(oldProcLock.gameObject);
            }

            // Find or create LockOverlay
            var overlayTr = button.transform.Find("LockOverlay");
            GameObject overlayGo;
            if (overlayTr == null)
            {
                overlayGo = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image));
                overlayGo.transform.SetParent(button.transform, false);
                var rt = (RectTransform)overlayGo.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
            else
            {
                overlayGo = overlayTr.gameObject;
            }

            // Hide any old child texts or crude padlock children
            for (int i = 0; i < overlayGo.transform.childCount; i++)
            {
                overlayGo.transform.GetChild(i).gameObject.SetActive(false);
            }

            Sprite lockSprite = Resources.Load<Sprite>("card_lock");
#if UNITY_EDITOR
            if (lockSprite == null) lockSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/card_lock.png");
#endif
            var img = overlayGo.GetComponent<Image>() ?? overlayGo.AddComponent<Image>();
            if (lockSprite != null)
            {
                img.sprite = lockSprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }
            img.raycastTarget = false;

            if (isLocked)
            {
                overlayGo.SetActive(true);
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SetStatus(hintMessage));
            }
            else
            {
                overlayGo.SetActive(false);
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                if (onUnlockedClick != null)
                {
                    button.onClick.AddListener(onUnlockedClick);
                }
            }
        }

        private void RefreshChapterCards()
        {
            EnsureChapterCardsArtworkApplied();

            bool isLeverChallengeDone = PlayerPrefs.GetInt("LostTemple_Lever_Challenge_Completed", 0) == 1;
            bool isPulleyChallengeDone = PlayerPrefs.GetInt("LostTemple_Pulley_Challenge_Completed", 0) == 1;
            bool isPulleyUnlocked = isLeverChallengeDone || PlayerPrefs.GetInt("LostTemple_Pulley_Unlocked", 0) == 1;
            bool isInclineUnlocked = isPulleyChallengeDone || PlayerPrefs.GetInt("LostTemple_Incline_Unlocked", 0) == 1;

            // Lever: Always unlocked
            if (selectLeverQuestButton != null)
            {
                selectLeverQuestButton.gameObject.SetActive(true);
                selectLeverQuestButton.interactable = true;
                var leverLock = selectLeverQuestButton.transform.Find("LockOverlay");
                if (leverLock != null) leverLock.gameObject.SetActive(false);
                var oldProcLock = selectLeverQuestButton.transform.Find("FutureChapterLock");
                if (oldProcLock != null) Destroy(oldProcLock.gameObject);
                selectLeverQuestButton.onClick.RemoveAllListeners();
                selectLeverQuestButton.onClick.AddListener(() => OnQuestMachineSelected("Lever"));
            }

            if (selectLeverSandboxButton != null)
            {
                selectLeverSandboxButton.gameObject.SetActive(true);
                selectLeverSandboxButton.interactable = true;
            }

            // Pulley:
            ConfigureLockedChapter(selectPulleyQuestButton, !isPulleyUnlocked,
                "Complete Chapter 1: The Sacred Lever to unlock this temple chamber!",
                () => OnQuestMachineSelected("Pulley"));

            ConfigureLockedChapter(selectPulleySandboxButton, !isPulleyUnlocked,
                "Complete Chapter 1: The Sacred Lever to unlock this temple chamber!",
                () => SelectSandboxMachine("Pulley"));

            // Incline:
            ConfigureLockedChapter(selectInclineQuestButton, !isInclineUnlocked,
                "Complete Chapter 2: The Sacred Pulley to unlock this temple chamber!",
                () => OnQuestMachineSelected("InclinedPlane"));

            ConfigureLockedChapter(selectInclineSandboxButton, !isInclineUnlocked,
                "Complete Chapter 2: The Sacred Pulley to unlock this temple chamber!",
                () => SelectSandboxMachine("InclinedPlane"));

            if (pulleyLockOverlay != null) pulleyLockOverlay.SetActive(!isPulleyUnlocked);
            if (inclineLockOverlay != null) inclineLockOverlay.SetActive(!isInclineUnlocked);
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

        private void EnsureQuestModalBackgroundApplied()
        {
            if (questModalPanel == null) return;
            var cardCont = questModalPanel.transform.Find("CardContainer");
            if (cardCont != null)
            {
                var rtCont = cardCont.GetComponent<RectTransform>();
                if (rtCont != null)
                {
                    // Enlarge the modal banner to cover the prominent central gameplay area (1460x780)
                    rtCont.sizeDelta = new Vector2(1460f, 780f);
                    rtCont.anchoredPosition = new Vector2(0f, 15f);
                }

                var img = cardCont.GetComponent<Image>();
                if (img != null && (img.sprite == null || img.sprite.name != "adventure_quest_modal_bg"))
                {
                    Sprite s = Resources.Load<Sprite>("adventure_quest_modal_bg");
#if UNITY_EDITOR
                    if (s == null) s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/adventure_quest_modal_bg.png");
#endif
                    if (s != null)
                    {
                        img.sprite = s;
                        img.color = Color.white;
                        img.preserveAspect = true;
                    }
                }

                // Replace Title & Subtitle with Ornate Golden Select Level Logo asset
                var titleTr = cardCont.Find("Title");
                if (titleTr != null)
                {
                    // Destroy any old Text, Shadow, or Outline components so Graphic conflict is avoided
                    var oldText = titleTr.GetComponent<Text>();
                    if (oldText != null)
                    {
                        if (Application.isPlaying) Destroy(oldText);
                        else DestroyImmediate(oldText);
                    }
                    var oldShadow = titleTr.GetComponent<Shadow>();
                    if (oldShadow != null)
                    {
                        if (Application.isPlaying) Destroy(oldShadow);
                        else DestroyImmediate(oldShadow);
                    }
                    var oldOutline = titleTr.GetComponent<Outline>();
                    if (oldOutline != null)
                    {
                        if (Application.isPlaying) Destroy(oldOutline);
                        else DestroyImmediate(oldOutline);
                    }

                    // Turn into Logo Image header
                    var imgLogo = titleTr.GetComponent<Image>();
                    if (imgLogo == null) imgLogo = titleTr.gameObject.AddComponent<Image>();

                    Sprite logoSprite = Resources.Load<Sprite>("ornate_golden_select_level_logo");
#if UNITY_EDITOR
                    if (logoSprite == null) logoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/ornate_golden_select_level_logo.png");
#endif
                    if (logoSprite != null && imgLogo != null)
                    {
                        imgLogo.sprite = logoSprite;
                        imgLogo.color = Color.white;
                        imgLogo.preserveAspect = true;
                        imgLogo.enabled = true;
                    }

                    var rtTitle = titleTr.GetComponent<RectTransform>();
                    if (rtTitle != null)
                    {
                        rtTitle.anchoredPosition = new Vector2(0f, -50f);
                        rtTitle.sizeDelta = new Vector2(720f, 130f);
                    }
                }

                var subTr = cardCont.Find("Subtitle");
                if (subTr != null)
                {
                    subTr.gameObject.SetActive(false);
                }

                // Enlarge Level Cards: Lever, Pulley, Incline
                var leverTr = cardCont.Find("Card_Lever");
                if (leverTr != null)
                {
                    var rtLever = leverTr.GetComponent<RectTransform>();
                    if (rtLever != null)
                    {
                        rtLever.anchoredPosition = new Vector2(-420f, -30f);
                        rtLever.sizeDelta = new Vector2(390f, 510f);
                    }
                }

                var pulleyTr = cardCont.Find("Card_Pulley");
                if (pulleyTr != null)
                {
                    var rtPulley = pulleyTr.GetComponent<RectTransform>();
                    if (rtPulley != null)
                    {
                        rtPulley.anchoredPosition = new Vector2(0f, -30f);
                        rtPulley.sizeDelta = new Vector2(390f, 510f);
                    }
                }

                var inclineTr = cardCont.Find("Card_Incline");
                if (inclineTr != null)
                {
                    var rtIncline = inclineTr.GetComponent<RectTransform>();
                    if (rtIncline != null)
                    {
                        rtIncline.anchoredPosition = new Vector2(420f, -30f);
                        rtIncline.sizeDelta = new Vector2(390f, 510f);
                    }
                }

                // Position Close Button cleanly on top-right of enlarged scroll banner
                var closeBtnTr = cardCont.Find("Btn_Close");
                if (closeBtnTr != null)
                {
                    var rtClose = closeBtnTr.GetComponent<RectTransform>();
                    if (rtClose != null)
                    {
                        rtClose.anchoredPosition = new Vector2(-45f, -40f);
                        rtClose.sizeDelta = new Vector2(65f, 65f);
                    }
                }
            }
        }

        public void OpenQuestModal()
        {
            PlaySound(buttonClickClip);
            EnsureQuestModalBackgroundApplied();
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
            SetStatus($"Entering AR Sandbox: {machineName} Spatial Lab...");
            CloseSandboxModal();
            StartCoroutine(LoadSceneRoutine("MainARScene"));
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
                string tierName = tierInt == 3 ? "🏆 Champion (Tier 3)" : (tierInt == 2 ? "⚡ Expert (Tier 2)" : "🎒 Explorer (Tier 1)");
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
    
        private void EnsureMusicMuteButtonBuilt()
        {
            if (musicOnSprite == null)
            {
                musicOnSprite = Resources.Load<Sprite>("btn_music_on");
            }
            if (musicMutedSprite == null)
            {
                musicMutedSprite = Resources.Load<Sprite>("btn_music_off");
            }

            if (musicToggleButton == null)
            {
                GameObject existing = GameObject.Find("Button_MusicToggle");
                if (existing != null)
                {
                    musicToggleButton = existing.GetComponent<Button>();
                    musicToggleIcon = existing.GetComponent<Image>();
                }
            }

            if (musicToggleButton == null && logoutButton != null)
            {
                Transform topHUD = logoutButton.transform.parent;
                GameObject btnObj = new GameObject("Button_MusicToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(topHUD, false);

                RectTransform rt = btnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.84f, 0.5f);
                rt.anchorMax = new Vector2(0.84f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-18f, 0f);

                float targetSize = 110f;
                if (topHUD is RectTransform parentRT && parentRT.rect.height > 40f)
                {
                    targetSize = Mathf.Clamp(parentRT.rect.height * 0.85f, 75f, 130f);
                }
                rt.sizeDelta = new Vector2(targetSize, targetSize);

                musicToggleIcon = btnObj.GetComponent<Image>();
                musicToggleIcon.preserveAspect = true;
                musicToggleIcon.color = Color.white;

                musicToggleButton = btnObj.GetComponent<Button>();
                var colors = musicToggleButton.colors;
                colors.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
                colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                musicToggleButton.colors = colors;
            }

            if (musicToggleButton != null)
            {
                musicToggleButton.onClick.RemoveAllListeners();
                musicToggleButton.onClick.AddListener(OnMusicToggleClicked);
            }
        }

        private void OnMusicToggleClicked()
        {
            if (SecretOfLostTemple.Core.AudioManager.Instance != null)
            {
                SecretOfLostTemple.Core.AudioManager.Instance.ToggleBGMMute();
            }
            PlaySound(buttonClickClip);
            UpdateMusicButtonUI();
        }

        private void UpdateMusicButtonUI()
        {
            if (musicToggleIcon == null && musicToggleButton != null)
            {
                musicToggleIcon = musicToggleButton.GetComponent<Image>();
            }

            if (musicToggleIcon != null)
            {
                bool isMuted = false;
                if (SecretOfLostTemple.Core.AudioManager.Instance != null)
                {
                    isMuted = SecretOfLostTemple.Core.AudioManager.Instance.IsBGMMuted;
                }
                else
                {
                    isMuted = PlayerPrefs.GetInt("LostTemple_BGM_Muted", 0) == 1;
                }

                musicToggleIcon.sprite = isMuted ? musicMutedSprite : musicOnSprite;
            }
        }
}
}
