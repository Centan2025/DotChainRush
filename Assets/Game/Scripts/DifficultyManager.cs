using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public int ActiveLevel { get; private set; } = 1;
    public int ActiveGoal { get; private set; } = 1000;
    public float TimeElapsed { get; private set; }

    // Dynamic gameplay variables from LevelData
    public float SpawnInterval { get; private set; } = 0.85f;
    public float GravityScale { get; private set; } = 0.12f;
    public float ConnectionCooldown { get; private set; } = 0f;
    public float SpecialDotChance { get; private set; } = 0f;
    public float FastDotChance { get; private set; } = 0f;
    public float ObstacleChance { get; private set; } = 0f;
    public float BombChance { get; private set; } = 0f;
    public float TimeDotChance { get; private set; } = 0f;

    // Current level metadata for HUD
    public string LevelTitle { get; private set; } = "First Spark";
    public string LevelSubtitle { get; private set; } = "TUTORIAL I";
    public string PhaseName { get; private set; } = "Öğretme";

    [Header("Debug Settings")]
    [SerializeField, Tooltip("Editor'da oyunu bu bölümden başlatmak için kullanın.")] private int debugStartLevel = 1;

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

    private void Start()
    {
        ResetDifficulty();
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        TimeElapsed += Time.deltaTime;
    }

    public void ResetDifficulty()
    {
        TimeElapsed = 0f;
        SetLevel(debugStartLevel > 0 ? debugStartLevel : 1);
    }

    public int GetGoalForLevel(int lvl)
    {
        LevelInfo info = LevelData.GetLevel(lvl);
        return info.TargetScore;
    }

    public void SetLevel(int lvl)
    {
        if (GameBrain.Instance != null)
        {
            GameBrain.Instance.SetCurrentLevel(lvl);
            var config = GameBrain.Instance.CurrentLevelConfig;
            if (config != null)
            {
                ActiveLevel = config.id;
                ActiveGoal = config.targetScore;
                SpawnInterval = config.spawnRate;
                GravityScale = config.speed;

                // Adjust by player run mutations
                float spawnMod = GameBrain.Instance.GetMutationValue("SpawnRateMod");
                float speedMod = GameBrain.Instance.GetMutationValue("GravityMod");

                SpawnInterval = Mathf.Max(0.12f, SpawnInterval + spawnMod);
                GravityScale = Mathf.Max(0.08f, GravityScale + speedMod);

                SpecialDotChance = 0.15f;
                FastDotChance = 0.15f;
                ObstacleChance = 0.1f;
                BombChance = 0.08f;
                TimeDotChance = 0.08f;

                LevelTitle = config.theme;
                LevelSubtitle = config.isBossLevel ? "BOSS FIGHT" : "LEVEL " + config.id;
                PhaseName = config.isBossLevel ? "Boss" : "Mücadele";
                ConnectionCooldown = 0.25f + Mathf.Min(0.35f, 0.04f * (ActiveLevel - 1));
                return;
            }
        }

        LevelInfo info = LevelData.GetLevel(lvl);

        ActiveLevel = info.Level;
        ActiveGoal = info.TargetScore;
        SpawnInterval = info.SpawnInterval;
        GravityScale = info.GravityScale;
        SpecialDotChance = info.SpecialDotChance;
        FastDotChance = info.FastDotChance;
        ObstacleChance = info.ObstacleChance;
        BombChance = info.BombChance;
        TimeDotChance = info.TimeDotChance;
        LevelTitle = info.Title;
        LevelSubtitle = info.Subtitle;
        PhaseName = info.PhaseName;
        ConnectionCooldown = 0.25f + Mathf.Min(0.35f, 0.04f * (ActiveLevel - 1));
    }

    public string GetLevelPreviewText(int nextLvl)
    {
        if (GameBrain.Instance != null)
        {
            var config = GameBrain.Instance.levelStream.Get(nextLvl);
            if (config != null) return config.previewText;
        }
        LevelInfo info = LevelData.GetLevel(nextLvl);
        return info.PreviewText;
    }

    public string GetLevelTitle(int lvl)
    {
        if (GameBrain.Instance != null)
        {
            var config = GameBrain.Instance.levelStream.Get(lvl);
            if (config != null) return config.theme;
        }
        return LevelData.GetLevelDisplayTitle(lvl);
    }

    public string GetLevelSubtitle(int lvl)
    {
        if (GameBrain.Instance != null)
        {
            var config = GameBrain.Instance.levelStream.Get(lvl);
            if (config != null) return config.isBossLevel ? "BOSS FIGHT" : "LEVEL " + config.id;
        }
        return LevelData.GetLevelDisplaySubtitle(lvl);
    }
}
