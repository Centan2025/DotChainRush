using UnityEngine;

[CreateAssetMenu(fileName = "BallConfig", menuName = "Balancing/BallConfig")]
public class BallConfig : ScriptableObject
{
    public TopTipi type;
    public float baseSpawnWeight = 0.15f;
    public float currentSpawnModifier = 1.0f;
    public int unlockLevel = 1;
    public int unlockLevelOffset = 0;
    public int scoreMultiplier = 1;
    public float speedFactor = 1.0f;
    public float effectScale = 1.0f; // Nerf/Buff multiplier (e.g. 0.8f for nerf, 1.2f for buff)
}
