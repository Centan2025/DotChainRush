using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BalanceAI
{
    public void Evaluate()
    {
        int maxUsage = 1;
        foreach (var u in TelemetrySystem.usage.Values)
        {
            if (u > maxUsage) maxUsage = u;
        }

        List<TopTipi> opBalls = new List<TopTipi>();
        List<TopTipi> weakBalls = new List<TopTipi>();

        foreach (TopTipi type in Enum.GetValues(typeof(TopTipi)))
        {
            if (BalanceDB.IsNormalColor(type) || BalanceDB.IsBoss(type))
                continue; // Basic colors and bosses are not tuned randomly

            float win = TelemetrySystem.winRate.ContainsKey(type) ? TelemetrySystem.winRate[type] : 0.5f;
            float use = TelemetrySystem.usage.ContainsKey(type) ? (float)TelemetrySystem.usage[type] / maxUsage : 0f;

            float power = win * 0.7f + use * 0.3f;

            // 1) Detect OP Balls (winrate > 80% and significant usage)
            if (power > 0.78f)
            {
                opBalls.Add(type);
                Nerf(type);
            }
            // 2) Detect Weak Balls (winrate < 20%)
            else if (power < 0.22f)
            {
                weakBalls.Add(type);
                Buff(type);
            }
        }

        // 3) Log diagnostics
        if (opBalls.Count > 0)
        {
            Debug.LogWarning("[BalanceAI] OP Balls Detected: " + string.Join(", ", opBalls));
        }
        if (weakBalls.Count > 0)
        {
            Debug.Log("[BalanceAI] Weak/Underperforming Balls Detected: " + string.Join(", ", weakBalls));
        }

        // 4) Check for broken levels (extremely high fail rate)
        if (TelemetrySystem.levelsPlayed > 0 && TelemetrySystem.failRate > 0.6f)
        {
            Debug.LogWarning($"[BalanceAI] Detection: Broken/Unfair difficulty spike. Global fail rate: {TelemetrySystem.failRate:P0}. Softening game parameters.");
            if (AdaptiveDirector.Instance != null)
            {
                AdaptiveDirector.Instance.difficultyModifier = Mathf.Max(0.5f, AdaptiveDirector.Instance.difficultyModifier * 0.8f);
            }
        }
    }

    private void Nerf(TopTipi b)
    {
        if (BalanceDB.spawn.ContainsKey(b))
        {
            BalanceDB.spawn[b] = Mathf.Max(0.01f, BalanceDB.spawn[b] * 0.75f);
        }
        if (BalanceDB.unlockOffset.ContainsKey(b))
        {
            BalanceDB.unlockOffset[b] = Mathf.Min(200, BalanceDB.unlockOffset[b] + 3);
        }
        Debug.Log($"[BalanceAI] Nerfed spawn probability & increased unlock level of OP Ball: {b}");
    }

    private void Buff(TopTipi b)
    {
        if (BalanceDB.spawn.ContainsKey(b))
        {
            BalanceDB.spawn[b] = Mathf.Min(0.6f, BalanceDB.spawn[b] * 1.25f);
        }
        if (BalanceDB.unlockOffset.ContainsKey(b))
        {
            BalanceDB.unlockOffset[b] = Mathf.Max(2, BalanceDB.unlockOffset[b] - 2);
        }
        Debug.Log($"[BalanceAI] Buffed spawn probability & lowered unlock level of underperforming Ball: {b}");
    }
}
