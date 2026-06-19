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
    public float SpecialDotChance { get; private set; } = 0f;
    public float FastDotChance { get; private set; } = 0f;
    public float ObstacleChance { get; private set; } = 0f;
    public float BombChance { get; private set; } = 0f;
    public float TimeDotChance { get; private set; } = 0f;

    // Current level metadata for HUD
    public string LevelTitle { get; private set; } = "First Spark";
    public string LevelSubtitle { get; private set; } = "TUTORIAL I";
    public string PhaseName { get; private set; } = "Öğretme";

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
        SetLevel(1);
    }

    public int GetGoalForLevel(int lvl)
    {
        LevelInfo info = LevelData.GetLevel(lvl);
        return info.TargetScore;
    }

    public void SetLevel(int lvl)
    {
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
    }

    public string GetLevelPreviewText(int nextLvl)
    {
        LevelInfo info = LevelData.GetLevel(nextLvl);
        return info.PreviewText;
    }

    public string GetLevelTitle(int lvl)
    {
        return LevelData.GetLevelDisplayTitle(lvl);
    }

    public string GetLevelSubtitle(int lvl)
    {
        return LevelData.GetLevelDisplaySubtitle(lvl);
    }
}
