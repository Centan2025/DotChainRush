using UnityEngine;

public class ArenaBuilder : MonoBehaviour
{
    public static ArenaBuilder Instance { get; private set; }

    [Header("Arena Prefabs")]
    public GameObject mirrorPrefab;
    public GameObject metalPrefab;
    public GameObject icePrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void BuildChaosArena()
    {
        Debug.Log("[ArenaBuilder] Building Chaos Arena for Level 10");
        
        // Spawn Mirrors (Middle-Top)
        SpawnBlock(TopTipi.Ayna, new Vector3(-1.5f, 2.5f, 0));
        SpawnBlock(TopTipi.Ayna, new Vector3(1.5f, 2.5f, 0));

        // Spawn Metal (Sides)
        SpawnBlock(TopTipi.AgirTop, new Vector3(-2.8f, 0f, 0));
        SpawnBlock(TopTipi.AgirTop, new Vector3(2.8f, 0f, 0));

        // Spawn Ice (Bottom)
        SpawnBlock(TopTipi.Buz, new Vector3(0f, -3.5f, 0));
    }

    private void SpawnBlock(TopTipi type, Vector3 position)
    {
        if (DotSpawner.Instance != null)
        {
            // Usually dot spawner can create these if they are represented as TopTipi
            GameObject spawned = DotSpawner.Instance.SpawnSpecificDotAtPosition(type, position);
            if (spawned != null)
            {
                Rigidbody2D rb = spawned.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic; // Lock them in place as arena borders
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
        }
    }
}
