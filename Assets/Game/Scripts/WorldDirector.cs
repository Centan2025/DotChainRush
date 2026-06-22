using System.Collections.Generic;
using UnityEngine;

public class WorldDirector : MonoBehaviour
{
    public static WorldDirector Instance { get; private set; }

    [Header("Generation Settings")]
    public float minY = -2.5f;
    public float maxY = 2.0f;
    public float minX = -2.2f;
    public float maxX = 2.2f;

    private List<GameObject> spawnedElements = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LevelConfig GenerateLevel(int levelId)
    {
        // 1) Define Level Identity
        LevelConfig config = new LevelConfig();
        config.id = levelId;
        config.seed = levelId * 12345 + 987;
        config.biome = GetBiomeForLevel(levelId);
        config.theme = GetThemeForLevel(levelId);

        // Seed-based randomizer for deterministic generation
        Random.State oldState = Random.state;
        Random.InitState(config.seed);

        float adaptiveModifier = 1.0f;
        if (AdaptiveDirector.Instance != null)
        {
            adaptiveModifier = AdaptiveDirector.Instance.difficultyModifier;
        }

        // 2) Difficulty Rating
        float baseDifficulty = (0.2f + 0.5f * Mathf.Log(levelId, 2f)) * adaptiveModifier;
        if (levelId > 50) baseDifficulty += (levelId - 50) * 0.02f; // scale harder for endgame
        config.difficultyRating = Mathf.Clamp(baseDifficulty, 0.1f, 3.5f);

        // 3) Objective Selection
        config.objectiveType = GetObjectiveTypeForLevel(levelId);
        config.comboRequirement = 5 + (levelId / 5);
        config.destroyTargetCount = 3 + (levelId / 4);

        if (config.objectiveType == LevelObjectiveType.Color)
        {
            config.targetColor = (TopTipi)Random.Range((int)TopTipi.KirmiziTop, (int)TopTipi.MorTop + 1);
        }
        else if (config.objectiveType == LevelObjectiveType.SpecialBall)
        {
            config.targetSpecialBall = TopTipi.Gokkusagi;
            if (levelId > 20) config.targetSpecialBall = TopTipi.Bomba;
            if (levelId > 40) config.targetSpecialBall = TopTipi.ZamanBukucu;
        }

        // 4) Core Scoring Curve
        int baseScore = 1000 + (levelId * 1500);
        if (levelId % 10 == 0) // Boss level score target
        {
            config.isBossLevel = true;
            config.bossType = GetBossTypeForLevel(levelId);
            config.bossHP = 3 + (levelId / 10) * 2;
            baseScore = Mathf.RoundToInt(baseScore * 1.3f);
        }
        
        float playerAdjustment = 1.0f;
        if (GameBrain.Instance != null && GameBrain.Instance.meta != null)
        {
            playerAdjustment = Mathf.Clamp(GameBrain.Instance.meta.playerSkill, 0.5f, 2.0f);
        }
        config.targetScore = Mathf.RoundToInt(baseScore * config.difficultyRating * playerAdjustment);

        if (levelId == 10)
        {
            config.isBossLevel = true;
            config.bossType = TopTipi.KaosOrbBoss1;
            config.bossHP = 5;
            config.difficultyRating = 0.25f;
            config.baseTimer = 90f;
            config.targetScore = 20800;
        }

        // 5) Clamped Time Limits
        float calculatedTime = 90f - (levelId * 0.5f);
        config.baseTimer = Mathf.Clamp(calculatedTime, 30f, 150f);
        if (levelId == 10) config.baseTimer = 90f;

        // 6) Physics settings
        config.spawnRate = Mathf.Max(0.12f, 0.85f - (levelId * 0.0035f));
        config.speed = Mathf.Min(0.85f, 0.12f + (levelId * 0.0035f));
        if (levelId == 10)
        {
            config.spawnRate = 0.8f;
            config.speed = 0.15f;
        }

        // 7) Allowed Balls pool
        config.allowedBalls = BuildAllowedBalls(levelId);

        // 8) Obstacle configurations
        int obstacleTypeCount = Mathf.Min(8, 1 + (levelId / 8));
        for (int i = 0; i < obstacleTypeCount; i++)
        {
            config.obstacleTypes.Add((ObstacleType)Random.Range(0, 10));
        }

        // 9) Potential Chaos Events
        int eventCount = Mathf.Min(3, levelId / 15);
        for (int i = 0; i < eventCount; i++)
        {
            config.potentialEvents.Add((ChaosEventType)Random.Range(1, 9));
        }

        // Set preview description
        if (config.isBossLevel)
        {
            config.previewText = $"BOSS FIGHT: {config.bossType}! Defeat the ultimate boss within {config.baseTimer:F0}s!";
        }
        else
        {
            config.previewText = $"Biome: {config.biome} | Objective: {config.objectiveType} | Target: {config.targetScore} pts!";
        }

        Random.state = oldState;
        return config;
    }

    public void SpawnLevelElements(LevelConfig config)
    {
        ClearSpawnedElements();

        Random.State oldState = Random.state;
        Random.InitState(config.seed);

        if (config.id == 10)
        {
            // Spawn Chaos Arena layout:
            SpawnSpecificObstacle(ObstacleType.MirrorBlock, new Vector3(-1f, 0.5f, 0f));
            SpawnSpecificObstacle(ObstacleType.MirrorBlock, new Vector3(1f, 0.5f, 0f));
            SpawnSpecificObstacle(ObstacleType.MetalBlock, new Vector3(-2f, -0.5f, 0f));
            SpawnSpecificObstacle(ObstacleType.MetalBlock, new Vector3(2f, -0.5f, 0f));
            SpawnSpecificObstacle(ObstacleType.IceBlock, new Vector3(0f, -1.8f, 0f));
        }
        else
        {
            // 1) Spawn Obstacles based on difficulty and level
            int obstacleOffset = AdaptiveDirector.Instance != null ? AdaptiveDirector.Instance.obstacleAmountOffset : 0;
            int obstacleCount = config.isBossLevel ? 3 : Mathf.Clamp(config.id / 6 + 1 + obstacleOffset, 1, 15);
            for (int i = 0; i < obstacleCount; i++)
            {
                SpawnProceduralObstacle(config);
            }

            // 2) Spawn Physics Zones
            int zoneCount = Mathf.Min(4, config.id / 10);
            for (int i = 0; i < zoneCount; i++)
            {
                SpawnProceduralPhysicsZone();
            }

            // 3) Spawn Live Objects (Entities)
            int liveObjectCount = Mathf.Min(3, config.id / 15);
            for (int i = 0; i < liveObjectCount; i++)
            {
                SpawnProceduralLiveObject();
            }

            // 4) Spawn Reward items
            int rewardCount = Mathf.Min(2, 1 + (config.id / 20));
            for (int i = 0; i < rewardCount; i++)
            {
                SpawnProceduralRewardObject();
            }
        }

        Random.state = oldState;
        Debug.Log($"[WorldDirector] Spawned elements for level {config.id} in Biome: {config.biome}");
    }

    public void ClearSpawnedElements()
    {
        foreach (var go in spawnedElements)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
        spawnedElements.Clear();
    }

    private void SpawnSpecificObstacle(ObstacleType type, Vector3 position)
    {
        GameObject obsGo = new GameObject("Obstacle_" + type);
        obsGo.transform.position = position;
        
        SpriteRenderer sr = obsGo.AddComponent<SpriteRenderer>();
        BoxCollider2D bc = obsGo.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.8f, 0.8f);
        bc.isTrigger = (type == ObstacleType.VoidBlock || type == ObstacleType.MirrorBlock);

        Obstacle obs = obsGo.AddComponent<Obstacle>();
        ObstacleConfig obsCfg = ScriptableObject.CreateInstance<ObstacleConfig>();
        obsCfg.type = type;
        obsCfg.maxHealth = type == ObstacleType.MetalBlock ? 3 : (type == ObstacleType.NormalBlock ? 2 : 1);
        obsCfg.rewardType = ObstacleRewardType.Points;
        obsCfg.rewardAmount = 15;

        if (DotChainRushLibrary.Instance != null)
        {
            obsCfg.visualSprite = DotChainRushLibrary.Instance.GetTopSprite(GetTopTipiForObstacle(type));
        }

        obs.InitializeFromConfig(obsCfg);
        spawnedElements.Add(obsGo);
    }

    private void SpawnProceduralObstacle(LevelConfig config)
    {
        if (config.obstacleTypes.Count == 0) return;
        ObstacleType type = config.obstacleTypes[Random.Range(0, config.obstacleTypes.Count)];

        GameObject obsGo = new GameObject("Obstacle_" + type);
        obsGo.transform.position = GetRandomSpawnPos();
        
        SpriteRenderer sr = obsGo.AddComponent<SpriteRenderer>();
        BoxCollider2D bc = obsGo.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.8f, 0.8f);
        bc.isTrigger = (type == ObstacleType.VoidBlock || type == ObstacleType.MirrorBlock);

        Obstacle obs = obsGo.AddComponent<Obstacle>();
        ObstacleConfig obsCfg = ScriptableObject.CreateInstance<ObstacleConfig>();
        obsCfg.type = type;
        obsCfg.maxHealth = type == ObstacleType.MetalBlock ? 3 : (type == ObstacleType.NormalBlock ? 2 : 1);
        obsCfg.rewardType = ObstacleRewardType.Points;
        obsCfg.rewardAmount = 15;

        // Custom Sprites from Library
        if (DotChainRushLibrary.Instance != null)
        {
            obsCfg.visualSprite = DotChainRushLibrary.Instance.GetTopSprite(GetTopTipiForObstacle(type));
        }

        obs.InitializeFromConfig(obsCfg);

        // Add movement behavior procedurally for sliding/rotating blocks
        if (config.id > 15 && Random.value < 0.35f)
        {
            MovingObstacle mover = obsGo.AddComponent<MovingObstacle>();
            mover.movementType = Random.value < 0.5f ? ObstacleMovement.Sliding : ObstacleMovement.Rotating;
            mover.speed = Random.Range(0.8f, 1.5f);
            mover.range = Random.Range(1.0f, 2.0f);
        }

        spawnedElements.Add(obsGo);
    }

    private void SpawnProceduralPhysicsZone()
    {
        PhysicsObjectType type = (PhysicsObjectType)Random.Range(0, 6);
        GameObject zoneGo = new GameObject("PhysicsZone_" + type);
        zoneGo.transform.position = GetRandomSpawnPos();

        SpriteRenderer sr = zoneGo.AddComponent<SpriteRenderer>();
        CircleCollider2D cc = zoneGo.AddComponent<CircleCollider2D>();
        cc.radius = 1.0f;
        cc.isTrigger = true;

        PhysicsObject zone = zoneGo.AddComponent<PhysicsObject>();
        zone.type = type;
        zone.forceMagnitude = type == PhysicsObjectType.BouncePad ? 12f : 5f;
        zone.direction = type == PhysicsObjectType.WindZone ? Vector2.right : Vector2.up;

        // Visual representation for editor/runtime debugging
        if (sr != null)
        {
            switch (type)
            {
                case PhysicsObjectType.BouncePad: sr.color = new Color(0.9f, 0.6f, 0f, 0.4f); break;
                case PhysicsObjectType.MagnetZone: sr.color = new Color(0.1f, 0.1f, 0.8f, 0.2f); break;
                case PhysicsObjectType.WindZone: sr.color = new Color(0.7f, 0.7f, 0.7f, 0.3f); break;
                case PhysicsObjectType.FireZone: sr.color = new Color(1f, 0.2f, 0f, 0.3f); break;
                case PhysicsObjectType.SlowZone: sr.color = new Color(0.5f, 0.2f, 0.8f, 0.2f); break;
                case PhysicsObjectType.ChaosZone: sr.color = new Color(0.9f, 0.1f, 0.9f, 0.3f); break;
            }
        }

        spawnedElements.Add(zoneGo);
    }

    private void SpawnProceduralLiveObject()
    {
        LiveObjectType type = (LiveObjectType)Random.Range(0, 4);
        GameObject liveGo = new GameObject("LiveEntity_" + type);
        liveGo.transform.position = GetRandomSpawnPos();

        SpriteRenderer sr = liveGo.AddComponent<SpriteRenderer>();
        CircleCollider2D cc = liveGo.AddComponent<CircleCollider2D>();
        cc.radius = 0.4f;

        // Needs to collide with dots to bounce
        Rigidbody2D rb = liveGo.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        LiveObject live = liveGo.AddComponent<LiveObject>();
        live.type = type;
        live.speed = Random.Range(1.0f, 2.0f);

        spawnedElements.Add(liveGo);
    }

    private void SpawnProceduralRewardObject()
    {
        RewardObjectType type = (RewardObjectType)Random.Range(0, 3);
        GameObject rewardGo = new GameObject("Reward_" + type);
        rewardGo.transform.position = GetRandomSpawnPos();

        SpriteRenderer sr = rewardGo.AddComponent<SpriteRenderer>();
        BoxCollider2D bc = rewardGo.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(0.5f, 0.5f);
        bc.isTrigger = true;

        RewardObject reward = rewardGo.AddComponent<RewardObject>();
        reward.type = type;
        reward.baseRewardAmount = 1;

        if (sr != null)
        {
            switch (type)
            {
                case RewardObjectType.TreasureBox: sr.color = new Color(0.9f, 0.8f, 0.1f); break;
                case RewardObjectType.MysteryOrb: sr.color = new Color(0.8f, 0f, 0.8f); break;
                case RewardObjectType.Crystal: sr.color = new Color(0f, 0.9f, 0.9f); break;
            }
        }

        spawnedElements.Add(rewardGo);
    }

    private Vector3 GetRandomSpawnPos()
    {
        return new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f
        );
    }

    private string GetBiomeForLevel(int id)
    {
        if (id <= 10) return "Neon City";
        if (id <= 20) return "Firelands";
        if (id <= 40) return "Glacial Ruins";
        if (id <= 60) return "Void Sector";
        if (id <= 80) return "Reality Matrix";
        return "Chaos Zone";
    }

    private string GetThemeForLevel(int id)
    {
        if (id <= 10) return "Spark Grid";
        if (id <= 20) return "Elemental Fury";
        if (id <= 40) return "Mutant Spores";
        if (id <= 60) return "Entropy Drift";
        if (id <= 80) return "Quantum Fault";
        return "Ultimate Rift";
    }

    private LevelObjectiveType GetObjectiveTypeForLevel(int id)
    {
        if (id <= 3) return LevelObjectiveType.Score;
        
        int typeIndex = (id % 6);
        return (LevelObjectiveType)typeIndex;
    }

    private List<TopTipi> BuildAllowedBalls(int level)
    {
        var pool = new List<TopTipi> { TopTipi.KirmiziTop, TopTipi.MaviTop, TopTipi.YesilTop, TopTipi.SariTop, TopTipi.MorTop };
        
        // Add special balls based on level
        if (level > 2) pool.Add(TopTipi.Gokkusagi);
        if (level > 4) pool.Add(TopTipi.HizliTop);
        if (level > 6) pool.Add(TopTipi.Bomba);
        if (level > 8) pool.Add(TopTipi.Zaman);
        if (level > 12) pool.Add(TopTipi.Ates);
        if (level > 15) pool.Add(TopTipi.Altin2x);
        if (level > 25) pool.Add(TopTipi.Ayna);
        if (level > 35) pool.Add(TopTipi.ZamanBukucu);

        return pool;
    }

    private TopTipi GetBossTypeForLevel(int level)
    {
        if (level == 10) return TopTipi.KaosOrbBoss1;
        if (level == 20) return TopTipi.ElementalFuryBoss2;
        if (level == 30) return TopTipi.FlowMasterBoss4;
        if (level == 40) return TopTipi.ZamanLorduBoss;
        return TopTipi.TheVoidFinalBoss;
    }

    private TopTipi GetTopTipiForObstacle(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.MetalBlock: return TopTipi.AgirTop;
            case ObstacleType.IceBlock: return TopTipi.Buz;
            case ObstacleType.ShieldBlock: return TopTipi.Kalkan;
            case ObstacleType.VoidBlock: return TopTipi.Bosluk;
            case ObstacleType.TimeBlock: return TopTipi.Zaman;
            case ObstacleType.VirusBlock: return TopTipi.Virus;
            default: return TopTipi.Kozmik;
        }
    }
}
