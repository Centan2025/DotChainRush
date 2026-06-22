using System.Collections.Generic;
using UnityEngine;

public class DotSpawner : MonoBehaviour
{
    public static DotSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private float spawnY = 3.5f; // Spawn inside visible play area (below header)
    [SerializeField] private float minX = -2.2f;
    [SerializeField] private float maxX = 2.2f;

    public float SpawnY
    {
        get => spawnY;
        set => spawnY = value;
    }

    private float spawnTimer = 0f;
    private float dangerTimer = 0f;
    private readonly List<Dot> activeDots = new List<Dot>();

    public List<Dot> ActiveDots => activeDots;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // Fetch dynamic interval from DifficultyManager (Fever mode scales it in GameManager/Time.timeScale)
        float interval = DifficultyManager.Instance != null 
            ? DifficultyManager.Instance.SpawnInterval 
            : 0.7f;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= interval)
        {
            spawnTimer = 0f;

            // Don't spawn if the area near spawn height is already occupied
            // This prevents dots from piling above the play area frame
            bool spawnAreaClear = true;
            for (int i = 0; i < activeDots.Count; i++)
            {
                if (activeDots[i] != null && activeDots[i].transform.position.y > spawnY - 0.5f)
                {
                    spawnAreaClear = false;
                    break;
                }
            }

            if (spawnAreaClear)
            {
                SpawnDot();
            }
        }

        // Find the highest settled dot Y position (ignoring dots near spawn height)
        float highestSettledY = -99f;
        for (int i = 0; i < activeDots.Count; i++)
        {
            Dot dot = activeDots[i];
            if (dot != null && dot.transform.position.y < 3.0f)
            {
                Rigidbody2D dotRb = dot.GetComponent<Rigidbody2D>();
                if (dotRb != null && dotRb.linearVelocity.magnitude < 0.15f)
                {
                    if (dot.transform.position.y > highestSettledY)
                    {
                        highestSettledY = dot.transform.position.y;
                    }
                }
            }
        }

        // Interpolate camera background color based on stack height
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            float startY = 0.6f;
            float endY = 3.2f;
            float dangerT = Mathf.Clamp01((highestSettledY - startY) / (endY - startY));
            Color normalBg = new Color(0.12f, 0.12f, 0.16f);
            Color warningBg = new Color(0.32f, 0.06f, 0.08f); // Deep crimson red alert color
            mainCam.backgroundColor = Color.Lerp(normalBg, warningBg, dangerT);
        }

        // Determine danger phase
        if (highestSettledY >= 3.2f) // RED (Critical) - adjusted for narrower play area
        {
            dangerTimer += Time.deltaTime;
            float remaining = Mathf.Max(0f, 3.0f - dangerTimer);
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetDangerActive(true, $"DANGER! {remaining:F1}s");
            }
            
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.TriggerCameraShake(0.08f, 0.05f);
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDangerWarningSound(true); // Fast tick
            }

            if (dangerTimer >= 3.0f)
            {
                GameManager.Instance.GameOver();
                dangerTimer = 0f;
            }
        }
        else if (highestSettledY >= 2.4f) // YELLOW (Warning) - adjusted for narrower play area
        {
            dangerTimer = 0f; // Reset critical timer, but keep warning visual
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetDangerActive(true, "WARNING!");
            }
            
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.TriggerCameraShake(0.02f, 0.05f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDangerWarningSound(false); // Slow tick
            }
        }
        else // GREEN (Normal)
        {
            dangerTimer = 0f;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetDangerActive(false);
            }
        }
    }

    public void InitializeSpawner()
    {
        ClearAll();
        spawnTimer = 0f;
        dangerTimer = 0f;
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResetDifficulty();
        }
    }

    public void ClearAll()
    {
        for (int i = activeDots.Count - 1; i >= 0; i--)
        {
            Dot dot = activeDots[i];
            if (dot != null)
            {
                dot.StopLifeCycle();
                if (ObjectPool.Instance != null)
                {
                    ObjectPool.Instance.ReturnToPool(dot.gameObject);
                }
                else
                {
                    Destroy(dot.gameObject);
                }
            }
        }
        activeDots.Clear();
    }
    private int GetColorIdForType(TopTipi type)
    {
        if (type == TopTipi.KirmiziTop) return 0;
        if (type == TopTipi.MaviTop) return 1;
        if (type == TopTipi.YesilTop) return 2;
        if (type == TopTipi.SariTop) return 3;
        if (type == TopTipi.MorTop) return 4;

        // Wildcards / Obstacles / Bosses have no color
        if (type == TopTipi.Gokkusagi || type == TopTipi.VoidRainbow || type == TopTipi.Sonsuzluk || 
            BalanceDB.IsBoss(type) || type == TopTipi.AgirTop || type == TopTipi.Buz || 
            type == TopTipi.ElitAgir || type == TopTipi.OlumTopu)
        {
            return -1;
        }

        // Other powerups can be chained with a random color
        return Random.Range(0, 5);
    }

    public GameObject SpawnDot()
    {
        if (ObjectPool.Instance == null) return null;

        // Calculate dynamic column count based on screen width (minX to maxX) and dot diameter (~0.52f)
        float dotSize = 0.52f;
        float playWidth = maxX - minX;
        
        // Ensure at least 3 columns, otherwise calculate how many dots fit with spacing (approx every 0.8f units)
        int columnCount = Mathf.Max(3, Mathf.FloorToInt(playWidth / 0.8f) + 1);
        
        int randomCol = Random.Range(0, columnCount);
        float step = (columnCount > 1) ? (maxX - minX) / (columnCount - 1) : 0f;
        
        // Add a micro random offset (+/- 0.05f) to prevent dots from stacking in perfect, unnatural vertical lines
        float microOffset = Random.Range(-0.06f, 0.06f);
        float spawnX = minX + randomCol * step + microOffset;
        
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);
        
        GameObject dotGO = ObjectPool.Instance.Get();
        dotGO.transform.position = spawnPos;
        dotGO.transform.rotation = Quaternion.identity;
        
        Dot dot = dotGO.GetComponent<Dot>();
        if (dot == null)
        {
            dot = dotGO.AddComponent<Dot>();
        }

        // Get allowed balls from GameBrain
        List<TopTipi> allowed = null;
        if (GameBrain.Instance != null && GameBrain.Instance.CurrentLevelConfig != null)
        {
            allowed = GameBrain.Instance.CurrentLevelConfig.allowedBalls;
        }

        if (allowed == null || allowed.Count == 0)
        {
            allowed = new List<TopTipi> { TopTipi.KirmiziTop, TopTipi.MaviTop, TopTipi.YesilTop, TopTipi.SariTop, TopTipi.MorTop };
        }

        // Weighted random selection based on BalanceDB.spawn
        float totalWeight = 0f;
        foreach (var b in allowed)
        {
            float w = BalanceDB.spawn.ContainsKey(b) ? BalanceDB.spawn[b] : 1f;
            totalWeight += w;
        }

        TopTipi chosenType = allowed[0];
        float r = Random.value * totalWeight;
        float sum = 0f;
        foreach (var b in allowed)
        {
            float w = BalanceDB.spawn.ContainsKey(b) ? BalanceDB.spawn[b] : 1f;
            sum += w;
            if (r <= sum)
            {
                chosenType = b;
                break;
            }
        }

        // Periodically spawn boss/hazard if boss level is active
        if (GameBrain.Instance != null && GameBrain.Instance.CurrentLevelConfig != null && GameBrain.Instance.CurrentLevelConfig.isBossLevel)
        {
            // 20% chance to spawn boss type directly as a target or element spawner
            if (Random.value < 0.20f)
            {
                chosenType = GameBrain.Instance.CurrentLevelConfig.bossType;
            }
        }

        DotType type = (DotType)chosenType;
        int colorId = GetColorIdForType(chosenType);

        dot.Init(colorId, type, OnDotLifeEnded);
        activeDots.Add(dot);

        return dotGO;
    }

    public void DespawnDot(Dot dot)
    {
        if (dot == null) return;

        dot.StopLifeCycle();
        activeDots.Remove(dot);

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(dot.gameObject);
        }
        else
        {
            Destroy(dot.gameObject);
        }
    }

    public void SetSpawnRange(float min, float max)
    {
        minX = min;
        maxX = max;
    }

    private void OnDotLifeEnded(Dot dot)
    {
        DespawnDot(dot);
    }
}
