using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfigSO", menuName = "Balancing/LevelConfigSO")]
public class LevelConfigSO : ScriptableObject
{
    public int id;
    public string theme;
    public int targetScore;
    public float baseTimer = 90f;
    public float spawnInterval = 0.8f;
    public float gravityScale = 0.15f;
    public float difficultyFactor = 1.0f;
    public bool isBossLevel;
    public BossConfig bossConfig;
    public List<BallConfig> allowedBalls = new List<BallConfig>();
    [TextArea(2, 5)]
    public string previewText;
}
