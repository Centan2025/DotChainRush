using System.Collections.Generic;
using UnityEngine;

public class AAABalanceEngine
{
    public float WinRateThresholdUpper = 0.72f;
    public float WinRateThresholdLower = 0.35f;

    public void EvaluateAndBalance(List<BallConfig> configs, Dictionary<TopTipi, float> winRates, Dictionary<TopTipi, float> usageRates)
    {
        foreach (var config in configs)
        {
            if (config == null) continue;

            TopTipi type = config.type;
            float win = winRates.ContainsKey(type) ? winRates[type] : 0.5f;
            float use = usageRates.ContainsKey(type) ? usageRates[type] : 0.1f;

            // Power Score combines win rate impact and usage frequency
            float powerScore = win * 0.7f + use * 0.3f;

            if (powerScore > WinRateThresholdUpper)
            {
                // Nerf: Reduce spawn modifier, delay unlock level, scale down points/effect size
                config.currentSpawnModifier = Mathf.Max(0.1f, config.currentSpawnModifier * 0.85f);
                config.unlockLevelOffset = Mathf.Min(100, config.unlockLevelOffset + 2);
                config.effectScale = Mathf.Max(0.5f, config.effectScale * 0.9f);
                Debug.LogFormat("[AAABalanceEngine] NERFED {0} (Power Score: {1:F2})", type, powerScore);
            }
            else if (powerScore < WinRateThresholdLower)
            {
                // Buff: Increase spawn modifier, lower unlock level offset, scale up points/effect size
                config.currentSpawnModifier = Mathf.Min(3.0f, config.currentSpawnModifier * 1.15f);
                config.unlockLevelOffset = Mathf.Max(-50, config.unlockLevelOffset - 1);
                config.effectScale = Mathf.Min(2.0f, config.effectScale * 1.1f);
                Debug.LogFormat("[AAABalanceEngine] BUFFED {0} (Power Score: {1:F2})", type, powerScore);
            }
        }
    }
}
