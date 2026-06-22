using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RadialTimerController : MonoBehaviour 
{
    [Header("UI Bileşenleri")]
    public Image radialFillImage;
    public RectTransform indicatorDot; 
    public TextMeshProUGUI countdownText;
    
    [Header("Zaman Ayarları")]
    public float totalTime = 90f; // 90 saniye (1:30) oyun süresi
    private float currentTime;
    public float CurrentTime
    {
        get => currentTime;
        set
        {
            currentTime = Mathf.Max(0f, value);
            UpdateUI();
        }
    }

    [Header("Renk Geçişi (Gradient)")]
    public Gradient timerGradient;

    private int lastCountdownSecond = -1;
    private TextMeshProUGUI centerCountdownText;
    private Coroutine textAnimationCoroutine;

    void Awake()
    {
        totalTime = 90f; // Force 90 seconds (1:30)
    }

    void Start() 
    {
        currentTime = totalTime;
        
        if (timerGradient == null || timerGradient.colorKeys.Length <= 1)
        {
            GradientColorKey[] gck = new GradientColorKey[2];
            gck[0].color = new Color(1f, 0.62f, 0.05f); // #FF9E0D (Turuncu)
            gck[0].time = 0.0f;
            gck[1].color = new Color(1f, 0f, 0.61f);    // #FF009D (Pembe)
            gck[1].time = 1.0f;

            GradientAlphaKey[] gak = new GradientAlphaKey[2];
            gak[0].alpha = 1.0f; gak[0].time = 0.0f;
            gak[1].alpha = 1.0f; gak[1].time = 1.0f;

            timerGradient = new Gradient();
            timerGradient.SetKeys(gck, gak);
        }
    }

    void Update() 
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        if (currentTime > 0) 
        {
            currentTime -= Time.deltaTime;
            UpdateUI();

            // Center countdown check for final 5 seconds (5, 4, 3, 2, 1)
            int currentSecond = Mathf.CeilToInt(currentTime);
            if (currentSecond > 0 && currentSecond <= 5 && currentSecond != lastCountdownSecond)
            {
                lastCountdownSecond = currentSecond;
                TriggerCenterCountdown(currentSecond);
            }
            
            if (currentTime <= 0)
            {
                currentTime = 0;
                UpdateUI();
                
                // Clear any remaining center countdown text
                if (centerCountdownText != null)
                {
                    centerCountdownText.gameObject.SetActive(false);
                }

                if (GameManager.Instance != null)
                {
                    Debug.Log("[Timer] Time is up! Triggering GameOver.");
                    GameManager.Instance.GameOver();
                }
            }
        } 
    }

    private void TriggerCenterCountdown(int secondValue)
    {
        // Find main Canvas to parent the countdown text
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        if (centerCountdownText == null)
        {
            GameObject textObj = new GameObject("CenterCountdownText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(canvas.transform, false);

            centerCountdownText = textObj.GetComponent<TextMeshProUGUI>();
            centerCountdownText.alignment = TextAlignmentOptions.Center;
            centerCountdownText.fontStyle = FontStyles.Bold | FontStyles.Italic;
            centerCountdownText.fontSize = 250f; // Ekstra devasa taban boyutu
            centerCountdownText.raycastTarget = false;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

        centerCountdownText.gameObject.SetActive(true);
        centerCountdownText.text = secondValue.ToString();

        // Color transitions from warm yellow (5) to glowing hot red (1)
        float t = 1f - ((float)secondValue / 5f);
        Color startColor = new Color(1f, 0.82f, 0f); // Gold/Yellow
        Color endColor = new Color(1f, 0.1f, 0.05f); // Neon Red/Orange
        centerCountdownText.color = Color.Lerp(startColor, endColor, t);

        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }
        textAnimationCoroutine = StartCoroutine(AnimateCountdownText());

        // Play warning tick sound if possible
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDangerWarningSound(true);
        }
    }

    private IEnumerator AnimateCountdownText()
    {
        float elapsed = 0f;
        float duration = 0.95f; // shrink and fade within the second

        Vector3 startScale = Vector3.one * 3.2f; // Daha agresif başlangıç büyüklüğü
        Vector3 endScale = Vector3.one * 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (centerCountdownText != null)
            {
                // Shrink
                centerCountdownText.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
                
                // Fade out
                Color c = centerCountdownText.color;
                c.a = Mathf.Lerp(1.0f, 0.0f, progress);
                centerCountdownText.color = c;
            }
            yield return null;
        }

        if (centerCountdownText != null)
        {
            centerCountdownText.gameObject.SetActive(false);
        }
    }

    public void ResetTimer()
    {
        currentTime = totalTime;
        lastCountdownSecond = -1;
        
        if (centerCountdownText != null)
        {
            centerCountdownText.gameObject.SetActive(false);
        }
        
        UpdateUI();
    }

    void UpdateUI() 
    {
        float progress = currentTime / totalTime;
        
        if (radialFillImage != null)
        {
            radialFillImage.fillAmount = progress;
            radialFillImage.color = timerGradient.Evaluate(1f - progress);
        }

        if (indicatorDot != null && radialFillImage != null)
        {
            float angle = progress * 360f;
            indicatorDot.localRotation = Quaternion.Euler(0, 0, -angle);
            
            Image dotImage = indicatorDot.GetComponentInChildren<Image>();
            if (dotImage != null) dotImage.color = radialFillImage.color;
        }

        if (countdownText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            countdownText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
    }
}