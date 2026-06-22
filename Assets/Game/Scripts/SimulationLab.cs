using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SimulationLab
{
    public void Run10000Tests(LevelGenerator gen)
    {
        Debug.Log("[SimulationLab] Initiating 10,000 game simulation tests to calibrate meta balancing...");

        int totalScore = 0;
        int winCount = 0;
        int failCount = 0;

        Dictionary<TopTipi, int> simUsage = new Dictionary<TopTipi, int>();
        foreach (TopTipi type in System.Enum.GetValues(typeof(TopTipi)))
        {
            simUsage[type] = 0;
        }

        // Run tests
        for (int i = 0; i < 10000; i++)
        {
            int levelNum = (i % 200) + 1;
            LevelConfig config = gen.GenerateLevel(levelNum);

            // Simulate simple game outcome
            int scoreReached = 0;
            bool win = true;

            // Simple AI player model: pops groups of dots
            int turns = Random.Range(10, 30);
            for (int t = 0; t < turns; t++)
            {
                // Select a random ball type from level's allowed list
                if (config.allowedBalls.Count > 0)
                {
                    TopTipi chosen = config.allowedBalls[Random.Range(0, config.allowedBalls.Count)];
                    simUsage[chosen]++;

                    int matchLength = Random.Range(3, 8);
                    int multiplier = 1;
                    if (matchLength >= 5) multiplier = 2;
                    
                    scoreReached += matchLength * 10 * multiplier;
                }
            }

            totalScore += scoreReached;
            if (scoreReached >= config.targetScore)
            {
                winCount++;
            }
            else
            {
                failCount++;
                win = false;
            }
        }

        float avgScore = (float)totalScore / 10000;
        float winRate = (float)winCount / 10000 * 100f;

        Debug.LogFormat("[SimulationLab] Completed 10,000 tests! Average Score: {0:F0}, Win Rate: {1:F2}%, Fails: {2}", 
            avgScore, winRate, failCount);

        // Find top used special ball types
        List<KeyValuePair<TopTipi, int>> usageList = new List<KeyValuePair<TopTipi, int>>();
        foreach (var pair in simUsage)
        {
            if (!BalanceDB.IsNormalColor(pair.Key))
            {
                usageList.Add(pair);
            }
        }

        usageList.Sort((x, y) => y.Value.CompareTo(x.Value));

        Debug.Log("[SimulationLab] Meta Balancing Report - Top Special Balls Populated in Levels:");
        for (int k = 0; k < Mathf.Min(5, usageList.Count); k++)
        {
            Debug.LogFormat(" - {0}: {1} spawns simulated", usageList[k].Key, usageList[k].Value);
        }
    }
}
