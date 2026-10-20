using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SecretOfLostTemple.Story
{
    public class StoryBookViewerController : MonoBehaviour
    {
        public static StoryBookViewerController Instance { get; private set; }
        public static bool StoryActivityActive { get; private set; }
        public static Sprite ActiveTargetPageSprite { get; private set; }
        static bool resumeRequested;
        static int resumePage;
        static string resumeMachine = "Lever";

        public static void ReturnToStory(bool continueAfterActivity)
        {
            if (continueAfterActivity) resumePage = 9;
            else resumePage = 8;
            resumeRequested = true;
            StoryActivityActive = false;
            SceneManager.LoadScene("HomeMapScene");
        }

        public void ResumeAfterActivity()
        {
            if (!resumeRequested) return;
            resumeRequested = false;
            OpenStoryBook(resumeMachine);
            currentPageIndex = Mathf.Clamp(resumePage, startPageIndex, endPageIndex);
            DisplayCurrentPage(false);
        }
        [Header("UIDisplayComponents")]
        [SerializeField] private GameObject storyBookRootPanel;
        [SerializeField] private Image pageDisplayImage;
        [SerializeField] private Text pageIndicatorText;
        [SerializeField] private Text chapterTitleText;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button closeBookButton;
        [SerializeField] private Button startARActivityButton;
        [SerializeField] private Text startARActivityButtonText;

        [Header("StoryBook Custom Button and Background Sprites")]
        [SerializeField] private Sprite mapButtonSprite;
        [SerializeField] private Sprite prevButtonSprite;
        [SerializeField] private Sprite nextButtonSprite;
        [SerializeField] private Sprite ancientBgSprite;

        [Header("StoryPageSprites")]
        [SerializeField] private List<Sprite> allStoryPages = new List<Sprite>();

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pageTurnClip;
        [SerializeField] private AudioClip activityStartClip;

        private string activeMachine = "Lever";
        private int startPageIndex = 0;
        private int endPageIndex = 8;
        private int currentPageIndex = 0;
        private Coroutine flipCoroutine;

        public int CurrentPageIndex => currentPageIndex;

        public void GoToPage(int pageIndex)
        {
            currentPageIndex = Mathf.Clamp(pageIndex, startPageIndex, endPageIndex);
            DisplayCurrentPage(false);
        }

        private void Awake()
        {
            Instance = this;
            if (GetComponent<StoryActivitiesManager>() == null)
            {
                gameObject.AddComponent<StoryActivitiesManager>();
            }
            SetupListeners();
            EnhanceButtonStyling();
        }

        private void Start()
        {
            EnhanceButtonStyling();
            ResumeAfterActivity();
        }

        private void OnEnable()
        {
            SetupListeners();
            EnhanceButtonStyling();
        }

        private void SetupListeners()
        {
            if (prevPageButton != null)
            {
                prevPageButton.onClick.RemoveAllListeners();
                prevPageButton.onClick.AddListener(OnPrevPageClicked);
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveAllListeners();
                nextPageButton.onClick.AddListener(OnNextPageClicked);
            }

            if (closeBookButton != null)
            {
                closeBookButton.onClick.RemoveAllListeners();
                closeBookButton.onClick.AddListener(CloseStoryBook);
            }

            if (startARActivityButton != null)
            {
                startARActivityButton.onClick.RemoveAllListeners();
                startARActivityButton.onClick.AddListener(OnStartARActivityClicked);
            }
        }

        private void EnhanceButtonStyling()
        {
            var mapSprite = mapButtonSprite != null ? mapButtonSprite : Resources.Load<Sprite>("btn_storybook_map");
            var prevSprite = prevButtonSprite != null ? prevButtonSprite : Resources.Load<Sprite>("btn_storybook_prev");
            var nextSprite = nextButtonSprite != null ? nextButtonSprite : Resources.Load<Sprite>("btn_storybook_next");

            if (storyBookRootPanel != null)
            {
                var topBarTr = storyBookRootPanel.transform.Find("TopBar");
                if (topBarTr != null)
                {
                    var topBarRt = topBarTr.GetComponent<RectTransform>();
                    if (topBarRt != null)
                    {
                        topBarRt.sizeDelta = new Vector2(0f, 130f);
                    }
                    var topBarImg = topBarTr.GetComponent<Image>();
                    if (topBarImg != null)
                    {
                        topBarImg.enabled = false;
                    }
                }

                var bottomNavTr = storyBookRootPanel.transform.Find("BottomNavBar");
                if (bottomNavTr != null)
                {
                    var navRt = bottomNavTr.GetComponent<RectTransform>();
                    if (navRt != null)
                    {
                        navRt.sizeDelta = new Vector2(1400f, 130f);
                    }
                }
            }

            if (closeBookButton != null)
            {
                ApplySpriteButton(closeBookButton, mapSprite, new Vector2(300f, 115f));
                var mapRt = closeBookButton.GetComponent<RectTransform>();
                if (mapRt != null)
                {
                    mapRt.anchoredPosition = new Vector2(175f, 0f);
                }
            }

            if (prevPageButton != null)
            {
                ApplySpriteButton(prevPageButton, prevSprite, new Vector2(350f, 115f));
                var prevRt = prevPageButton.GetComponent<RectTransform>();
                if (prevRt != null)
                {
                    prevRt.anchoredPosition = new Vector2(185f, 0f);
                }
            }

            if (nextPageButton != null)
            {
                ApplySpriteButton(nextPageButton, nextSprite, new Vector2(295f, 115f));
                var nextRt = nextPageButton.GetComponent<RectTransform>();
                if (nextRt != null)
                {
                    nextRt.anchoredPosition = new Vector2(-160f, 0f);
                }
            }

            if (pageIndicatorText != null)
            {
                pageIndicatorText.fontSize = 28;
                pageIndicatorText.color = new Color(1f, 0.92f, 0.68f);
                var shadow = pageIndicatorText.GetComponent<Shadow>() ?? pageIndicatorText.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                shadow.effectDistance = new Vector2(1.5f, -1.5f);
            }

            if (chapterTitleText != null)
            {
                chapterTitleText.text = "";
                chapterTitleText.gameObject.SetActive(false);
            }

            EnsureAncientBackdrop();
            EnsurePageBorder();
        }

        private void EnsurePageBorder()
        {
            if (pageDisplayImage == null) return;

            Transform parentTr = pageDisplayImage.transform.parent;
            if (parentTr == null) return;

            Transform borderTr = parentTr.Find("PageBorderFrame");
            GameObject borderObj;
            if (borderTr == null)
            {
                borderObj = new GameObject("PageBorderFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                borderObj.transform.SetParent(parentTr, false);
            }
            else
            {
                borderObj = borderTr.gameObject;
            }
            borderObj.transform.SetSiblingIndex(0);

            var rt = borderObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(-8f, -8f);
                rt.offsetMax = new Vector2(8f, 8f);
                rt.localScale = Vector3.one;
            }

            var img = borderObj.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.85f, 0.68f, 0.28f, 1f);
                img.raycastTarget = false;
            }

            var outline = borderObj.GetComponent<Outline>() ?? borderObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.15f, 0.10f, 0.04f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            var shadow = borderObj.GetComponent<Shadow>() ?? borderObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(6f, -6f);

            var pageInnerOutline = pageDisplayImage.GetComponent<Outline>() ?? pageDisplayImage.gameObject.AddComponent<Outline>();
            pageInnerOutline.effectColor = new Color(0.18f, 0.12f, 0.05f, 0.9f);
            pageInnerOutline.effectDistance = new Vector2(2f, -2f);
        }

        private void ApplySpriteButton(Button btn, Sprite sprite, Vector2 size)
        {
            if (btn == null) return;

            var rt = btn.GetComponent<RectTransform>();
            if (rt != null && size != Vector2.zero)
            {
                rt.sizeDelta = size;
            }

            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (sprite != null)
                {
                    img.sprite = sprite;
                    img.type = Image.Type.Simple;
                    img.preserveAspect = true;
                }
                img.color = Color.white;
            }

            var outline = btn.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            var shadow = btn.GetComponent<Shadow>();
            if (shadow != null) shadow.enabled = false;

            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(0.65f, 0.65f, 0.65f, 0.45f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            var texts = btn.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                t.gameObject.SetActive(false);
            }
        }

        private void EnsureAncientBackdrop()
        {
            if (storyBookRootPanel == null) return;

            var bgSprite = ancientBgSprite != null ? ancientBgSprite : Resources.Load<Sprite>("bg_ancient_lever_storybook");
            if (bgSprite == null) return;

            var rootImg = storyBookRootPanel.GetComponent<Image>();
            if (rootImg != null)
            {
                rootImg.color = new Color(0.06f, 0.05f, 0.03f, 1f);
            }

            Transform backdropTr = storyBookRootPanel.transform.Find("AncientTempleBackdrop");
            GameObject backdropObj;
            if (backdropTr == null)
            {
                backdropObj = new GameObject("AncientTempleBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                backdropObj.transform.SetParent(storyBookRootPanel.transform, false);
                backdropObj.transform.SetSiblingIndex(0);
            }
            else
            {
                backdropObj = backdropTr.gameObject;
                backdropObj.transform.SetSiblingIndex(0);
            }

            var rt = backdropObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
            }

            var img = backdropObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = bgSprite;
                img.color = Color.white;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;
                img.raycastTarget = false;
            }
        }

        private void StyleAdventureButton(Button btn, string label, Color bgCol, Color borderCol)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = bgCol;
            }
            var outline = btn.GetComponent<Outline>() ?? btn.gameObject.AddComponent<Outline>();
            outline.effectColor = borderCol;
            outline.effectDistance = new Vector2(2f, -2f);

            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = $"<b>{label}</b>";
                txt.color = new Color(1f, 0.94f, 0.75f);
                var shadow = txt.GetComponent<Shadow>() ?? txt.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
                shadow.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        public void OpenStoryBook(string machineName, bool resetChallenge = false)
        {
            resumeRequested = false;
            activeMachine = machineName;

            if (resetChallenge)
            {
                PlayerPrefs.SetInt("LostTemple_Lever_Challenge_Completed", 0);
                PlayerPrefs.Save();
            }

            if (SecretOfLostTemple.Research.Research.Store.Current == null && !SecretOfLostTemple.Research.Research.DemoMode)
            {
                SecretOfLostTemple.Research.Research.Store.EnsureAutoParticipantSession("interactive");
            }
            SecretOfLostTemple.Research.Research.Record("storybook_opened", machineName);
            SecretOfLostTemple.Research.ResearchCloudSync.Instance?.TriggerImmediateSync($"Reading {machineName} StoryBook");

            if (chapterTitleText != null)
            {
                chapterTitleText.text = "";
                chapterTitleText.gameObject.SetActive(false);
            }

            if (machineName == "Lever")
            {
                startPageIndex = 0;
                endPageIndex = (allStoryPages != null && allStoryPages.Count > 0) ? (allStoryPages.Count - 1) : 17;
                if (startARActivityButtonText != null) startARActivityButtonText.text = "📸 <b>SCAN PRINTED MARKER</b>";
            }
            else if (machineName == "Pulley")
            {
                startPageIndex = 9;
                endPageIndex = 12;
                if (startARActivityButtonText != null) startARActivityButtonText.text = "📸 <b>SCAN PRINTED MARKER</b>";
            }
            else if (machineName == "InclinedPlane")
            {
                startPageIndex = 13;
                endPageIndex = 17;
                if (startARActivityButtonText != null) startARActivityButtonText.text = "📸 <b>SCAN PRINTED MARKER</b>";
            }

            currentPageIndex = startPageIndex;

            if (storyBookRootPanel != null)
            {
                storyBookRootPanel.SetActive(true);
                EnsureAncientBackdrop();
                EnhanceButtonStyling();
                var mapStatus = GameObject.Find("StatusBanner") ?? GameObject.Find("StatusInfoText");
                if (mapStatus != null) mapStatus.SetActive(false);
            }

            DisplayCurrentPage(false);
        }

        public void CloseStoryBook()
        {
            PlaySound(pageTurnClip);
            if (storyBookRootPanel != null)
            {
                storyBookRootPanel.SetActive(false);
                var mapStatus = GameObject.Find("StatusBanner") ?? GameObject.Find("StatusInfoText");
                if (mapStatus != null) mapStatus.SetActive(true);
            }
        }

        public void OnNextPageClicked()
        {
            if (CloudTransitionController.Instance != null && CloudTransitionController.Instance.IsTransitioning) return;

            // Mini-activity gates for Lever chapter
            if (activeMachine == "Lever")
            {
                // Page 2 -> Activity 1 (Pack Explorer's Backpack)
                if (currentPageIndex == 1 && !StoryActivitiesManager.IsBackpackCompleted)
                {
                    if (StoryActivitiesManager.Instance != null)
                    {
                        StoryActivitiesManager.Instance.OpenBackpackActivity();
                        return;
                    }
                }

                // Page 7 -> Activity 2 (Assemble Temple Lever)
                if (currentPageIndex == 6 && !StoryActivitiesManager.IsLeverAssemblyCompleted)
                {
                    if (StoryActivitiesManager.Instance != null)
                    {
                        StoryActivitiesManager.Instance.OpenLeverAssemblyActivity();
                        return;
                    }
                }
            }

            bool isLeverChallengeCompleted = PlayerPrefs.GetInt("LostTemple_Lever_Challenge_Completed", 0) == 1;
            if (activeMachine == "Lever" && currentPageIndex >= 8 && !isLeverChallengeCompleted)
            {
                // Challenge gate: cannot advance past marker page until challenge is completed in AR
                return;
            }

            if (currentPageIndex < endPageIndex)
            {
                int targetPage = currentPageIndex + 1;
                CloudTransitionController.Instance.PlayCloudTransition(() =>
                {
                    currentPageIndex = targetPage;
                    DisplayCurrentPage(false);
                });
            }
        }

        public void OnPrevPageClicked()
        {
            if (CloudTransitionController.Instance != null && CloudTransitionController.Instance.IsTransitioning) return;

            if (currentPageIndex > startPageIndex)
            {
                int targetPage = currentPageIndex - 1;
                CloudTransitionController.Instance.PlayCloudTransition(() =>
                {
                    currentPageIndex = targetPage;
                    DisplayCurrentPage(false);
                });
            }
        }

        private void DisplayCurrentPage(bool animate)
        {
            PlaySound(pageTurnClip);

            if (pageDisplayImage != null && allStoryPages != null && currentPageIndex >= 0 && currentPageIndex < allStoryPages.Count)
            {
                pageDisplayImage.sprite = allStoryPages[currentPageIndex];
                pageDisplayImage.color = Color.white;
                pageDisplayImage.preserveAspect = true;

                if (animate && Application.isPlaying && gameObject.activeInHierarchy)
                {
                    if (flipCoroutine != null) StopCoroutine(flipCoroutine);
                    flipCoroutine = StartCoroutine(AnimatePageFlip());
                }
                else
                {
                    pageDisplayImage.transform.localScale = Vector3.one;
                }
            }

            int relativePage = (currentPageIndex - startPageIndex) + 1;
            int totalPagesInChapter = (endPageIndex - startPageIndex) + 1;
            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = "<b>PAGE " + relativePage + " / " + totalPagesInChapter + "</b>";
            }

            if (prevPageButton != null)
            {
                prevPageButton.interactable = (currentPageIndex > startPageIndex);
            }

            bool isLeverChallengeCompleted = PlayerPrefs.GetInt("LostTemple_Lever_Challenge_Completed", 0) == 1;
            bool canGoNext = currentPageIndex < endPageIndex;
            if (activeMachine == "Lever" && currentPageIndex >= 8 && !isLeverChallengeCompleted)
            {
                canGoNext = false;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = canGoNext;
            }

            bool isScanMarkerPage = activeMachine == "Lever" ? (currentPageIndex == 8) : currentPageIndex == endPageIndex;
            if (startARActivityButtonText != null) startARActivityButtonText.text = "📸 <b>SCAN PRINTED MARKER</b>";
            if (startARActivityButton != null)
            {
                startARActivityButton.gameObject.SetActive(isScanMarkerPage);
            }

            UpdateActivityPromptButton();
        }

        private GameObject activityPromptButtonObj;

        private void UpdateActivityPromptButton()
        {
            bool showPrompt = activeMachine == "Lever" && (currentPageIndex == 1 || currentPageIndex == 6);
            if (!showPrompt)
            {
                if (activityPromptButtonObj != null) activityPromptButtonObj.SetActive(false);
                return;
            }

            if (activityPromptButtonObj == null && storyBookRootPanel != null)
            {
                activityPromptButtonObj = new GameObject("StoryActivityPromptButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                activityPromptButtonObj.transform.SetParent(storyBookRootPanel.transform, false);

                RectTransform rt = activityPromptButtonObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 138f); // nicely above the bottom navigation bar
                rt.sizeDelta = new Vector2(400f, 54f);

                Image img = activityPromptButtonObj.GetComponent<Image>();
                img.color = new Color(0.2f, 0.14f, 0.05f, 0.95f);

                Outline outline = activityPromptButtonObj.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.85f, 0.3f, 1f);
                outline.effectDistance = new Vector2(2.5f, -2.5f);

                Shadow shadow = activityPromptButtonObj.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                shadow.effectDistance = new Vector2(3f, -3f);

                GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                labelObj.transform.SetParent(activityPromptButtonObj.transform, false);
                RectTransform lRt = labelObj.GetComponent<RectTransform>();
                lRt.anchorMin = Vector2.zero;
                lRt.anchorMax = Vector2.one;
                lRt.offsetMin = Vector2.zero;
                lRt.offsetMax = Vector2.zero;

                Text txt = labelObj.GetComponent<Text>();
                txt.font = Resources.Load<Font>("MontserratFont") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = 17;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = new Color(1f, 0.92f, 0.72f);
                txt.raycastTarget = false;
            }

            if (activityPromptButtonObj != null)
            {
                activityPromptButtonObj.SetActive(true);
                activityPromptButtonObj.transform.SetAsLastSibling();

                Button btn = activityPromptButtonObj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();

                Text txt = activityPromptButtonObj.GetComponentInChildren<Text>();

                if (currentPageIndex == 1)
                {
                    bool done = StoryActivitiesManager.IsBackpackCompleted;
                    if (txt != null)
                    {
                        txt.text = done ? "🎒 <b>MINI-QUEST: REPLAY PACK BACKPACK</b>" : "🎒 <b>MINI-QUEST: PACK THE BACKPACK!</b>";
                    }
                    btn.onClick.AddListener(() =>
                    {
                        if (StoryActivitiesManager.Instance != null)
                        {
                            StoryActivitiesManager.Instance.OpenBackpackActivity();
                        }
                    });
                }
                else if (currentPageIndex == 6)
                {
                    bool done = StoryActivitiesManager.IsLeverAssemblyCompleted;
                    if (txt != null)
                    {
                        txt.text = done ? "⚙️ <b>MINI-QUEST: REPLAY ASSEMBLE LEVER</b>" : "⚙️ <b>MINI-QUEST: ASSEMBLE THE LEVER!</b>";
                    }
                    btn.onClick.AddListener(() =>
                    {
                        if (StoryActivitiesManager.Instance != null)
                        {
                            StoryActivitiesManager.Instance.OpenLeverAssemblyActivity();
                        }
                    });
                }
            }
        }

        private IEnumerator AnimatePageFlip()
        {
            if (pageDisplayImage == null) yield break;

            Transform tr = pageDisplayImage.transform;
            float duration = 0.08f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float scaleX = Mathf.Lerp(0.85f, 1f, elapsed / duration);
                tr.localScale = new Vector3(scaleX, 1f, 1f);
                yield return null;
            }
            tr.localScale = Vector3.one;
        }

        public void OnStartARActivityClicked()
        {
            StoryActivityActive = true;
            if (pageDisplayImage != null) ActiveTargetPageSprite = pageDisplayImage.sprite;
            resumePage = currentPageIndex;
            resumeMachine = activeMachine;
            PlaySound(activityStartClip != null ? activityStartClip : pageTurnClip);

            var store = SecretOfLostTemple.Research.Research.Store;
            if (store.Current == null && !SecretOfLostTemple.Research.Research.DemoMode)
            {
                store.EnsureAutoParticipantSession("interactive");
            }
            if (store.Current != null && string.IsNullOrEmpty(store.Current.learningStarted))
            {
                store.Current.learningStarted = System.DateTime.UtcNow.ToString("O");
            }
            SecretOfLostTemple.Research.Research.Record("scan_marker_button_clicked", activeMachine);
            SecretOfLostTemple.Research.ResearchCloudSync.Instance?.TriggerImmediateSync($"Scanning Page 9 Marker ({activeMachine})");

            PlayerPrefs.SetString("LostTemple_App_Mode", "Quest");
            PlayerPrefs.SetString("Sandbox_Selected_Machine", activeMachine);
            PlayerPrefs.Save();

            StartCoroutine(LoadARSceneRoutine());
        }

        private IEnumerator LoadARSceneRoutine()
        {
            yield return new WaitForSeconds(0.2f);
            SceneManager.LoadScene("MainARScene");
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
