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

        if (BossDimensionManager.Instance != null && BossDimensionManager.Instance.IsBossLevelActive)
        {
            BossDimensionManager.Instance.OnChainProcessed(chain);
        }

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

        // ChainLimiter score penalty
        if (BossDimensionManager.Instance != null && 
            BossDimensionManager.Instance.IsBossLevelActive && 
            BossDimensionManager.Instance.Adaptation.counterAbility == "ChainLimiter" && 
            length >= 6)
        {
            points /= 2;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowComboFeedback("CHAIN LIMITER: POINTS HALVED!", Color.red);
            }
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

        // Execute unique behaviors for the 85 dot types
        bool hasBomb = false;
        Vector3 bombPosition = Vector3.zero;
        float bombRadius = 0.8f;
        bool clearAllDots = false;
        bool triggerFever = false;
        int scoreMultiplier = 1;

        foreach (Dot dot in chain)
        {
            if (dot != null)
            {
                // Record telemetry pop metric
                TelemetrySystem.RecordPop((TopTipi)dot.Type, 1);

                SpawnExplosion(dot.transform.position, themeColor, particleCount);

                // Specific dot type triggers
                switch (dot.Type)
                {
                    case DotType.Bomba:
                    case DotType.Ates:
                        hasBomb = true;
                        bombPosition = dot.transform.position;
                        bombRadius = 1.2f;
                        break;

                    case DotType.Nukleer:
                        hasBomb = true;
                        bombPosition = dot.transform.position;
                        bombRadius = 3.5f; // Screen-wide explosion
                        break;

                    case DotType.Zaman:
                        AddTimeToTimer(5f);
                        if (UIManager.Instance != null) UIManager.Instance.ShowComboFeedback("ZAMAN +5s", Color.green);
                        break;

                    case DotType.SonsuzZaman:
                        AddTimeToTimer(12f);
                        if (UIManager.Instance != null) UIManager.Instance.ShowComboFeedback("Sonsuz Zaman! +12s", Color.green);
                        break;

                    case DotType.ZamanBukucu:
                        AddTimeToTimer(8f);
                        if (UIManager.Instance != null) UIManager.Instance.ShowComboFeedback("Zaman Bükücü! +8s", Color.cyan);
                        break;

                    case DotType.Can:
                        AddTimeToTimer(10f);
                        break;

                    case DotType.Altin2x:
                    case DotType.Skor2x:
                    case DotType.Kozmik:
                        scoreMultiplier *= 2;
                        break;

                    case DotType.Jackpot:
                        int currentGold = SaveSystem.LoadInt("GoldCoins", 12450);
                        SaveSystem.SaveInt("GoldCoins", currentGold + 250);
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.UpdateGoldAmount(currentGold + 250);
                            UIManager.Instance.ShowComboFeedback("JACKPOT! +250 GOLD", Color.yellow);
                        }
                        break;

                    case DotType.Elektrik:
                    case DotType.Rezonans:
                        // Clear all other dots of the same color
                        TriggerColorClear(dot.ColorId, chain);
                        break;

                    case DotType.GerceklikKirici:
                        clearAllDots = true;
                        break;

                    case DotType.Omega:
                        triggerFever = true;
                        break;
                }

                // Handle Boss Dot Damage
                if (BalanceDB.IsBoss((TopTipi)dot.Type))
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.DamageBoss();
                    }
                }
            }
        }

        // Apply score multipliers from dot types
        if (scoreMultiplier > 1)
        {
            points *= scoreMultiplier;
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoints(points - (points / scoreMultiplier)); // Add remaining difference
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowComboFeedback($"MULTİPLİER x{scoreMultiplier}!", Color.magenta);
            }
        }

        if (triggerFever && GameManager.Instance != null && !GameManager.Instance.IsFeverActive)
        {
            // Set progress to threshold to trigger fever next update
            GameManager.Instance.RegisterChain(25);
        }

        if (clearAllDots)
        {
            TriggerScreenClear(chain);
        }

        // Trigger Bomb Pop logic
        if (hasBomb)
        {
            if (BossDimensionManager.Instance != null && 
                BossDimensionManager.Instance.IsBossLevelActive && 
                BossDimensionManager.Instance.Adaptation.counterAbility == "BombShield")
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowComboFeedback("BOMB SHIELDED!", Color.magenta);
                }
            }
            else
            {
                List<Dot> bombTargets = new List<Dot>();
                if (DotSpawner.Instance != null)
                {
                    List<Dot> snapshot = new List<Dot>(DotSpawner.Instance.ActiveDots);
                    foreach (Dot dot in snapshot)
                    {
                        if (dot != null && !chain.Contains(dot))
                        {
                            if (Vector2.Distance(dot.transform.position, bombPosition) <= bombRadius)
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
        }

        // Clear adjacent Frozen dots (Metal obstacles cannot be cleared by normal pops, only bombs)
        List<Dot> obstaclesToClear = new List<Dot>();
        if (DotSpawner.Instance != null)
        {
            // Copy the list to avoid modifying ActiveDots during iteration
            List<Dot> frozenSnapshot = new List<Dot>(DotSpawner.Instance.ActiveDots);
            foreach (Dot dot in frozenSnapshot)
            {
                if (dot != null && dot.IsFrozen)
                {
                    foreach (Dot chainDot in chain)
                    {
                        if (chainDot != null && Vector2.Distance(dot.transform.position, chainDot.transform.position) <= 0.75f) // Only touching frozen dots
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

    private static Sprite metalShardSprite;

    private Sprite GetMetalShardSprite()
    {
        if (metalShardSprite != null) return metalShardSprite;

        // Create a 32x32 texture representing an elegant 4-pointed star sparkle flare
        Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = (x - 15.5f) / 15.5f;
                float dy = (y - 15.5f) / 15.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Star flare calculation: horizontal and vertical thin glowing rays
                float flareX = Mathf.Max(0f, 1f - Mathf.Abs(dx) * 1.3f) * Mathf.Max(0f, 1f - Mathf.Abs(dy) * 6.5f);
                float flareY = Mathf.Max(0f, 1f - Mathf.Abs(dy) * 1.3f) * Mathf.Max(0f, 1f - Mathf.Abs(dx) * 6.5f);
                // Central bright core
                float core = Mathf.Max(0f, 1f - dist * 1.8f);

                float intensity = Mathf.Clamp01(flareX + flareY + core);

                if (intensity > 0.05f)
                {
                    // High-end UI sparkle color blending (white hot core fading to bright edge)
                    float spec = Mathf.Pow(intensity, 2f);
                    float finalVal = Mathf.Clamp01(intensity * 0.6f + spec * 0.4f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, finalVal));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();

        metalShardSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        return metalShardSprite;
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
            main.startSize = new ParticleSystem.MinMaxCurve(0.24f, 0.58f); // Larger, more visible metallic shards
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f); // Lives slightly longer to show off spin

            // Enable dynamic rotation over lifetime to spin the metal shards
            var rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-450f, 450f); // Spin even faster for glint effect

            // Assign the shiny metallic shard sprite to the particle system
            var textureSheet = ps.textureSheetAnimation;
            if (textureSheet.enabled)
            {
                textureSheet.SetSprite(0, GetMetalShardSprite());
            }

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

    public int collectedTimeBonusCount = 0;

    public void ResetComboManager()
    {
        collectedTimeBonusCount = 0;
    }

    private void AddTimeToTimer(float seconds)
    {
        float boost = 0f;
        if (GameBrain.Instance != null)
        {
            boost = GameBrain.Instance.GetMutationValue("TimeBallBoost");
        }
        seconds *= (1f + boost);

        float factor = 1.0f;
        if (collectedTimeBonusCount == 1) factor = 0.7f;
        else if (collectedTimeBonusCount == 2) factor = 0.4f;
        else if (collectedTimeBonusCount >= 3) factor = 0.1f;

        collectedTimeBonusCount++;
        float scaledSeconds = seconds * factor;

        var rtc = FindAnyObjectByType<RadialTimerController>();
        if (rtc != null)
        {
            float targetTime = Mathf.Clamp(rtc.CurrentTime + scaledSeconds, 0f, 150f);
            rtc.CurrentTime = targetTime;
            rtc.totalTime = Mathf.Clamp(rtc.totalTime, 30f, 150f);
        }
        var ct = FindAnyObjectByType<CircularTimer>();
        if (ct != null)
        {
            float targetTime = Mathf.Clamp(ct.CurrentTime + scaledSeconds, 0f, 150f);
            ct.CurrentTime = targetTime;
            ct.totalTime = Mathf.Clamp(ct.totalTime, 30f, 150f);
        }
    }

    private void TriggerColorClear(int colorId, List<Dot> excluded)
    {
        if (DotSpawner.Instance == null) return;
        List<Dot> snapshot = new List<Dot>(DotSpawner.Instance.ActiveDots);
        foreach (Dot target in snapshot)
        {
            if (target != null && !excluded.Contains(target) && target.ColorId == colorId && !target.IsObstacle)
            {
                Color targetColor = ColorManager.Instance != null ? ColorManager.Instance.GetColor(colorId) : Color.white;
                SpawnExplosion(target.transform.position, targetColor, 15);
                target.DestroyDot();
            }
        }
    }

    private void TriggerScreenClear(List<Dot> excluded)
    {
        if (DotSpawner.Instance == null) return;
        List<Dot> snapshot = new List<Dot>(DotSpawner.Instance.ActiveDots);
        foreach (Dot target in snapshot)
        {
            if (target != null && !excluded.Contains(target) && !target.IsObstacle)
            {
                Color targetColor = ColorManager.Instance != null && target.ColorId >= 0 ? ColorManager.Instance.GetColor(target.ColorId) : Color.white;
                SpawnExplosion(target.transform.position, targetColor, 15);
                target.DestroyDot();
            }
        }
    }
}
