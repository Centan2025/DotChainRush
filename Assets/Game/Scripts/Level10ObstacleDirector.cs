using UnityEngine;

public class Level10ObstacleDirector : MonoBehaviour
{
    public static Level10ObstacleDirector Instance { get; private set; }

    private int currentPhase = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetPhase(int phase)
    {
        currentPhase = phase;
        Debug.Log($"[Level10ObstacleDirector] Switched to Phase {phase}");
    }

    public TopTipi GetObstacleToSpawn()
    {
        if (currentPhase == 1)
        {
            // Ice only
            return TopTipi.Buz;
        }
        else if (currentPhase == 2)
        {
            // Metal + Mirror only
            return Random.value < 0.5f ? TopTipi.Ayna : TopTipi.AgirTop;
        }
        else
        {
            // Phase 3: All obstacle types (Ice, Mirror, Metal, Fake)
            TopTipi[] allObstacles = { TopTipi.Buz, TopTipi.Ayna, TopTipi.AgirTop, TopTipi.SahteTop };
            return allObstacles[Random.Range(0, allObstacles.Length)];
        }
    }
}
