using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SecretOfLostTemple.Story
{
    public class StoryBookViewerController : MonoBehaviour
    {
        public static bool StoryActivityActive { get; private set; }
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

        private void Awake()
        {
            SetupListeners();
        }

        private void Start()
        {
            ResumeAfterActivity();
        }

        private void OnEnable()
        {
            SetupListeners();
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

        public void OpenStoryBook(string machineName, bool resetChallenge = false)
        {
            activeMachine = machineName;

            if (resetChallenge)
            {
                PlayerPrefs.SetInt("LostTemple_Lever_Challenge_Completed", 0);
                PlayerPrefs.Save();
            }

            if (machineName == "Lever")
            {
                startPageIndex = 0;
                endPageIndex = (allStoryPages != null && allStoryPages.Count > 0) ? (allStoryPages.Count - 1) : 17;
                if (chapterTitleText != null) chapterTitleText.text = "<b>CHAPTER 1: THE ANCIENT LEVER</b>";
                if (startARActivityButtonText != null) startARActivityButtonText.text = "📸 <b>SCAN PRINTED MARKER</b>";
            }
            else if (machineName == "Pulley")
            {
                startPageIndex = 9;
                endPageIndex = 12;
                if (chapterTitleText != null) chapterTitleText.text = "<b>CHAPTER 2: THE SACRED PULLEY</b>";
                if (startARActivityButtonText != null) startARActivityButtonText.text = "📸 <b>SCAN PRINTED MARKER</b>";
            }
            else if (machineName == "InclinedPlane")
            {
                startPageIndex = 13;
                endPageIndex = 17;
                if (chapterTitleText != null) chapterTitleText.text = "<b>CHAPTER 3: THE TEMPLE RAMP</b>";
                if (startARActivityButtonText != null) startARActivityButtonText.text = "📸 <b>SCAN PRINTED MARKER</b>";
            }

            currentPageIndex = startPageIndex;

            if (storyBookRootPanel != null)
            {
                storyBookRootPanel.SetActive(true);
            }

            DisplayCurrentPage(false);
        }

        public void CloseStoryBook()
        {
            PlaySound(pageTurnClip);
            if (storyBookRootPanel != null)
            {
                storyBookRootPanel.SetActive(false);
            }
        }

        public void OnNextPageClicked()
        {
            bool isLeverChallengeCompleted = PlayerPrefs.GetInt("LostTemple_Lever_Challenge_Completed", 0) == 1;
            if (activeMachine == "Lever" && currentPageIndex >= 8 && !isLeverChallengeCompleted)
            {
                // Challenge gate: cannot advance past marker page until challenge is completed in AR
                return;
            }

            if (currentPageIndex < endPageIndex)
            {
                currentPageIndex++;
                DisplayCurrentPage(true);
            }
        }

        public void OnPrevPageClicked()
        {
            if (currentPageIndex > startPageIndex)
            {
                currentPageIndex--;
                DisplayCurrentPage(true);
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
            resumePage = currentPageIndex;
            resumeMachine = activeMachine;
            PlaySound(activityStartClip != null ? activityStartClip : pageTurnClip);

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
