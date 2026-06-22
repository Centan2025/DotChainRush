using System.Collections.Generic;
using UnityEngine;

public class SimulationReport
{
    public float averageWinRate;
    public List<int> brokenLevels = new List<int>();
    public List<TopTipi> opBalls = new List<TopTipi>();
    public float difficultyCurveScore; // higher = smoother curve
    public int timeOverflowsCount;
}

public class SimulationTester
{
    public SimulationReport RunSimulation(List<LevelConfigSO> levels, int runCount = 10000)
    {
        SimulationReport report = new SimulationReport();
        if (levels == null || levels.Count == 0) return report;

        int totalWins = 0;
        int totalFails = 0;
        float totalCurveDeviation = 0f;
        
        Dictionary<int, float> lvlWinRates = new Dictionary<int, float>();
        Dictionary<int, int> lvlWins = new Dictionary<int, int>();
        Dictionary<int, int> lvlAttempts = new Dictionary<int, int>();

        // Seed-based random to ensure deterministic simulation results
        Random.State oldState = Random.state;
        Random.InitState(424242);

        for (int i = 0; i < runCount; i++)
        {
            // Pick a level sequentially to distribute simulation runs evenly
            int lvlIdx = i % levels.Count;
            LevelConfigSO lvl = levels[lvlIdx];

            if (!lvlAttempts.ContainsKey(lvl.id))
            {
                lvlAttempts[lvl.id] = 0;
                lvlWins[lvl.id] = 0;
            }

            lvlAttempts[lvl.id]++;

            // Simple statistical AI play model
            float simulatedScore = 0f;
            float timeSpent = 0f;
            float target = lvl.targetScore;

            // Player skill factor (0.5 to 1.5)
            float playerSkill = Random.Range(0.6f, 1.4f);
            float basePopSpeed = 3.5f; // matches popped per second

            while (simulatedScore < target && timeSpent < lvl.baseTimer)
            {
                float delay = lvl.spawnInterval / (basePopSpeed * playerSkill);
                timeSpent += delay;

                // Score per match
                float matchLength = Random.Range(3, 10);
                float multiplier = 1f;
                if (matchLength >= 5) multiplier = 1.5f;

                // Adjust by difficulty scale and ball modifiers
                simulatedScore += matchLength * 10f * multiplier;
            }

            if (simulatedScore >= target && timeSpent <= lvl.baseTimer)
            {
                totalWins++;
                lvlWins[lvl.id]++;
            }
            else
            {
                totalFails++;
                if (timeSpent > lvl.baseTimer)
                {
                    report.timeOverflowsCount++;
                }
            }
        }

        // Calculate statistics per level
        foreach (var lvl in levels)
        {
            int attempts = lvlAttempts.ContainsKey(lvl.id) ? lvlAttempts[lvl.id] : 0;
            int wins = lvlWins.ContainsKey(lvl.id) ? lvlWins[lvl.id] : 0;
            
            float rate = attempts > 0 ? (float)wins / attempts : 0f;
            lvlWinRates[lvl.id] = rate;

            // Mark levels with win rate < 15% or > 95% as broken/unbalanced
            if (rate < 0.15f || rate > 0.95f)
            {
                report.brokenLevels.Add(lvl.id);
            }

            // Calculate curve deviation (target ideal is 60% win rate)
            totalCurveDeviation += Mathf.Abs(rate - 0.60f);
        }

        report.averageWinRate = (float)totalWins / runCount;
        report.difficultyCurveScore = 1f - (totalCurveDeviation / levels.Count);
        
        Random.state = oldState;
        return report;
    }
}
