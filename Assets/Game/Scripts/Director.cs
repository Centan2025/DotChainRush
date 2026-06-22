using UnityEngine;

[System.Serializable]
public class Director
{
    public float currentSpeedScale = 1.0f;
    public float currentSpawnScale = 1.0f;

    public void Adjust()
    {
        // Skill metric: 1.0 is perfect play, 0.0 is constant failing
        float skill = 1f - TelemetrySystem.failRate;

        if (skill > 0.7f)
        {
            // High skill: increase speed scale and spawn speed
            currentSpeedScale = 1.15f;
            currentSpawnScale = 0.85f; // lower interval = faster spawn
        }
        else if (skill < 0.3f)
        {
            // Low skill: reduce speed scale and slow spawn
            currentSpeedScale = 0.85f;
            currentSpawnScale = 1.15f; // higher interval = slower spawn
        }
        else
        {
            currentSpeedScale = 1.0f;
            currentSpawnScale = 1.0f;
        }

        // Apply global timescale slightly based on speed scale
        Time.timeScale = currentSpeedScale;
    }
}
