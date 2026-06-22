using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SimulationIssue
{
    public int levelId;
    public string problemType;
    public string details;
    public string suggestedFix;
}

[System.Serializable]
public class SimulationLab
{
    public List<SimulationIssue> detectedIssues = new List<SimulationIssue>();

    public string Run10000Tests(WorldDirector director)
    {
        Debug.Log("[SimulationLab] Initiating 10,000 game simulation tests to calibrate level director...");
        detectedIssues.Clear();

        int totalScore = 0;
        int winCount = 0;
        int failCount = 0;
        int timeOutCount = 0;

        Dictionary<int, int> levelWins = new Dictionary<int, int>();
        Dictionary<int, int> levelTimeOuts = new Dictionary<int, int>();
        Dictionary<int, int> levelAttempts = new Dictionary<int, int>();

        // Run 10,000 tests across 200 levels
        for (int i = 0; i < 10000; i++)
        {
            int levelNum = (i % 200) + 1;
            LevelConfig config = director != null ? director.GenerateLevel(levelNum) : WorldDirector.Instance.GenerateLevel(levelNum);

            if (!levelAttempts.ContainsKey(levelNum))
            {
                levelAttempts[levelNum] = 0;
                levelWins[levelNum] = 0;
                levelTimeOuts[levelNum] = 0;
            }
            levelAttempts[levelNum]++;

            // Simulate run
            float simulatedScore = 0f;
            float timeRemaining = config.baseTimer;
            bool win = false;

            // Player skill factor variation
            float playerSkill = Random.Range(0.6f, 1.4f);
            int turns = Random.Range(15, 35);

            for (int t = 0; t < turns; t++)
            {
                // Simulate time delta for turn
                float turnDuration = (config.spawnRate * 2.5f) / playerSkill;
                timeRemaining -= turnDuration;

                if (timeRemaining <= 0)
                {
                    levelTimeOuts[levelNum]++;
                    timeOutCount++;
                    break;
                }

                // Simulate pop
                int matchLength = Random.Range(3, 8);
                float multiplier = 1.0f;
                if (matchLength >= 5) multiplier = 1.5f;

                // Add time bonuses from time dots
                if (Random.value < 0.12f)
                {
                    float timeBonus = Random.Range(5f, 10f);
                    timeRemaining = Mathf.Min(150f, timeRemaining + timeBonus); // clamped max time
                }

                float turnScore = matchLength * 10f * multiplier * config.difficultyRating * playerSkill;
                simulatedScore += turnScore;

                if (simulatedScore >= config.targetScore)
                {
                    win = true;
                    levelWins[levelNum]++;
                    winCount++;
                    break;
                }
            }

            totalScore += Mathf.RoundToInt(simulatedScore);
            if (!win) failCount++;
        }

        // 2) Analyze Results & Generate Diagnostic Fix Suggestions
        for (int levelNum = 1; levelNum <= 200; levelNum++)
        {
            int attempts = levelAttempts.ContainsKey(levelNum) ? levelAttempts[levelNum] : 0;
            int wins = levelWins.ContainsKey(levelNum) ? levelWins[levelNum] : 0;
            int timeouts = levelTimeOuts.ContainsKey(levelNum) ? levelTimeOuts[levelNum] : 0;

            float winRate = attempts > 0 ? (float)wins / attempts : 0f;
            float timeoutRate = attempts > 0 ? (float)timeouts / attempts : 0f;

            if (winRate < 0.20f)
            {
                detectedIssues.Add(new SimulationIssue
                {
                    levelId = levelNum,
                    problemType = "Difficulty Spike / Wall",
                    details = $"Win rate is extremely low ({winRate:P0}) over {attempts} attempts.",
                    suggestedFix = "Reduce targetScore by 20%, increase spawnRate (interval) by 10%, or reduce initial gravityScale."
                });
            }
            else if (winRate > 0.95f)
            {
                detectedIssues.Add(new SimulationIssue
                {
                    levelId = levelNum,
                    problemType = "Too Easy",
                    details = $"Win rate is too high ({winRate:P0}) over {attempts} attempts.",
                    suggestedFix = "Increase targetScore by 15%, increase gravityScale, or speed up spawnRate (interval)."
                });
            }

            if (timeoutRate > 0.40f)
            {
                detectedIssues.Add(new SimulationIssue
                {
                    levelId = levelNum,
                    problemType = "Time Problem / Timeouts",
                    details = $"Players time out in {timeoutRate:P0} of runs.",
                    suggestedFix = "Increase level baseTimer by 15 seconds, or increase the spawn rate of Time Balls."
                });
            }
        }

        // Write report file
        string reportText = GenerateTextReport(winCount, failCount, timeOutCount);
        string reportPath = Path.Combine(Application.dataPath, "BalanceReport.txt");
        try
        {
            File.WriteAllText(reportPath, reportText);
            Debug.Log("[SimulationLab] Report exported successfully to: Assets/BalanceReport.txt");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[SimulationLab] Failed to write report file: " + ex.Message);
        }

        return reportText;
    }

    private string GenerateTextReport(int wins, int fails, int timeouts)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("============================================================");
        sb.AppendLine("DOT GAME — AAA SIMULATION LAB BALANCE REPORT");
        sb.AppendLine("============================================================");
        sb.AppendLine($"Total Virtual Runs: 10,000");
        sb.AppendLine($"Global Wins: {wins} ({ (float)wins / 10000:P1} win rate)");
        sb.AppendLine($"Global Fails: {fails}");
        sb.AppendLine($"Global Time-Outs: {timeouts}");
        sb.AppendLine($"Total Issues Detected: {detectedIssues.Count}");
        sb.AppendLine("============================================================");
        sb.AppendLine();
        sb.AppendLine("LEVEL DIAGNOSTICS & RECOMMENDED FIXES:");
        sb.AppendLine("------------------------------------------------------------");

        foreach (var issue in detectedIssues)
        {
            sb.AppendLine($"Level {issue.levelId} | Problem: {issue.problemType}");
            sb.AppendLine($"  - Details: {issue.details}");
            sb.AppendLine($"  - Suggested Fix: {issue.suggestedFix}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
