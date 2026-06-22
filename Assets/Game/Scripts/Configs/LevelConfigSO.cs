using System.Collections.Generic;
using UnityEngine;

public enum LevelObjectiveType
{
    Score,
    Destroy,
    Combo,
    Survival,
    Time,
    Color,
    SpecialBall
}

[CreateAssetMenu(fileName = "LevelConfigSO", menuName = "Balancing/LevelConfigSO")]
public class LevelConfigSO : ScriptableObject
{
    [Header("Level Identity")]
    public int id;
    public float difficultyRating = 1.0f;
    public string theme = "Neon Grid";
    public string biome = "Grid";
    public int seed;

    [Header("Gameplay Parameters")]
    public int targetScore;
    public float baseTimer = 90f;
    public int comboRequirement = 0;
    public LevelObjectiveType objectiveType = LevelObjectiveType.Score;
    public int destroyTargetCount = 0;
    public TopTipi targetColor = TopTipi.KirmiziTop;
    public TopTipi targetSpecialBall = TopTipi.Gokkusagi;

    [Header("Generation Settings")]
    public float spawnInterval = 0.8f;
    public float gravityScale = 0.15f;
    public float difficultyFactor = 1.0f;
    public bool isBossLevel;
    public BossConfig bossConfig;
    public List<BallConfig> allowedBalls = new List<BallConfig>();
    public List<ObstacleConfig> presetObstacles = new List<ObstacleConfig>();
    public List<EventConfig> potentialEvents = new List<EventConfig>();

    [TextArea(2, 5)]
    public string previewText;
}
