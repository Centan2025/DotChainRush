using System;
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

        foreach (TopTipi type in Enum.GetValues(typeof(TopTipi)))
        {
            if (BalanceDB.IsNormalColor(type) || BalanceDB.IsBoss(type))
                continue; // Do not nerf/buff basic colors or bosses dynamically

            float win = TelemetrySystem.winRate.ContainsKey(type) ? TelemetrySystem.winRate[type] : 0.5f;
            float use = TelemetrySystem.usage.ContainsKey(type) ? (float)TelemetrySystem.usage[type] / maxUsage : 0f;

            float power = win * 0.7f + use * 0.3f;

            if (power > 0.75f)
            {
                Nerf(type);
            }
            else if (power < 0.25f)
            {
                Buff(type);
            }
        }
    }

    private void Nerf(TopTipi b)
    {
        if (BalanceDB.spawn.ContainsKey(b))
        {
            BalanceDB.spawn[b] = Mathf.Max(0.02f, BalanceDB.spawn[b] * 0.7f);
        }
        if (BalanceDB.unlockOffset.ContainsKey(b))
        {
            BalanceDB.unlockOffset[b] = Mathf.Min(200, BalanceDB.unlockOffset[b] + 5);
        }
    }

    private void Buff(TopTipi b)
    {
        if (BalanceDB.spawn.ContainsKey(b))
        {
            BalanceDB.spawn[b] = Mathf.Min(0.5f, BalanceDB.spawn[b] * 1.2f);
        }
        if (BalanceDB.unlockOffset.ContainsKey(b))
        {
            BalanceDB.unlockOffset[b] = Mathf.Max(2, BalanceDB.unlockOffset[b] - 2);
        }
    }
}
