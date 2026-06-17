using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Config/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    public float BaseSpawnInterval = 0.8f;
    public float BaseGravityScale = 0.5f;
    public float SpeedFactorPerLevel = 0.05f;
    public float GravityIncreasePerLevel = 0.03f;
    public float MaxGravityScale = 1.8f;
    public float MinSpawnInterval = 0.35f;
}
