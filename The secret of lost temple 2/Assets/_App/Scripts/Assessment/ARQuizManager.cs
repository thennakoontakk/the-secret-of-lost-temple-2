using ResearchData = SecretOfLostTemple.Research.Research;
using SecretOfLostTemple.Research;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SecretOfLostTemple.Assessment
{
    [Serializable]
    public class QuestionResult
    {
        public string questionId;
        public int selectedAnswer;
        public int correctAnswer;
        public bool isCorrect;
        public float responseTimeSeconds;
        public int attemptCount;
    }

    [Serializable]
    public class QuizResult
    {
        public string quizId;
        public string userId;
        public string timestamp;
        public string quizType; // "pretest", "posttest", "contextual", "retention"
        public int totalQuestions;
        public int correctAnswers;
        public int score;
        public float timeTakenSeconds;
        public float averageResponseTime;
        public List<QuestionResult> questionResults = new List<QuestionResult>();
    }

    
    public class ARQuizManager : MonoBehaviour
    {
        public static ARQuizManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject quizPanelPrefab;
        [SerializeField] private Text questionText;
        [SerializeField] private Text explanationText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private Text[] answerTexts;
        [SerializeField] private Image progressBar;
        [SerializeField] private Text questionCounterText;

        [Header("Feedback Effects")]
        [SerializeField] private GameObject correctFeedbackEffect;
        [SerializeField] private GameObject incorrectFeedbackEffect;
        [SerializeField] private AudioClip correctSound;
        [SerializeField] private AudioClip incorrectSound;
        [SerializeField] private AudioClip quizStartSound;
        [SerializeField] private AudioClip quizCompleteSound;
        [SerializeField] private AudioSource audioSource;

        [Header("Settings")]
        [SerializeField] private int coinsPerCorrectAnswer = 10;

        // State
        private List<QuizQuestion> currentQuiz;
        private int currentQuestionIndex;
        private int score;
        private float startTime;
        private float questionStartTime;
        private QuizResult currentResult;
        private string currentQuizType;
        private bool answerSubmitted;

        // Events
        public event Action<QuizResult> OnQuizCompleted;
        public event Action<bool> OnAnswerSubmitted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void EnsureSinhalaFont()
        {
            var font = Resources.Load<Font>("SinhalaFont");
            if (font == null) return;
            if (questionText != null)
            {
                questionText.font = font;
                questionText.fontSize = 36; // 34-38px for primary school children
                questionText.lineSpacing = 1.25f; // Comfortable line spacing
            }
            if (explanationText != null)
            {
                explanationText.font = font;
                explanationText.fontSize = 30;
                explanationText.lineSpacing = 1.25f;
            }
            if (feedbackText != null)
            {
                feedbackText.font = font;
                feedbackText.fontSize = 32;
            }
            if (answerTexts != null)
            {
                foreach (var at in answerTexts)
                {
                    if (at != null)
                    {
                        at.font = font;
                        at.fontSize = 32; // 30-34px bold
                        at.fontStyle = FontStyle.Bold;
                    }
                }
            }
        }

        public void StartPreTest()
        {
            StartQuiz(QuestionBank.GetPreTestQuestions(), "pretest");
        }

        public void StartPostTest()
        {
            StartQuiz(QuestionBank.GetPostTestQuestions(), "posttest");
        }

        public void StartContextualQuiz(string machineType)
        {
            StartQuiz(QuestionBank.GetQuestionsByMachine(machineType), "contextual");
        }

        public void StartQuiz(List<QuizQuestion> questions, string quizType)
        {
            if (questions == null || questions.Count == 0) return;

            currentQuiz = questions;
            currentQuizType = quizType;
            currentQuestionIndex = 0;
            score = 0;
            startTime = Time.time;

            currentResult = new QuizResult
            {
                quizId = Guid.NewGuid().ToString(),
                userId = ResearchData.Store.ParticipantCode ?? "practice",
                timestamp = DateTime.UtcNow.ToString("o"),
                quizType = quizType,
                totalQuestions = questions.Count,
                questionResults = new List<QuestionResult>()
            };

            if (quizPanelPrefab != null) quizPanelPrefab.SetActive(true);
            if (audioSource != null && quizStartSound != null) audioSource.PlayOneShot(quizStartSound);

            ShowQuestion(currentQuestionIndex);
        }

        private void ShowQuestion(int index)
        {
            if (index >= currentQuiz.Count)
            {
                CompleteQuiz();
                return;
            }

            questionStartTime = Time.time;
            answerSubmitted = false;
            EnsureSinhalaFont();
            QuizQuestion q = currentQuiz[index];
            questionText.text = SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess($"{q.templeContext}\n\n{q.questionText}");
            explanationText.text = "";
            feedbackText.text = "";
            scoreText.text = $"Score: {score}";
            questionCounterText.text = $"{index + 1}/{currentQuiz.Count}";
            progressBar.fillAmount = (float)index / currentQuiz.Count;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < q.options.Count)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerButtons[i].interactable = true;
                    answerButtons[i].transform.localScale = Vector3.one;

                    var btnImg = answerButtons[i].GetComponent<Image>();
                    if (btnImg != null) btnImg.color = new Color(0.18f, 0.14f, 0.10f, 0.95f);

                    if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                    {
                        answerTexts[i].text = SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(q.options[i]);
                        answerTexts[i].color = Color.white;
                    }

                    int answerIndex = i; // capture for closure
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void OnAnswerSelected(int answerIndex)
        {
            if (answerSubmitted || currentQuiz == null || currentQuestionIndex >= currentQuiz.Count) return;
            answerSubmitted = true;
            QuizQuestion q = currentQuiz[currentQuestionIndex];
            bool isCorrect = (answerIndex == q.correctAnswerIndex);
            float responseTime = Time.time - questionStartTime;
            ResearchData.Record("draft_quiz_answer", currentQuizType + ":" + q.questionId + ":selected=" + answerIndex, true, isCorrect, responseTime);

            // Visual feedback on buttons
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] == null) continue;
                answerButtons[i].interactable = false;
                var btnImg = answerButtons[i].GetComponent<Image>();

                if (i == q.correctAnswerIndex)
                {
                    if (btnImg != null) btnImg.color = new Color(0.12f, 0.78f, 0.35f, 1f); // Vivid Emerald Green
                    if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                    {
                        answerTexts[i].text = $"<b>✓ {SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(q.options[i])}</b>";
                        answerTexts[i].color = Color.white;
                    }
                    if (isCorrect) StartCoroutine(PulseButton(answerButtons[i].transform));
                }
                else if (i == answerIndex && !isCorrect)
                {
                    if (btnImg != null) btnImg.color = new Color(0.85f, 0.22f, 0.22f, 1f); // Vivid Coral Red
                    if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                    {
                        answerTexts[i].text = $"<b>✗ {SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(q.options[i])}</b>";
                        answerTexts[i].color = new Color(1f, 0.9f, 0.9f, 1f);
                    }
                    StartCoroutine(ShakeButton(answerButtons[i].transform));
                }
                else
                {
                    if (btnImg != null) btnImg.color = new Color(0.12f, 0.10f, 0.08f, 0.5f);
                    if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                    {
                        answerTexts[i].color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                    }
                }
            }

            currentResult.questionResults.Add(new QuestionResult
            {
                questionId = q.questionId,
                selectedAnswer = answerIndex,
                correctAnswer = q.correctAnswerIndex,
                isCorrect = isCorrect,
                responseTimeSeconds = responseTime,
                attemptCount = 1
            });

            int currentStars = ResearchProgress.GetInt("LostTemple_Total_Stars", 0);
            if (isCorrect)
            {
                score += coinsPerCorrectAnswer;
                currentStars += 1;
                ResearchProgress.SetInt("LostTemple_Total_Stars", currentStars);
                PlayerPrefs.Save();
                if (audioSource != null && correctSound != null) audioSource.PlayOneShot(correctSound);
            }
            else
            {
                currentStars = Mathf.Max(0, currentStars - 1);
                ResearchProgress.SetInt("LostTemple_Total_Stars", currentStars);
                PlayerPrefs.Save();
                if (audioSource != null && incorrectSound != null) audioSource.PlayOneShot(incorrectSound);
            }

            ShowFeedback(isCorrect, q.explanation);
            OnAnswerSubmitted?.Invoke(isCorrect);
            
            // Notify Retention Tracker
            LearningRetentionTracker.Instance?.RecordFirstAttempt(q.questionId, isCorrect, responseTime);

            StartCoroutine(WaitAndNextQuestion());
        }

        private IEnumerator PulseButton(Transform tr)
        {
            if (tr == null) yield break;
            Vector3 orig = Vector3.one;
            float elapsed = 0f;
            while (elapsed < 0.35f)
            {
                elapsed += Time.deltaTime;
                float scale = 1f + 0.08f * Mathf.Sin((elapsed / 0.35f) * Mathf.PI);
                tr.localScale = orig * scale;
                yield return null;
            }
            tr.localScale = orig;
        }

        private IEnumerator ShakeButton(Transform tr)
        {
            if (tr == null) yield break;
            Vector3 origPos = tr.localPosition;
            float elapsed = 0f;
            while (elapsed < 0.35f)
            {
                elapsed += Time.deltaTime;
                float offset = Mathf.Sin(elapsed * 45f) * (6f * (1f - (elapsed / 0.35f)));
                tr.localPosition = origPos + new Vector3(offset, 0f, 0f);
                yield return null;
            }
            tr.localPosition = origPos;
        }

        private void ShowFeedback(bool correct, string explanation)
        {
            if (correct)
            {
                feedbackText.text = "Correct! Well done explorer.";
                feedbackText.color = Color.yellow;
                if (correctFeedbackEffect) Instantiate(correctFeedbackEffect, transform.position, Quaternion.identity);
            }
            else
            {
                feedbackText.text = "Not quite. Try again next time.";
                feedbackText.color = Color.red;
                if (incorrectFeedbackEffect) Instantiate(incorrectFeedbackEffect, transform.position, Quaternion.identity);
            }
            explanationText.text = SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(explanation);
        }

        private IEnumerator WaitAndNextQuestion()
        {
            foreach (var btn in answerButtons) btn.interactable = false;
            yield return new WaitForSeconds(3.0f);
            foreach (var btn in answerButtons) btn.interactable = true;
            
            NextQuestion();
        }

        public void NextQuestion()
        {
            currentQuestionIndex++;
            ShowQuestion(currentQuestionIndex);
        }

        public void CompleteQuiz()
        {
            progressBar.fillAmount = 1f;
            currentResult.correctAnswers = currentResult.questionResults.FindAll(q => q.isCorrect).Count;
            currentResult.score = score;
            currentResult.timeTakenSeconds = Time.time - startTime;
            
            float totalResponseTime = 0;
            foreach(var q in currentResult.questionResults) totalResponseTime += q.responseTimeSeconds;
            currentResult.averageResponseTime = currentResult.questionResults.Count > 0 ? totalResponseTime / currentResult.questionResults.Count : 0;

            if (audioSource != null && quizCompleteSound != null) audioSource.PlayOneShot(quizCompleteSound);
            if (quizPanelPrefab != null) quizPanelPrefab.SetActive(false);

            ResearchData.Record("draft_quiz_complete", JsonUtility.ToJson(currentResult));
            OnQuizCompleted?.Invoke(currentResult);
        }

        public QuizResult GetCurrentResult()
        {
            return currentResult;
        }
    }
}
