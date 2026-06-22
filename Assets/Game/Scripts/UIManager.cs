using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Top Bar UI References (Canvas TMP)")]
    [SerializeField] private TextMeshProUGUI scoreText;         // Center big white score
    [SerializeField] private TextMeshProUGUI bestScoreText;     // Center small gold best score
    [SerializeField] private TextMeshProUGUI timerText;         // Timer value text (inside circle)
    [SerializeField] private Image timerProgressCircle;         // Radial Image for timer circle
    [SerializeField] private TextMeshProUGUI goldText;          // Gold amount value
    [SerializeField] private Button pauseButton;                // Left pause button

    [Header("Fever Panel References")]
    [SerializeField] private GameObject feverPanel;             // Left Fever container
    [SerializeField] private Image feverProgressBar;            // Fever slider progress
    [SerializeField] private FeverSegmentedBar feverSegmentedBar; // 5-slot segmented progress bar
    [SerializeField] private TextMeshProUGUI feverTimeLeftText;  // Fever remaining seconds

    [Header("Combo & Danger References")]
    [SerializeField] private GameObject comboContainer;         // Center combo panel
    [SerializeField] private TextMeshProUGUI comboTitleText;    // "COMBO" text
    [SerializeField] private TextMeshProUGUI comboMultiplierText; // "x4" multiplier text
    [SerializeField] private TextMeshProUGUI comboChainText;     // "12 CHAIN" subtext
    [SerializeField] private GameObject dangerBanner;           // Danger zone hazard strip
    [SerializeField] private TextMeshProUGUI dangerText;         // Danger countdown text
    [SerializeField] private Image flashScreenImage;            // Fullscreen feedback flash

    [Header("Bottom Card References")]
    [SerializeField] private TextMeshProUGUI goalText;          // Bottom goal text
    [SerializeField] private TextMeshProUGUI levelText;         // Bottom level text

    [Header("Overlay Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelUpOverlay;
    [SerializeField] private TextMeshProUGUI unlockedLevelTitleText;
    [SerializeField] private TextMeshProUGUI levelPreviewText;
    [SerializeField] private Button levelUpContinueButton;

    // Neon glow colors
    private static readonly Color NeonCyan = new Color(0f, 0.85f, 1f, 1f);
    private static readonly Color NeonPink = new Color(1f, 0.3f, 0.6f, 1f);
    private static readonly Color DarkPurple = new Color(0.15f, 0.1f, 0.25f, 0.5f);

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
        // Dynamic Resolution UI Alignment (Deferred by 1 frame to allow layout initialization)
        StartCoroutine(AlignUILayoutDeferred());

        // Deactivate overlay modules initially
        if (comboContainer != null) comboContainer.SetActive(false);
        if (dangerBanner != null) dangerBanner.SetActive(false);
        if (feverPanel != null) feverPanel.SetActive(true);
        if (levelUpOverlay != null) levelUpOverlay.SetActive(false);
        if (flashScreenImage != null) flashScreenImage.color = Color.clear;

        if (comboTitleText != null)
        {
            comboTitleText.enableWordWrapping = false;
        }
        if (comboMultiplierText != null)
        {
            comboMultiplierText.enableWordWrapping = false;
            comboMultiplierText.enableAutoSizing = true;
            comboMultiplierText.fontSizeMin = 24;
            comboMultiplierText.fontSizeMax = 86;
        }
        if (comboChainText != null)
        {
            comboChainText.enableWordWrapping = false;
        }

        if (scoreText != null)
        {
            originalScoreScale = scoreText.transform.localScale;
            scoreText.text = "<b>0</b>";
        }
        if (timerText != null)
        {
            originalTimerScale = timerText.transform.localScale;
            timerText.text = "0:00";
        }

        UpdateBestScore(SaveSystem.LoadInt("HighScore", 0));
        UpdateGoldAmount(SaveSystem.LoadInt("GoldCoins", 12450));

        SetupFeverGlow();

        // Dynamically setup and attach RadialTimerController to TimerCircleContainer
        if (timerProgressCircle != null)
        {
            Transform parent = timerProgressCircle.transform.parent;
            if (parent != null)
            {
                // Setup RadialBackground (Track)
                Transform trackTransform = parent.Find("Track");
                if (trackTransform != null)
                {
                    Image trackImg = trackTransform.GetComponent<Image>();
                    if (trackImg != null)
                    {
                        trackImg.color = new Color(0.12f, 0.09f, 0.18f, 1f); // #1F182E
                        trackImg.type = Image.Type.Simple;
                    }
                }

                // Setup RadialFill (ProgressRing)
                Image fillImg = timerProgressCircle;
                fillImg.type = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Radial360;
                fillImg.fillOrigin = (int)Image.Origin360.Top;
                fillImg.fillClockwise = true;

                // Setup Label (TimeHeader)
                Transform headerTransform = parent.Find("TimeHeader");
                if (headerTransform != null)
                {
                    TextMeshProUGUI headerText = headerTransform.GetComponent<TextMeshProUGUI>();
                    if (headerText != null)
                    {
                        headerText.color = new Color(0.93f, 0.7f, 1f, 0.6f); // #ECB2FF %60 Opaklık
                    }
                }

                // Setup CountdownText (TimerText)
                if (timerText != null)
                {
                    timerText.fontStyle = FontStyles.Bold;
                }

                // Attach RadialTimerController dynamically
                RadialTimerController rtc = parent.gameObject.GetComponent<RadialTimerController>();
                if (rtc == null)
                {
                    rtc = parent.gameObject.AddComponent<RadialTimerController>();
                }
                rtc.radialFillImage = fillImg;
                rtc.countdownText = timerText;

                // Create Turuncu (#FF9E0D) -> Pembe (#FF009D) Gradient
                Gradient grad = new Gradient();
                GradientColorKey[] gck = new GradientColorKey[2];
                gck[0] = new GradientColorKey(new Color(1f, 0.62f, 0.05f, 1f), 0f); // #FF9E0D
                gck[1] = new GradientColorKey(new Color(1f, 0f, 0.61f, 1f), 1f);    // #FF009D
                GradientAlphaKey[] gak = new GradientAlphaKey[2];
                gak[0] = new GradientAlphaKey(1f, 0f);
                gak[1] = new GradientAlphaKey(1f, 1f);
                grad.SetKeys(gck, gak);
                rtc.timerGradient = grad;
                rtc.totalTime = 90f; // 90 saniye (1:30) oyun süresi
                rtc.ResetTimer();
            }
        }
    }

    private void SetupFeverGlow()
    {
        if (feverPanel == null) return;

        // Tint fever progress bar
        if (feverProgressBar != null)
        {
            feverProgressBar.color = NeonPink;
        }

        // Create fever time text if not assigned
        if (feverTimeLeftText == null && feverPanel != null)
        {
            Transform existing = feverPanel.transform.Find("FeverTimeText");
            if (existing != null)
            {
                feverTimeLeftText = existing.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject obj = new GameObject("FeverTimeText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                obj.transform.SetParent(feverPanel.transform, false);
                RectTransform rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, -40f); // Positioned nicely below the segmented bar
                rect.sizeDelta = new Vector2(250f, 50f);
                feverTimeLeftText = obj.GetComponent<TextMeshProUGUI>();
                feverTimeLeftText.text = "";
                feverTimeLeftText.fontSize = 36f; // KOCAMAN ve okunabilir yazı boyutu
                feverTimeLeftText.color = NeonPink;
                feverTimeLeftText.fontStyle = FontStyles.Bold;
                feverTimeLeftText.alignment = TextAlignmentOptions.Center;
                feverTimeLeftText.raycastTarget = false;
            }
        }
    }



    public void UpdateBestScore(int bestScore)
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = string.Format("<b>{0:N0}</b>", bestScore);
        }
    }

    public void UpdateGoldAmount(int goldCoins)
    {
        if (goldText != null)
        {
            goldText.text = string.Format("{0:N0}", goldCoins);
        }
    }

    public void UpdateScore(int score)
    {
        targetScore = score;
        if (scoreRollCoroutine != null) StopCoroutine(scoreRollCoroutine);
        scoreRollCoroutine = StartCoroutine(RollScoreCoroutine(score));

        if (scoreText != null)
        {
            if (scoreScaleCoroutine != null) StopCoroutine(scoreScaleCoroutine);
            scoreScaleCoroutine = StartCoroutine(ScalePopCoroutine(scoreText.transform, originalScoreScale, 1.15f, 0.15f));
        }
    }

    public void UpdateTimer(float timeElapsed)
    {
        // Let RadialTimerController handle text and radial fill updates
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
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
        // Faster rolling animation that increments through every single score unit (1, 2, ..., target)
        // using a rapid lerp over a short duration, but ensuring we display integer steps.
        float duration = 0.35f;
        float elapsed = 0f;
        float start = currentDisplayedScore;

        if (Mathf.Abs(target - start) > 0)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                // Use a smooth/fast progression
                currentDisplayedScore = Mathf.Lerp(start, target, progress);
                int currentVal = Mathf.RoundToInt(currentDisplayedScore);
                
                // Group with commas like 1,234,560 and apply bold styling tags (not italic)
                string formattedScore = string.Format("<b>{0:N0}</b>", currentVal);

                if (scoreText != null)
                {
                    scoreText.text = formattedScore;
                }

                int activeHighScore = SaveSystem.LoadInt("HighScore", 0);
                if (currentVal > activeHighScore)
                {
                    UpdateBestScore(currentVal);
                }
                yield return null;
            }
        }

        currentDisplayedScore = target;
        string finalScore = string.Format("<b>{0:N0}</b>", target);
        if (scoreText != null)
        {
            scoreText.text = finalScore;
        }

        int endHighScore = SaveSystem.LoadInt("HighScore", 0);
        if (target > endHighScore)
        {
            UpdateBestScore(target);
        }
    }

    private IEnumerator ScalePopCoroutine(Transform targetTransform, Vector3 originalScale, float maxScaleFactor, float duration)
    {
        if (targetTransform == null) yield break;

        float halfDuration = duration / 2f;
        
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(originalScale, originalScale * maxScaleFactor, elapsed / halfDuration);
            yield return null;
        }

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
                UpdateBestScore(SaveSystem.LoadInt("HighScore", finalScore));

                TextMeshProUGUI[] texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI txt in texts)
                {
                    if (txt.gameObject.name == "ScoreTextVal")
                    {
                        txt.text = $"SKOR: {finalScore:N0}";
                    }
                    else if (txt.gameObject.name == "BestComboTextVal")
                    {
                        txt.text = $"EN İYİ COMBO: {bestCombo} TOP";
                    }
                    else if (txt.gameObject.name == "HighScoreTextVal")
                    {
                        txt.text = isNewHighScore ? "YENİ EN YÜKSEK SKOR!" : $"EN YÜKSEK SKOR: {SaveSystem.LoadInt("HighScore", 0):N0}";
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
        // Styling like the provided image: Bold, Italic, orange/yellow gradient coloring
        // We can use TextMeshPro's <color> or styling tags for a beautiful gradient look.
        if (comboTitleText != null) 
            comboTitleText.text = "<i><b>COMBO</b></i>";
        
        if (comboMultiplierText != null) 
            comboMultiplierText.text = $"<i><b>{value}</b></i>";
            
        if (comboChainText != null) 
            comboChainText.text = string.IsNullOrEmpty(subtitle) ? "" : $"<i><b>{subtitle}</b></i>";

        // Assigning warm orange/yellow gradient color
        Color warmOrange = new Color(1f, 0.65f, 0f); // Beautiful bright orange
        if (comboTitleText != null) comboTitleText.color = warmOrange;
        if (comboMultiplierText != null) comboMultiplierText.color = warmOrange;
        if (comboChainText != null) comboChainText.color = new Color(1f, 0.8f, 0.2f); // Golden yellow

        if (comboContainer != null)
        {
            comboContainer.SetActive(true);
            comboContainer.transform.localScale = Vector3.one * 0.5f;
        }

        float elapsed = 0f;
        float duration = 0.22f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentScale = Mathf.Lerp(0.5f, 1.25f, elapsed / duration);
            if (comboContainer != null)
            {
                comboContainer.transform.localScale = Vector3.one * currentScale;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.75f);

        if (comboContainer != null)
        {
            comboContainer.SetActive(false);
        }
    }

    public void SetDangerActive(bool active, string text = "")
    {
        if (dangerBanner != null)
        {
            dangerBanner.SetActive(active);
        }
        if (dangerText != null && active && !string.IsNullOrEmpty(text))
        {
            dangerText.text = text;
        }
    }

    public void SetFeverActive(bool active)
    {
        // Keep the feverPanel active at all times so progress is visible.
        // We can use this to trigger a visual glow effect or animation.
        if (feverPanel != null)
        {
            feverPanel.SetActive(true);
        }
    }

    public void UpdateFeverProgress(float current, float max)
    {
        if (feverProgressBar != null)
        {
            float fill = max > 0 ? Mathf.Clamp01(current / max) : 0f;
            feverProgressBar.fillAmount = fill;
        }
        if (feverSegmentedBar != null)
        {
            float fill = max > 0 ? Mathf.Clamp01(current / max) : 0f;
            feverSegmentedBar.SetProgress(fill);
        }
        if (feverTimeLeftText != null)
        {
            bool isFever = GameManager.Instance != null && GameManager.Instance.IsFeverActive;
            if (isFever)
            {
                feverTimeLeftText.text = $"{current:F1}s";
                feverTimeLeftText.color = NeonPink;
            }
            else
            {
                int chain = Mathf.RoundToInt(current);
                int threshold = Mathf.RoundToInt(max);
                float percent = threshold > 0 ? (float)chain / threshold * 100f : 0f;
                
                if (percent >= 100f)
                {
                    feverTimeLeftText.text = "READY!";
                    feverTimeLeftText.color = Color.yellow;
                }
                else if (percent > 0f)
                {
                    feverTimeLeftText.text = $"%{Mathf.RoundToInt(percent)}";
                    feverTimeLeftText.color = new Color(1f, 0.3f, 0.6f, 0.8f); // Slightly faded NeonPink
                }
                else
                {
                    feverTimeLeftText.text = "";
                }
            }
        }
    }

    public void UpdateGoal(int goal)
    {
        if (goalText != null)
        {
            goalText.text = $"HEDEF: {goal:N0}";
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            string subtitle = "";
            if (DifficultyManager.Instance != null)
            {
                subtitle = DifficultyManager.Instance.LevelSubtitle;
            }
            levelText.text = string.IsNullOrEmpty(subtitle)
                ? $"LVL {level}"
                : $"LVL {level}\n<size=60%>{subtitle}</size>";
        }
    }

    private System.Action onLevelUpOverlayContinueCallback;

    private string GetRandomUpgradeAndApply()
    {
        if (GameBrain.Instance == null) return "Enerji Dengesi (Hazırlık tamamlandı)";

        int index = Random.Range(0, 5);
        switch (index)
        {
            case 0:
                GameBrain.Instance.AddMutation("GravityMod", -0.015f);
                return "Yerçekimi Hafifletme (Yerçekimi hızı azaldı!)";
            case 1:
                GameBrain.Instance.AddMutation("SpawnRateMod", -0.02f);
                return "Hızlı Bağlantı (Topların geliş süresi hızlandı!)";
            case 2:
                GameBrain.Instance.AddMutation("ScoreMod", 0.05f);
                return "Skor Akışı (Tüm patlamalar +5% ekstra puan!)";
            case 3:
                GameBrain.Instance.AddMutation("FeverMod", 1.0f);
                return "Fever Fırtınası (Fever modunun süresi 1 saniye uzadı!)";
            default:
                GameBrain.Instance.AddMutation("GoldMod", 50f);
                return "Altın Dokunuş (Seviye tamamlandığında +50 Altın bonus!)";
        }
    }

    public void ShowLevelUpOverlay(int nextLevel, string previewText, System.Action onContinue)
    {
        if (levelUpOverlay != null)
        {
            if (unlockedLevelTitleText != null)
            {
                string title = LevelData.GetLevelDisplayTitle(nextLevel);
                unlockedLevelTitleText.text = $"SEVİYE {nextLevel}\n<size=70%>{title}</size>";
            }
            
            string upgradeText = GetRandomUpgradeAndApply();
            if (levelPreviewText != null)
            {
                levelPreviewText.text = previewText + $"\n\n<b><color=#FFD700>ROGUELIKE MUTASYON KAZANILDI:</color></b>\n<color=#00FFFF>{upgradeText}</color>";
            }

            levelUpOverlay.SetActive(true);

            if (levelUpContinueButton != null)
            {
                levelUpContinueButton.onClick.RemoveAllListeners();
                onLevelUpOverlayContinueCallback = onContinue;
                levelUpContinueButton.onClick.AddListener(OnLevelUpContinueClicked);
            }
        }
        else
        {
            onContinue?.Invoke();
        }
    }

    private void OnLevelUpContinueClicked()
    {
        if (levelUpOverlay != null)
        {
            levelUpOverlay.SetActive(false);
        }
        onLevelUpOverlayContinueCallback?.Invoke();
    }

    private IEnumerator AlignUILayoutDeferred()
    {
        // Wait 1 frame so Canvas and automatic layout groups calculate their initial sizes/rects
        yield return null;

        // 1. Check and configure CanvasScaler for notch-safety & robust aspect ratio scaling
        CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balance width scaling (perfect for 9:16 fallback)
        }

        // 2. Align Fever Bar (feverPanel / FeverSegmentedBar) directly under FeverIcon
        GameObject feverIconGo = GameObject.Find("FeverIcon");
        GameObject targetFeverPanel = feverPanel;
        
        // Fallback search if feverPanel is linked incorrectly
        if (targetFeverPanel == null)
        {
            FeverSegmentedBar segBar = GetComponentInChildren<FeverSegmentedBar>(true);
            if (segBar != null) targetFeverPanel = segBar.gameObject;
        }

        if (feverIconGo != null && targetFeverPanel != null)
        {
            RectTransform iconRt = feverIconGo.GetComponent<RectTransform>();
            RectTransform panelRt = targetFeverPanel.GetComponent<RectTransform>();
            if (iconRt != null && panelRt != null)
            {
                // Force top-left anchoring to align with FeverIcon
                panelRt.anchorMin = new Vector2(0f, 1f);
                panelRt.anchorMax = new Vector2(0f, 1f);
                panelRt.pivot = new Vector2(0f, 1f);
                
                // Align Y coordinates relative to parent (Header)
                Vector2 iconPos = iconRt.anchoredPosition;
                panelRt.anchoredPosition = new Vector2(iconPos.x, iconPos.y - iconRt.sizeDelta.y - 12f);
                panelRt.sizeDelta = new Vector2(110f, 26f); // Ensure a compact sleek width
            }
        }

        // 3. Align Combo Container (comboContainer) exactly below ScoreText (Y=-200) and above the play boundaries (Y=-240)
        if (comboContainer != null)
        {
            RectTransform comboRt = comboContainer.GetComponent<RectTransform>();
            if (comboRt != null)
            {
                // Anchor to Top-Center to follow ScoreText
                comboRt.anchorMin = new Vector2(0.5f, 1f);
                comboRt.anchorMax = new Vector2(0.5f, 1f);
                comboRt.pivot = new Vector2(0.5f, 1f);
                
                // Align Y exactly between header bar (200px) and play frame top bounds
                comboRt.anchoredPosition = new Vector2(0f, -205f);
                comboRt.sizeDelta = new Vector2(300f, 85f);
                
                // Adapt font sizes to fit nicely without overlap
                if (comboTitleText != null) comboTitleText.fontSize = 24f;
                if (comboMultiplierText != null) comboMultiplierText.fontSize = 20f;
                if (comboChainText != null) comboChainText.fontSize = 11f;
            }
        }

        // 4. Clean anchors for HeaderPanel & BottomHUDPanel to avoid clipping on high resolutions
        GameObject headerPanelGo = GameObject.Find("HeaderPanel");
        if (headerPanelGo != null)
        {
            RectTransform headerRt = headerPanelGo.GetComponent<RectTransform>();
            if (headerRt != null)
            {
                headerRt.anchorMin = new Vector2(0f, 1f);
                headerRt.anchorMax = new Vector2(1f, 1f);
                headerRt.pivot = new Vector2(0.5f, 1f);
                headerRt.anchoredPosition = Vector2.zero;
                headerRt.sizeDelta = new Vector2(0f, 200f);
            }
        }

        GameObject bottomHudGo = GameObject.Find("BottomHUDPanel");
        if (bottomHudGo == null) bottomHudGo = GameObject.Find("BottomHUD");
        if (bottomHudGo != null)
        {
            RectTransform bottomRt = bottomHudGo.GetComponent<RectTransform>();
            if (bottomRt != null)
            {
                bottomRt.anchorMin = new Vector2(0f, 0f);
                bottomRt.anchorMax = new Vector2(1f, 0f);
                bottomRt.pivot = new Vector2(0.5f, 0f);
                bottomRt.anchoredPosition = new Vector2(0f, 6f);
                bottomRt.sizeDelta = new Vector2(-14f, 68f);
            }
        }
    }
}
