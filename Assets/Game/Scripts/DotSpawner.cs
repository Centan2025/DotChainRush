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

    private bool bossGuaranteedSpawned = false;

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

            Vector3 dynamicSpawnPos = GetDynamicSpawnPosition();
            if (IsDynamicSpawnAreaClear(dynamicSpawnPos))
            {
                SpawnDot(dynamicSpawnPos);
            }
            else
            {
                Debug.Log($"[Spawner] Spawn BLOCKED: dot near spawn location {dynamicSpawnPos}");
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
        bossGuaranteedSpawned = false;
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResetDifficulty();
        }
    }

    /// <summary>Yeni bir boss seviyesine geçildiğinde boss spawn garantisini sıfırlar.</summary>
    public void ResetBossSpawnState()
    {
        bossGuaranteedSpawned = false;
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

    public GameObject SpawnSpecificDotAtPosition(TopTipi type, Vector3 spawnPos)
    {
        if (ObjectPool.Instance == null) return null;

        GameObject dotGO = ObjectPool.Instance.Get();
        dotGO.transform.position = spawnPos;
        dotGO.transform.rotation = Quaternion.identity;
        
        Dot dot = dotGO.GetComponent<Dot>();
        if (dot == null)
        {
            dot = dotGO.AddComponent<Dot>();
        }

        DotType dotType = (DotType)type;
        int colorId = GetColorIdForType(type);
        dot.Init(colorId, dotType, OnDotLifeEnded);

        if (BossDimensionManager.Instance != null && BossDimensionManager.Instance.IsBossLevelActive)
        {
            BossDimensionManager.Instance.OverrideSpawn(dot);
        }

        activeDots.Add(dot);
        return dotGO;
    }

    public GameObject SpawnDot(Vector3 spawnPos)
    {
        if (ObjectPool.Instance == null) return null;

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
        bool isLevel10 = GameBrain.Instance != null && GameBrain.Instance.CurrentLevelConfig != null && GameBrain.Instance.CurrentLevelConfig.id == 10;
        
        if (GameBrain.Instance != null && GameBrain.Instance.CurrentLevelConfig != null)
        {
            allowed = GameBrain.Instance.CurrentLevelConfig.allowedBalls;
        }

        if (allowed == null || allowed.Count == 0)
        {
            allowed = new List<TopTipi> { TopTipi.KirmiziTop, TopTipi.MaviTop, TopTipi.YesilTop, TopTipi.SariTop, TopTipi.MorTop };
        }

        TopTipi chosenType = allowed[0];

        if (isLevel10)
        {
            float roll = Random.value;
            if (roll < 0.20f) chosenType = TopTipi.KirmiziTop;
            else if (roll < 0.40f) chosenType = TopTipi.MaviTop;
            else if (roll < 0.60f) chosenType = TopTipi.YesilTop;
            else if (roll < 0.75f) chosenType = TopTipi.SariTop;
            else if (roll < 0.90f) chosenType = TopTipi.MorTop;
            else if (roll < 0.95f) chosenType = TopTipi.Bomba;
            else if (roll < 0.98f) chosenType = TopTipi.AgirTop;
            else chosenType = TopTipi.Gokkusagi;

            if (ChaosOrbBossSystem.Instance != null && ChaosOrbBossSystem.Instance.ShouldSpawnFakeBall())
            {
                chosenType = TopTipi.SahteTop;
            }
        }
        else
        {
            // Weighted random selection based on BalanceDB.spawn + mutations
            float totalWeight = 0f;
            foreach (var b in allowed)
            {
                totalWeight += GetSpawnWeight(b);
            }

            float r = Random.value * totalWeight;
            float sum = 0f;
            foreach (var b in allowed)
            {
                sum += GetSpawnWeight(b);
                if (r <= sum)
                {
                    chosenType = b;
                    break;
                }
            }

            // Boss level: guarantee first spawn is a boss, then 60% chance on subsequent spawns
            bool isBossLevelNow = GameBrain.Instance != null && GameBrain.Instance.CurrentLevelConfig != null && GameBrain.Instance.CurrentLevelConfig.isBossLevel;
            Debug.Log($"[Spawner] SpawnDot called | isBossLevel={isBossLevelNow} | bossGuaranteedSpawned={bossGuaranteedSpawned} | chosenSoFar={chosenType}");

            if (isBossLevelNow)
            {
                if (!bossGuaranteedSpawned)
                {
                    chosenType = GameBrain.Instance.CurrentLevelConfig.bossType;
                    bossGuaranteedSpawned = true;
                    Debug.Log($"[Spawner] BOSS GUARANTEED → {chosenType}");
                }
                else if (Random.value < 0.60f)
                {
                    chosenType = GameBrain.Instance.CurrentLevelConfig.bossType;
                    Debug.Log($"[Spawner] BOSS 60% ROLL → {chosenType}");
                }
                else
                {
                    Debug.Log($"[Spawner] Boss roll missed → normal type {chosenType}");
                }
            }
        }

        DotType type = (DotType)chosenType;
        int colorId = GetColorIdForType(chosenType);

        dot.Init(colorId, type, OnDotLifeEnded);

        if (BossDimensionManager.Instance != null && BossDimensionManager.Instance.IsBossLevelActive)
        {
            BossDimensionManager.Instance.OverrideSpawn(dot);
        }

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

    private float GetSpawnWeight(TopTipi b)
    {
        float w = BalanceDB.spawn.ContainsKey(b) ? BalanceDB.spawn[b] : 1f;

        if (GameBrain.Instance != null)
        {
            if (b == TopTipi.Bomba || b == TopTipi.Ates || b == TopTipi.Nukleer)
            {
                w += GameBrain.Instance.GetMutationValue("BombChanceMod") * 10f;
            }
            if (b == TopTipi.Gokkusagi || b == TopTipi.VoidRainbow)
            {
                w += GameBrain.Instance.GetMutationValue("RainbowChanceMod") * 10f;
            }
            if (b == TopTipi.Magnet)
            {
                w += GameBrain.Instance.GetMutationValue("MagnetChanceMod") * 10f;
            }

            // Unlocked special balls
            if (b == TopTipi.Bosluk && GameBrain.Instance.GetMutationValue("UnlockVoidBall") > 0) w += 8f;
            if (b == TopTipi.Elektrik && GameBrain.Instance.GetMutationValue("UnlockElectricBall") > 0) w += 8f;
            if (b == TopTipi.Teleport && GameBrain.Instance.GetMutationValue("UnlockTeleportBall") > 0) w += 8f;
            if (b == TopTipi.ZamanBukucu && GameBrain.Instance.GetMutationValue("UnlockTimeBenderBall") > 0) w += 8f;
            if (b == TopTipi.Kuantum && GameBrain.Instance.GetMutationValue("UnlockQuantumBall") > 0) w += 8f;
            if (b == TopTipi.Glitch && GameBrain.Instance.GetMutationValue("UnlockGlitchBall") > 0) w += 8f;
            if (b == TopTipi.GravityCore && GameBrain.Instance.GetMutationValue("UnlockGravityCoreBall") > 0) w += 8f;
            if (b == TopTipi.Omega && GameBrain.Instance.GetMutationValue("UnlockOmegaBall") > 0) w += 8f;
            if (b == TopTipi.PrestigeBoss && GameBrain.Instance.GetMutationValue("UnlockPrestigeBall") > 0) w += 8f;
        }

        // Apply RainbowDampener boss adaptation
        if (BossDimensionManager.Instance != null && 
            BossDimensionManager.Instance.IsBossLevelActive && 
            BossDimensionManager.Instance.Adaptation.counterAbility == "RainbowDampener")
        {
            if (b == TopTipi.Gokkusagi || b == TopTipi.VoidRainbow || b == TopTipi.GokkusagiKani)
            {
                w = 0f;
            }
        }

        return w;
    }

    private Vector3 GetDynamicSpawnPosition()
    {
        Vector2 g = Physics2D.gravity;
        
        float playWidth = maxX - minX;
        int columnCount = Mathf.Max(3, Mathf.FloorToInt(playWidth / 0.8f) + 1);
        int randomCol = Random.Range(0, columnCount);
        float step = (columnCount > 1) ? (maxX - minX) / (columnCount - 1) : 0f;
        float microOffset = Random.Range(-0.06f, 0.06f);
        
        // If gravity is upward (gravity.y > 2.0f)
        if (g.y > 2.0f)
        {
            float spawnX = minX + randomCol * step + microOffset;
            // Spawn exactly on top of the bottom wall (approx -3.6 + 0.22 radius)
            float spawnYBottom = -3.38f; 
            return new Vector3(spawnX, spawnYBottom, 0f);
        }
        // If gravity is leftward (gravity.x < -2.0f)
        else if (g.x < -2.0f)
        {
            // Cancel the 0.15f margin so dot perfectly hugs the right wall
            float spawnXRight = maxX + 0.15f; 
            float spawnYPos = Random.Range(-2.5f, 2.0f);
            return new Vector3(spawnXRight, spawnYPos, 0f);
        }
        // If gravity is rightward (gravity.x > 2.0f)
        else if (g.x > 2.0f)
        {
            // Cancel the 0.15f margin so dot perfectly hugs the left wall
            float spawnXLeft = minX - 0.15f; 
            float spawnYPos = Random.Range(-2.5f, 2.0f);
            return new Vector3(spawnXLeft, spawnYPos, 0f);
        }
        
        float defaultSpawnX = minX + randomCol * step + microOffset;
        return new Vector3(defaultSpawnX, spawnY, 0f);
    }

    private bool IsDynamicSpawnAreaClear(Vector3 spawnPos)
    {
        for (int i = 0; i < activeDots.Count; i++)
        {
            if (activeDots[i] != null && Vector2.Distance(activeDots[i].transform.position, spawnPos) < 0.6f)
            {
                return false;
            }
        }
        return true;
    }
}
