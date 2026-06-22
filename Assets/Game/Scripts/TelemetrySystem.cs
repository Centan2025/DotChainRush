using System;
using System.Collections.Generic;

public static class TelemetrySystem
{
    public static Dictionary<TopTipi, int> usage = new Dictionary<TopTipi, int>();
    public static Dictionary<TopTipi, float> winRate = new Dictionary<TopTipi, float>();
    public static Dictionary<TopTipi, int> pops = new Dictionary<TopTipi, int>();
    public static Dictionary<TopTipi, int> wins = new Dictionary<TopTipi, int>();
    public static Dictionary<TopTipi, int> losses = new Dictionary<TopTipi, int>();

    public static float failRate = 0f;
    public static int runsPlayed = 0;
    public static int levelsPlayed = 0;
    public static int levelsFailed = 0;

    // Boss Telemetry
    public static float lastBossCompletionTime = 0f;
    public static int lastBossFailedAttempts = 0;
    public static int lastBossComboCount = 0;
    public static Dictionary<string, int> bossDamageSources = new Dictionary<string, int>();

    static TelemetrySystem()
    {
        ResetTelemetry();
    }

    public static void ResetTelemetry()
    {
        usage.Clear();
        winRate.Clear();
        pops.Clear();
        wins.Clear();
        losses.Clear();

        foreach (TopTipi type in Enum.GetValues(typeof(TopTipi)))
        {
            usage[type] = 0;
            winRate[type] = 0.5f; // start at 50%
            pops[type] = 0;
            wins[type] = 0;
            losses[type] = 0;
        }

        failRate = 0f;
        runsPlayed = 0;
        levelsPlayed = 0;
        levelsFailed = 0;
    }

    public static void RecordPop(TopTipi type, int amount = 1)
    {
        if (pops.ContainsKey(type))
        {
            pops[type] += amount;
            usage[type] += amount;
        }
    }

    public static void RecordLevelResult(bool won, List<TopTipi> ballsInLevel)
    {
        levelsPlayed++;
        if (!won)
        {
            levelsFailed++;
        }

        failRate = (float)levelsFailed / levelsPlayed;

        foreach (var b in ballsInLevel)
        {
            if (won)
            {
                if (wins.ContainsKey(b)) wins[b]++;
            }
            else
            {
                if (losses.ContainsKey(b)) losses[b]++;
            }

            int total = wins[b] + losses[b];
            if (total > 0)
            {
                winRate[b] = (float)wins[b] / total;
            }
        }
    }

    public static void RecordBossResult(bool won, float completionTime, int comboCount)
    {
        if (won)
        {
            lastBossCompletionTime = completionTime;
            lastBossComboCount = comboCount;
            // Next time a boss is encountered, we use these metrics
            // Reset attempts for next boss
            lastBossFailedAttempts = 0; 
        }
        else
        {
            lastBossFailedAttempts++;
        }
    }

    public static void RecordBossDamageSource(string sourceName)
    {
        if (!bossDamageSources.ContainsKey(sourceName))
        {
            bossDamageSources[sourceName] = 0;
        }
        bossDamageSources[sourceName]++;
    }
}
