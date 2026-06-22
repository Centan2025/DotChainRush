using System.Collections.Generic;
using UnityEngine;

public class AutoDifficultyCurve
{
    public float CalculateDifficultyFactor(int level)
    {
        // Smooth logarithmic difficulty scaling curve from level 1 (0.2) to level 200 (2.5)
        float baseFactor = 0.2f + 0.5f * Mathf.Log(level, 2f);
        return Mathf.Clamp(baseFactor, 0.1f, 2.5f);
    }

    public void AdjustCurve(List<LevelConfigSO> levels, Dictionary<int, float> simulationWinRates)
    {
        // Correct difficulty factor dynamically based on simulated win rates to smooth spikes/walls
        foreach (var lvl in levels)
        {
            if (lvl == null) continue;

            int id = lvl.id;
            float winRate = simulationWinRates.ContainsKey(id) ? simulationWinRates[id] : 0.5f;

            if (winRate < 0.25f)
            {
                // Too hard: Lower gravity scale, increase spawn intervals, or lower target score
                lvl.difficultyFactor = Mathf.Max(0.1f, lvl.difficultyFactor * 0.9f);
                lvl.gravityScale = Mathf.Max(0.08f, lvl.gravityScale * 0.9f);
                lvl.spawnInterval = Mathf.Min(2.0f, lvl.spawnInterval * 1.05f);
                lvl.targetScore = Mathf.RoundToInt(lvl.targetScore * 0.9f);
                Debug.LogFormat("[AutoDifficultyCurve] Eased Level {0} (Sim Win Rate: {1:P1})", id, winRate);
            }
            else if (winRate > 0.85f)
            {
                // Too easy: Increase gravity, decrease spawn interval, increase target score
                lvl.difficultyFactor = Mathf.Min(2.5f, lvl.difficultyFactor * 1.05f);
                lvl.gravityScale = Mathf.Min(1.8f, lvl.gravityScale * 1.05f);
                lvl.spawnInterval = Mathf.Max(0.12f, lvl.spawnInterval * 0.95f);
                lvl.targetScore = Mathf.RoundToInt(lvl.targetScore * 1.05f);
                Debug.LogFormat("[AutoDifficultyCurve] Hardened Level {0} (Sim Win Rate: {1:P1})", id, winRate);
            }
        }
    }
}
