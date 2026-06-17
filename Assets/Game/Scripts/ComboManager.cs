using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [Header("Juice Prefabs")]
    [SerializeField] private GameObject particlePrefab;

    private Coroutine shakeCoroutine;
    private Coroutine slowMoCoroutine;
    private Vector3 originalCamPos;

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

    public void ProcessChain(List<Dot> chain)
    {
        if (chain == null || chain.Count < 3) return;

        int length = chain.Count;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterChain(length);
        }

        int multiplier = 1;
        if (length >= 15) multiplier = 5;
        else if (length >= 8) multiplier = 3;
        else if (length >= 5) multiplier = 2;
        else if (length >= 3) multiplier = 1;

        int points = length * 10 * multiplier;
        if (GameManager.Instance != null && GameManager.Instance.IsFeverActive)
        {
            points *= 10; // FEVER x10 SCORE MULTIPLIER!
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(points);
        }

        // Satisfying chord resolution sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMatchSound(length);
        }

        Color themeColor = Color.white;
        if (GameManager.Instance != null && GameManager.Instance.IsFeverActive)
        {
            // Rainbow colored explosion during Fever
            float hue = (Time.time * 2f) % 1f;
            themeColor = Color.HSVToRGB(hue, 1f, 1f);
        }
        else if (ColorManager.Instance != null && length > 0)
        {
            themeColor = ColorManager.Instance.GetColor(chain[0].ColorId);
        }

        int particleCount = GameManager.Instance != null && GameManager.Instance.IsFeverActive ? 30 : 15;

        // Spawn particle explosions at each dot's location
        bool hasBomb = false;
        Vector3 bombPosition = Vector3.zero;
        bool hasTimeDot = false;

        foreach (Dot dot in chain)
        {
            if (dot != null)
            {
                SpawnExplosion(dot.transform.position, themeColor, particleCount);
                if (dot.IsBomb)
                {
                    hasBomb = true;
                    bombPosition = dot.transform.position;
                }
                if (dot.IsTimeDot)
                {
                    hasTimeDot = true;
                }
            }
        }

        // Trigger Bomb Pop logic
        if (hasBomb)
        {
            List<Dot> bombTargets = new List<Dot>();
            if (DotSpawner.Instance != null)
            {
                foreach (Dot dot in DotSpawner.Instance.ActiveDots)
                {
                    if (dot != null && !chain.Contains(dot))
                    {
                        if (Vector2.Distance(dot.transform.position, bombPosition) <= 2.2f)
                        {
                            bombTargets.Add(dot);
                        }
                    }
                }
            }
            foreach (Dot target in bombTargets)
            {
                if (target != null)
                {
                    Color targetColor = target.IsMetal ? Color.gray : (target.IsFrozen ? Color.cyan : (ColorManager.Instance != null ? ColorManager.Instance.GetColor(target.ColorId) : Color.white));
                    SpawnExplosion(target.transform.position, targetColor, 20);
                    target.DestroyDot();
                }
            }
        }

        // Time Dot bonus logic
        if (hasTimeDot)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoints(250); // Huge bonus points
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowComboFeedback("TIME BONUS! +250 PTS", Color.green);
            }
        }

        // Clear adjacent Frozen dots (Metal obstacles cannot be cleared by normal pops, only bombs)
        List<Dot> obstaclesToClear = new List<Dot>();
        if (DotSpawner.Instance != null)
        {
            foreach (Dot dot in DotSpawner.Instance.ActiveDots)
            {
                if (dot != null && dot.IsFrozen)
                {
                    foreach (Dot chainDot in chain)
                    {
                        if (chainDot != null && Vector2.Distance(dot.transform.position, chainDot.transform.position) <= 1.15f)
                        {
                            if (!obstaclesToClear.Contains(dot))
                            {
                                obstaclesToClear.Add(dot);
                            }
                            break;
                        }
                    }
                }
            }
        }

        foreach (Dot obstacle in obstaclesToClear)
        {
            if (obstacle != null)
            {
                SpawnExplosion(obstacle.transform.position, Color.cyan, 20);
                obstacle.DestroyDot();
            }
        }

        // Juiciness configurations based on milestones & combos
        float shakeIntensity = 0.05f;
        float shakeDuration = 0.15f;
        float slowMoScale = 1f;
        float slowMoDuration = 0f;
        string feedbackMsg = "";
        int milestoneLevel = -1;

        if (length >= 50)
        {
            shakeIntensity = 0.6f;
            shakeDuration = 0.8f;
            slowMoScale = 0.15f;
            slowMoDuration = 0.7f;
            feedbackMsg = "GODLIKE CHAOS! x10";
            milestoneLevel = 3;
        }
        else if (length >= 20)
        {
            shakeIntensity = 0.4f;
            shakeDuration = 0.5f;
            slowMoScale = 0.25f;
            slowMoDuration = 0.5f;
            feedbackMsg = "HYPER BLAST! x5";
            milestoneLevel = 2;
        }
        else if (length >= 10)
        {
            shakeIntensity = 0.25f;
            shakeDuration = 0.35f;
            slowMoScale = 0.4f;
            slowMoDuration = 0.3f;
            feedbackMsg = "DECIMATOR! x3";
            milestoneLevel = 1;
        }
        else if (length >= 5)
        {
            shakeIntensity = 0.12f;
            shakeDuration = 0.2f;
            feedbackMsg = $"COMBO x{multiplier}!";
            milestoneLevel = 0;
        }

        // Milestone celebration
        if (milestoneLevel > 0)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMilestoneSound(milestoneLevel);
            }

            // Central massive explosion burst
            Vector3 centerPos = GetChainCenter(chain);
            SpawnExplosion(centerPos, themeColor, 40);
        }

        // Shake Camera
        if (length >= 5)
        {
            TriggerCameraShake(shakeIntensity, shakeDuration);
        }

        // Trigger Slow Motion
        if (slowMoDuration > 0f)
        {
            TriggerSlowMotion(slowMoScale, slowMoDuration);
        }

        // UI Flashes and Text punch scale animations
        if (UIManager.Instance != null && !string.IsNullOrEmpty(feedbackMsg))
        {
            UIManager.Instance.TriggerScreenFlash(themeColor, shakeDuration);
            
            string title = "COMBO";
            if (length >= 50) title = "GODLIKE!";
            else if (length >= 20) title = "HYPER BLAST!";
            else if (length >= 10) title = "DECIMATOR!";

            UIManager.Instance.ShowComboFeedback(title, $"x{multiplier}", $"{length} CHAIN!", themeColor);
        }
    }

    private Vector3 GetChainCenter(List<Dot> chain)
    {
        if (chain == null || chain.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero;
        foreach (Dot d in chain)
        {
            if (d != null) sum += d.transform.position;
        }
        return sum / chain.Count;
    }

    public void SpawnExplosion(Vector3 position, Color color, int count)
    {
        if (particlePrefab == null) return;
        GameObject explosion = Instantiate(particlePrefab, position, Quaternion.identity);
        
        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = color;
            var emission = ps.emission;
            emission.burstCount = 1;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, count));
            ps.Play();
        }
    }

    public void TriggerCameraShake(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            cam.transform.position = originalCamPos;
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine(cam, intensity, duration));
    }

    public void TriggerSlowMotion(float scale, float duration)
    {
        if (slowMoCoroutine != null)
        {
            StopCoroutine(slowMoCoroutine);
        }
        slowMoCoroutine = StartCoroutine(SlowMotionCoroutine(scale, duration));
    }

    private IEnumerator ShakeCoroutine(Camera cam, float intensity, float duration)
    {
        originalCamPos = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Shake continues in real time
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            cam.transform.position = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);
            yield return null;
        }

        cam.transform.position = originalCamPos;
        shakeCoroutine = null;
    }

    private IEnumerator SlowMotionCoroutine(float slowScale, float duration)
    {
        Time.timeScale = slowScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        float elapsed = 0f;
        float lerpDuration = 0.25f;
        while (elapsed < lerpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(slowScale, 1f, elapsed / lerpDuration);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        slowMoCoroutine = null;
    }
}
