using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig_Level10", menuName = "DotChainRush/Levels/Level10")]
public class LevelConfig_Level10 : ScriptableObject
{
    [Header("Level Identification")]
    public int levelId = 10;
    public string levelName = "CHAOS ORB DIMENSION";
    public string levelTheme = "First Reality Distortion Event";
    public string levelType = "BOSS_DIMENSION";
    
    [Header("Core Configs")]
    public float difficulty = 0.25f;
    public float baseTime = 90f;
    
    [Header("Score Goals")]
    public int baseScoreGoal = 1000;
    public int levelScoreMultiplier = 1500;
    public float bossMultiplier = 1.3f;
    public int finalTarget = 20800; // Expected output of (1000 + 10 * 1500) * 1.3

    [Header("Spawn Rates")]
    public float redChance = 0.20f;
    public float blueChance = 0.20f;
    public float greenChance = 0.20f;
    public float yellowChance = 0.15f;
    public float purpleChance = 0.15f;
    public float bombChance = 0.05f;
    public float heavyChance = 0.03f;
    public float rainbowChance = 0.02f;

    public int GetTargetScore()
    {
        return Mathf.RoundToInt((baseScoreGoal + (levelId * levelScoreMultiplier)) * bossMultiplier);
    }
}
