using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CircularTimer : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public Image ring;
    public TMP_Text timerText;

    [Header("Zaman Ayarları")]
    public float totalTime = 90f; // 90 saniye (1:30) oyun süresi

    private float time;
    public float CurrentTime
    {
        get => time;
        set
        {
            time = Mathf.Max(0f, value);
            UpdateUI();
        }
    }
    private int lastCountdownSecond = -1;
    private TextMeshProUGUI centerCountdownText;
    private Coroutine textAnimationCoroutine;

    void Awake()
    {
        totalTime = 90f; // Force 90 seconds (1:30)
    }

    void Start()
    {
        time = totalTime;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        if (time > 0)
        {
            time -= Time.deltaTime;
            UpdateUI();

            // Center countdown check for final 5 seconds (5, 4, 3, 2, 1)
            int currentSecond = Mathf.CeilToInt(time);
            if (currentSecond > 0 && currentSecond <= 5 && currentSecond != lastCountdownSecond)
            {
                lastCountdownSecond = currentSecond;
                TriggerCenterCountdown(currentSecond);
            }

            if (time <= 0)
            {
                time = 0;
                UpdateUI();

                if (centerCountdownText != null)
                {
                    centerCountdownText.gameObject.SetActive(false);
                }

                if (GameManager.Instance != null)
                {
                    Debug.Log("[CircularTimer] Time is up! Triggering GameOver.");
                    GameManager.Instance.GameOver();
                }
            }
        }
    }

    private void TriggerCenterCountdown(int secondValue)
    {
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

        float t = 1f - ((float)secondValue / 5f);
        Color startColor = new Color(1f, 0.82f, 0f);
        Color endColor = new Color(1f, 0.1f, 0.05f);
        centerCountdownText.color = Color.Lerp(startColor, endColor, t);

        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }
        textAnimationCoroutine = StartCoroutine(AnimateCountdownText());

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDangerWarningSound(true);
        }
    }

    private IEnumerator AnimateCountdownText()
    {
        float elapsed = 0f;
        float duration = 0.95f;

        Vector3 startScale = Vector3.one * 3.2f; // Daha agresif başlangıç büyüklüğü
        Vector3 endScale = Vector3.one * 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (centerCountdownText != null)
            {
                centerCountdownText.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
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
        float bonusTime = 0f;
        if (GameBrain.Instance != null)
        {
            bonusTime = GameBrain.Instance.GetMutationValue("TimeLimitMod");
        }
        totalTime = 90f + bonusTime;
        time = totalTime;
        lastCountdownSecond = -1;

        if (centerCountdownText != null)
        {
            centerCountdownText.gameObject.SetActive(false);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        float percent = time / totalTime;
        if (ring != null)
        {
            ring.fillAmount = percent;
        }

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(time);
            int minute = seconds / 60;
            int second = seconds % 60;
            timerText.text = minute + ":" + second.ToString("00");
        }
    }
}