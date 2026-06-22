using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MetaLearner
{
    public Dictionary<TopTipi, float> weights = new Dictionary<TopTipi, float>();

    public void Train()
    {
        // Calculate max usage to normalize usage rates
        int maxUsage = 1;
        foreach (var u in TelemetrySystem.usage.Values)
        {
            if (u > maxUsage) maxUsage = u;
        }

        foreach (TopTipi type in Enum.GetValues(typeof(TopTipi)))
        {
            if (!weights.ContainsKey(type))
            {
                weights[type] = 1.0f;
            }

            float win = TelemetrySystem.winRate.ContainsKey(type) ? TelemetrySystem.winRate[type] : 0.5f;
            float use = TelemetrySystem.usage.ContainsKey(type) ? (float)TelemetrySystem.usage[type] / maxUsage : 0f;

            // Power score combines usage and win rate
            float score = win * 0.7f + use * 0.3f;

            // Lerp target is (1.0f - score). High power -> low weight (spawn less), low power -> high weight (spawn more)
            weights[type] = Mathf.Lerp(weights[type], Mathf.Clamp01(1f - score), 0.05f);
        }
    }
}
