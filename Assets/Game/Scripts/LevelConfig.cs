using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Config/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public int LevelIndex;
    public int TargetScore;
    [TextArea(2, 4)]
    public string UnlockMessage; // Teaser for next level mechanics
    public float SpawnInterval = 0.8f;
    public float ObstacleChance = 0.05f;
    public float FastDotChance = 0.05f;
    public float SpecialDotChance = 0.05f;
}
