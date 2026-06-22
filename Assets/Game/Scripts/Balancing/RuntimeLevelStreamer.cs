using System.Collections.Generic;
using UnityEngine;

public class RuntimeLevelStreamer
{
    private Dictionary<int, LevelConfigSO> cache = new Dictionary<int, LevelConfigSO>();
    private AutoDifficultyCurve curveCalculator = new AutoDifficultyCurve();
    private ProceduralBossAI bossGenerator = new ProceduralBossAI();

    public LevelConfigSO StreamLevel(int levelId, List<BallConfig> availableBalls)
    {
        if (cache.ContainsKey(levelId) && cache[levelId] != null)
        {
            return cache[levelId];
        }

        // Generate ScriptableObject in memory dynamically
        LevelConfigSO level = ScriptableObject.CreateInstance<LevelConfigSO>();
        level.id = levelId;
        level.isBossLevel = (levelId % 10 == 0);
        level.theme = level.isBossLevel ? "Titan Arena" : "Neon Grid";
        level.baseTimer = 90f;
        level.difficultyFactor = curveCalculator.CalculateDifficultyFactor(levelId);

        // Core physics calculations based on level curve
        level.spawnInterval = Mathf.Max(0.15f, 0.85f - (levelId * 0.0035f));
        level.gravityScale = Mathf.Min(0.85f, 0.12f + (levelId * 0.0035f));
        level.targetScore = 1000 + levelId * 1500;

        // Select and assign allowed balls config references
        foreach (var ball in availableBalls)
        {
            if (ball.unlockLevel + ball.unlockLevelOffset <= levelId)
            {
                level.allowedBalls.Add(ball);
            }
        }

        if (level.isBossLevel)
        {
            level.bossConfig = bossGenerator.GenerateBoss(levelId, levelId * 999);
            level.targetScore = Mathf.RoundToInt(level.targetScore * 1.3f);
            level.previewText = $"BOSS SEVİYESİ! Dev {level.bossConfig.bossType} topunu yok et!";
        }
        else
        {
            level.previewText = $"Seviye {levelId}: Neon akış hızlanıyor, combolarla hedefi tamamla!";
        }

        // Cache level to avoid repeated instantiation
        cache[levelId] = level;

        // Memory cleanup to keep cache size low on mobile devices
        if (cache.Count > 15)
        {
            // Evict oldest or farthest cache keys
            int oldestKey = -1;
            foreach (var key in cache.Keys)
            {
                if (key != levelId)
                {
                    oldestKey = key;
                    break;
                }
            }
            if (oldestKey != -1)
            {
                cache.Remove(oldestKey);
            }
        }

        return level;
    }

    public void FlushCache()
    {
        cache.Clear();
    }
}
