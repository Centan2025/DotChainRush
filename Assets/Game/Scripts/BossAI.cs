using System.Collections.Generic;
using UnityEngine;

public enum BossTrait
{
    Shielded,        // Requires 50+ combo or specific pops to damage
    GravityFlipper,  // Flipped gravity randomly
    ColorGlitcher,   // Randomly changes dot colors
    TimeSqueezer,    // Speeds up level timer countdown
    ElementSpawners, // Constantly spawns element hazard dots
    LavaHazards      // Obstacles appear on board periodically
}

[System.Serializable]
public class BossAI
{
    public List<BossTrait> traits = new List<BossTrait>();

    public void GenerateBossTraits(int seed)
    {
        Random.State oldState = Random.state;
        Random.InitState(seed);

        traits.Clear();

        int traitCount = Random.Range(2, 5); // 2 to 4 random traits
        for (int i = 0; i < traitCount; i++)
        {
            BossTrait t = (BossTrait)Random.Range(0, 6);
            if (!traits.Contains(t))
            {
                traits.Add(t);
            }
        }

        Random.state = oldState;
    }
}
