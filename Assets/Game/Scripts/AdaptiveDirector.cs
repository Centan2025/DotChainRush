using UnityEngine;

public class AdaptiveDirector : MonoBehaviour
{
    public static AdaptiveDirector Instance { get; private set; }

    [Header("Outputs (Adapted parameters for WorldDirector)")]
    public float difficultyModifier = 1.0f;
    public int obstacleAmountOffset = 0;
    public float eventIntensityMultiplier = 1.0f;
    public float rewardQualityFactor = 1.0f;

    [Header("Tracking Stats")]
    private float totalLevelTimes = 0f;
    private int completedLevelsCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RecordLevelTime(float time)
    {
        totalLevelTimes += time;
        completedLevelsCount++;
    }

    public float GetAverageTime()
    {
        if (completedLevelsCount == 0) return 45f; // default average time
        return totalLevelTimes / completedLevelsCount;
    }

    public void AnalyzeAndAdapt()
    {
        // 1) Read Inputs from Telemetry & Meta Learner
        float playerSkill = 1.0f;
        if (GameBrain.Instance != null && GameBrain.Instance.meta != null)
        {
            playerSkill = GameBrain.Instance.meta.playerSkill;
        }

        float winRate = 1.0f - TelemetrySystem.failRate;
        float failRate = TelemetrySystem.failRate;
        float avgTime = GetAverageTime();

        Debug.Log($"[AdaptiveDirector] Analyzing Player Performance. Skill: {playerSkill:F2}, WinRate: {winRate:P0}, AvgTime: {avgTime:F1}s");

        // 2) Adapt Difficulty Modifier
        // If winrate is high (>80%) and skill is high, boost difficulty. If failrate is high (>40%), drop difficulty.
        if (winRate > 0.80f && playerSkill > 1.1f)
        {
            difficultyModifier = Mathf.Min(2.5f, difficultyModifier * 1.15f);
            obstacleAmountOffset = Mathf.Min(5, obstacleAmountOffset + 1);
            eventIntensityMultiplier = Mathf.Min(2.0f, eventIntensityMultiplier * 1.1f);
            rewardQualityFactor = Mathf.Min(2.0f, rewardQualityFactor * 1.05f); // slightly better rewards for high difficulty
            Debug.Log("[AdaptiveDirector] Player is OP! Hardening difficulty parameters.");
        }
        else if (failRate > 0.35f || (completedLevelsCount > 3 && avgTime > 80f))
        {
            difficultyModifier = Mathf.Max(0.5f, difficultyModifier * 0.85f);
            obstacleAmountOffset = Mathf.Max(-4, obstacleAmountOffset - 1);
            eventIntensityMultiplier = Mathf.Max(0.4f, eventIntensityMultiplier * 0.85f);
            rewardQualityFactor = Mathf.Min(2.5f, rewardQualityFactor * 1.15f); // drop better rewards to help struggling player
            Debug.Log("[AdaptiveDirector] Player is struggling. Easing difficulty parameters and boosting rewards.");
        }
        else
        {
            // Stable performance: gently steer towards baseline
            difficultyModifier = Mathf.Lerp(difficultyModifier, playerSkill, 0.2f);
            eventIntensityMultiplier = Mathf.Lerp(eventIntensityMultiplier, 1.0f, 0.1f);
        }

        // Boss level specific adaptation
        if (BossDimensionManager.Instance != null && BossDimensionManager.Instance.IsBossLevelActive)
        {
            float bossCompletionTime = TelemetrySystem.lastBossCompletionTime;
            int bossFailedAttempts = TelemetrySystem.lastBossFailedAttempts;
            int bossComboCount = TelemetrySystem.lastBossComboCount;

            if (bossFailedAttempts > 0)
            {
                difficultyModifier = Mathf.Max(0.5f, difficultyModifier - (bossFailedAttempts * 0.1f));
                Debug.Log($"[AdaptiveDirector] Boss failed {bossFailedAttempts} times. Reducing difficultyModifier to {difficultyModifier:F2}");
            }
            else if (bossCompletionTime > 0 && bossCompletionTime < 60f && bossComboCount >= 8)
            {
                difficultyModifier = Mathf.Min(2.5f, difficultyModifier + 0.15f);
                Debug.Log($"[AdaptiveDirector] Boss defeated quickly with high combo! Increasing difficultyModifier to {difficultyModifier:F2}");
            }
        }

        Debug.Log($"[AdaptiveDirector] Outputs -> DiffMod: {difficultyModifier:F2}, ObstacleOffset: {obstacleAmountOffset}, EventIntensity: {eventIntensityMultiplier:F2}, RewardQuality: {rewardQualityFactor:F2}");
    }
}
