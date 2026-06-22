using UnityEngine;

public enum PlayerArchetype
{
    Casual,
    Average,
    Hardcore
}

public class PlayerMetaLearning
{
    public PlayerArchetype CurrentArchetype { get; private set; } = PlayerArchetype.Average;
    public float DifficultyBias { get; private set; } = 1.0f; // Multiplier applied to physics/goals

    public void AnalyzeTelemetry(float failRate, int runsPlayed, float avgWinTime)
    {
        // Simple heuristic rules to classify player skill archetype
        if (failRate > 0.45f)
        {
            CurrentArchetype = PlayerArchetype.Casual;
            DifficultyBias = 0.82f; // scale down difficulty
        }
        else if (failRate < 0.18f && runsPlayed > 5)
        {
            CurrentArchetype = PlayerArchetype.Hardcore;
            DifficultyBias = 1.25f; // scale up difficulty
        }
        else
        {
            CurrentArchetype = PlayerArchetype.Average;
            DifficultyBias = 1.0f;
        }

        Debug.LogFormat("[PlayerMetaLearning] Player classified as {0} (Difficulty Bias: {1:F2})", 
            CurrentArchetype, DifficultyBias);
    }
}
