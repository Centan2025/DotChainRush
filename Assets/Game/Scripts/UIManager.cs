using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI comboFeedbackText;
    [SerializeField] private UnityEngine.UI.Image flashScreenImage;

    [Header("UI Toolkit (UXML) References")]
    [SerializeField] private UIDocument uiDocument;
    private Label scoreLabelUXML;
    private Label timerLabelUXML;
    private VisualElement comboContainerUXML;
    private Label comboTitleUXML;
    private Label comboMultiplierUXML;
    private Label comboChainUXML;
    private Label dangerLabelUXML;
    private Label feverLabelUXML;
    private Label goalLabelUXML;
    private Label levelLabelUXML;
    
    // Level Up Overlay references
    private VisualElement levelUpOverlayUXML;
    private Label unlockedLevelTitleUXML;
    private Label levelPreviewTextUXML;
    private UnityEngine.UIElements.Button continueBtnUXML;

    private Coroutine flashCoroutine;
    private Coroutine feedbackCoroutine;

    private float currentDisplayedScore = 0f;
    private Coroutine scoreRollCoroutine;
    private Coroutine scoreScaleCoroutine;
    private Coroutine timerScaleCoroutine;
    private Vector3 originalScoreScale = Vector3.one;
    private Vector3 originalTimerScale = Vector3.one;
    private int targetScore = 0;
    private int lastIntegerTime = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (comboFeedbackText != null) comboFeedbackText.gameObject.SetActive(false);
        if (flashScreenImage != null) flashScreenImage.color = Color.clear;
        
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            if (root != null)
            {
                scoreLabelUXML = root.Q<Label>("scoreLabel");
                timerLabelUXML = root.Q<Label>("timerLabel");
                comboContainerUXML = root.Q<VisualElement>("comboContainer");
                comboTitleUXML = root.Q<Label>("comboTitle");
                comboMultiplierUXML = root.Q<Label>("comboMultiplier");
                comboChainUXML = root.Q<Label>("comboChain");
                dangerLabelUXML = root.Q<Label>("dangerLabel");
                feverLabelUXML = root.Q<Label>("feverLabel");
                goalLabelUXML = root.Q<Label>("goalLabel");
                levelLabelUXML = root.Q<Label>("levelLabel");
                
                levelUpOverlayUXML = root.Q<VisualElement>("levelUpOverlay");
                unlockedLevelTitleUXML = root.Q<Label>("unlockedLevelTitle");
                levelPreviewTextUXML = root.Q<Label>("levelPreviewText");
                continueBtnUXML = root.Q<UnityEngine.UIElements.Button>("continueBtn");

                if (comboContainerUXML != null) comboContainerUXML.style.display = DisplayStyle.None;
                if (dangerLabelUXML != null) dangerLabelUXML.style.display = DisplayStyle.None;
                if (feverLabelUXML != null) feverLabelUXML.style.display = DisplayStyle.None;
                if (levelUpOverlayUXML != null) levelUpOverlayUXML.style.display = DisplayStyle.None;

                // Disable old canvas text renderers to show the new ones exclusively
                if (scoreText != null) scoreText.gameObject.SetActive(false);
                if (timerText != null) timerText.gameObject.SetActive(false);
                if (comboFeedbackText != null) comboFeedbackText.gameObject.SetActive(false);
            }
        }

        if (scoreText != null)
        {
            originalScoreScale = scoreText.transform.localScale;
            scoreText.text = "000000";
        }
        if (timerText != null)
        {
            originalTimerScale = timerText.transform.localScale;
            timerText.text = "00:00";
        }
    }

    public void UpdateScore(int score)
    {
        targetScore = score;
        
        if (scoreRollCoroutine != null) StopCoroutine(scoreRollCoroutine);
        scoreRollCoroutine = StartCoroutine(RollScoreCoroutine(score));

        if (scoreScaleCoroutine != null) StopCoroutine(scoreScaleCoroutine);
        scoreScaleCoroutine = StartCoroutine(ScalePopCoroutine(scoreText.transform, originalScoreScale, 1.15f, 0.15f));
    }

    public void UpdateTimer(float timeElapsed)
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        string formattedTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timerText != null)
        {
            timerText.text = formattedTime;
        }

        // Pulse the timer every second for a game-like heartbeat effect
        if (seconds != lastIntegerTime)
        {
            lastIntegerTime = seconds;
            if (timerText != null)
            {
                if (timerScaleCoroutine != null) StopCoroutine(timerScaleCoroutine);
                timerScaleCoroutine = StartCoroutine(ScalePopCoroutine(timerText.transform, originalTimerScale, 1.08f, 0.12f));
            }
        }
    }

    private IEnumerator RollScoreCoroutine(int target)
    {
        float duration = 0.4f;
        float elapsed = 0f;
        float start = currentDisplayedScore;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentDisplayedScore = Mathf.Lerp(start, target, elapsed / duration);
            string formattedScore = string.Format("{0:000000}", Mathf.RoundToInt(currentDisplayedScore));
            
            if (scoreText != null)
            {
                scoreText.text = formattedScore;
            }
            if (scoreLabelUXML != null)
            {
                scoreLabelUXML.text = formattedScore;
            }
            yield return null;
        }

        currentDisplayedScore = target;
        string finalScore = string.Format("{0:000000}", target);
        if (scoreText != null)
        {
            scoreText.text = finalScore;
        }
        if (scoreLabelUXML != null)
        {
            scoreLabelUXML.text = finalScore;
        }
    }

    private IEnumerator ScalePopCoroutine(Transform targetTransform, Vector3 originalScale, float maxScaleFactor, float duration)
    {
        if (targetTransform == null) yield break;

        float halfDuration = duration / 2f;
        
        // Scale Up
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(originalScale, originalScale * maxScaleFactor, elapsed / halfDuration);
            yield return null;
        }

        // Scale Down
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(originalScale * maxScaleFactor, originalScale, elapsed / halfDuration);
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }

    public void SetGameOverActive(bool active, int finalScore = 0, int bestCombo = 0, bool isNewHighScore = false, int improvementPercentage = 0)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(active);
            if (active)
            {
                // Find and set text values on TMPro components in children
                TextMeshProUGUI[] texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI txt in texts)
                {
                    if (txt.gameObject.name == "ScoreTextVal")
                    {
                        txt.text = $"SKOR: {finalScore}";
                    }
                    else if (txt.gameObject.name == "BestComboTextVal")
                    {
                        txt.text = $"EN İYİ COMBO: {bestCombo} TOP";
                    }
                    else if (txt.gameObject.name == "HighScoreTextVal")
                    {
                        txt.text = isNewHighScore ? "YENİ EN YÜKSEK SKOR!" : $"EN YÜKSEK SKOR: {SaveSystem.LoadInt("HighScore", 0)}";
                        txt.color = isNewHighScore ? new Color(1f, 0.84f, 0f) : Color.white;
                    }
                    else if (txt.gameObject.name == "ImprovementTextVal")
                    {
                        if (improvementPercentage >= 0)
                        {
                            txt.text = $"+{improvementPercentage}% daha iyi oynadın!";
                            txt.color = Color.green;
                        }
                        else
                        {
                            txt.text = $"{improvementPercentage}% daha düşük performans";
                            txt.color = new Color(1f, 0.4f, 0.4f);
                        }
                    }
                }
            }
        }
    }

    public void TriggerScreenFlash(Color color, float duration)
    {
        if (flashScreenImage == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashCoroutine(color, duration));
    }

    public void ShowComboFeedback(string message, Color color)
    {
        if (message.Contains("FEVER"))
        {
            ShowComboFeedback("FEVER", "ACTIVE!", "x10 POINTS!", color);
        }
        else if (message.Contains("COMBO x"))
        {
            ShowComboFeedback("COMBO", message.Replace("COMBO ", ""), "", color);
        }
        else
        {
            ShowComboFeedback("", message, "", color);
        }
    }

    public void ShowComboFeedback(string title, string value, string subtitle, Color color)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackCoroutine(title, value, subtitle, color));
    }

    private IEnumerator FlashCoroutine(Color color, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.35f, 0f, elapsed / duration);
            flashScreenImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        flashScreenImage.color = Color.clear;
    }

    private IEnumerator FeedbackCoroutine(string title, string value, string subtitle, Color color)
    {
        bool useUXML = (comboContainerUXML != null);

        if (useUXML)
        {
            if (comboTitleUXML != null) comboTitleUXML.text = title;
            if (comboMultiplierUXML != null) comboMultiplierUXML.text = value;
            if (comboChainUXML != null) comboChainUXML.text = subtitle;

            // Set color overrides if needed, otherwise default styles apply
            if (comboTitleUXML != null) comboTitleUXML.style.color = (title == "FEVER") ? Color.yellow : new Color(1f, 0.66f, 0f);
            if (comboMultiplierUXML != null) comboMultiplierUXML.style.color = color;
            if (comboChainUXML != null) comboChainUXML.style.color = (title == "FEVER") ? Color.white : new Color(0.36f, 0.94f, 1f);

            comboContainerUXML.style.display = DisplayStyle.Flex;
            comboContainerUXML.style.scale = new StyleScale(new Scale(new Vector3(0.5f, 0.5f, 1f)));
            comboContainerUXML.style.opacity = 1f;
        }
        else if (comboFeedbackText != null)
        {
            comboFeedbackText.text = $"{title} {value} {subtitle}".Trim();
            comboFeedbackText.color = color;
            comboFeedbackText.gameObject.SetActive(true);
            comboFeedbackText.transform.localScale = Vector3.one * 0.5f;
        }

        float elapsed = 0f;
        float duration = 0.22f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentScale = Mathf.Lerp(0.5f, 1.25f, t);

            if (useUXML)
            {
                comboContainerUXML.style.scale = new StyleScale(new Scale(new Vector3(currentScale, currentScale, 1f)));
            }
            else if (comboFeedbackText != null)
            {
                comboFeedbackText.transform.localScale = Vector3.one * currentScale;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.75f);

        // Smooth fade out
        if (useUXML)
        {
            elapsed = 0f;
            float fadeDuration = 0.18f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                float opacity = Mathf.Lerp(1f, 0f, t);
                comboContainerUXML.style.opacity = opacity;
                yield return null;
            }
        }

        if (useUXML)
        {
            comboContainerUXML.style.display = DisplayStyle.None;
        }
        else if (comboFeedbackText != null)
        {
            comboFeedbackText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (feverLabelUXML != null && feverLabelUXML.resolvedStyle.display == DisplayStyle.Flex)
        {
            float scale = 1.0f + 0.08f * Mathf.Sin(Time.time * 12f);
            feverLabelUXML.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
        }
    }

    public void SetDangerActive(bool active, string text = "")
    {
        if (dangerLabelUXML != null)
        {
            dangerLabelUXML.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            if (active && !string.IsNullOrEmpty(text))
            {
                dangerLabelUXML.text = text;
            }
        }
    }

    public void SetFeverActive(bool active)
    {
        if (feverLabelUXML != null)
        {
            feverLabelUXML.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void UpdateGoal(int goal)
    {
        if (goalLabelUXML != null)
        {
            goalLabelUXML.text = string.Format("{0:000000}", goal);
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelLabelUXML != null)
        {
            levelLabelUXML.text = level.ToString();
        }
    }

    private System.Action onLevelUpOverlayContinueCallback;

    public void ShowLevelUpOverlay(int nextLevel, string previewText, System.Action onContinue)
    {
        if (levelUpOverlayUXML != null)
        {
            if (unlockedLevelTitleUXML != null)
            {
                unlockedLevelTitleUXML.text = $"SEVİYE {nextLevel}";
            }
            if (levelPreviewTextUXML != null)
            {
                levelPreviewTextUXML.text = previewText;
            }

            levelUpOverlayUXML.style.display = DisplayStyle.Flex;

            // Remove existing listener to prevent duplicate clicks
            continueBtnUXML.clicked -= OnLevelUpContinueClicked;
            onLevelUpOverlayContinueCallback = onContinue;
            continueBtnUXML.clicked += OnLevelUpContinueClicked;
        }
        else
        {
            // If UI elements are missing, immediately fallback and continue
            onContinue?.Invoke();
        }
    }

    private void OnLevelUpContinueClicked()
    {
        if (levelUpOverlayUXML != null)
        {
            levelUpOverlayUXML.style.display = DisplayStyle.None;
        }
        continueBtnUXML.clicked -= OnLevelUpContinueClicked;
        onLevelUpOverlayContinueCallback?.Invoke();
    }
}
