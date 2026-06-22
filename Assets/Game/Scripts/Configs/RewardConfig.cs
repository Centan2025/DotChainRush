using UnityEngine;

public enum RewardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "RewardConfig", menuName = "Balancing/RewardConfig")]
public class RewardConfig : ScriptableObject
{
    public string rewardId;
    public ObstacleRewardType type = ObstacleRewardType.Points;
    public RewardRarity rarity = RewardRarity.Common;
    public float dropChance = 0.5f;
    public int baseAmount = 10;
    public TopTipi rewardBallType = TopTipi.Altin2x;
}
