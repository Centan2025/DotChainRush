using UnityEngine;

public enum ObstacleType
{
    NormalBlock,
    MetalBlock,
    IceBlock,
    ShieldBlock,
    ChainBlock,
    MirrorBlock,
    VoidBlock,
    TimeBlock,
    VirusBlock,
    CorruptedBlock
}

public enum ObstacleElement
{
    None,
    Fire,
    Ice,
    Void,
    Nature,
    Metal,
    Light,
    Corrupted
}

public enum ObstacleMovement
{
    Static,
    Sliding,
    Rotating,
    LaserBarrier,
    TeleportGate,
    GravityWall
}

public enum ObstacleInteraction
{
    NormalCombo,
    HeavyOnly,
    IceMelting,
    ShieldBlocked,
    ChainLink,
    Reflection,
    VoidConsumption,
    TimeBonus,
    VirusSpread,
    CorruptedRuleChange
}

public enum ObstacleRewardType
{
    None,
    Points,
    Coins,
    ExtraTime,
    SpecialBall,
    Upgrades,
    Crystal
}

[CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Balancing/ObstacleConfig")]
public class ObstacleConfig : ScriptableObject
{
    public string obstacleId;
    public ObstacleType type;
    public int maxHealth = 1;
    public int armor = 0;
    public ObstacleElement element = ObstacleElement.None;
    public ObstacleMovement movement = ObstacleMovement.Static;
    public ObstacleInteraction interaction = ObstacleInteraction.NormalCombo;
    public ObstacleRewardType rewardType = ObstacleRewardType.None;
    public int rewardAmount = 10;
    public float movementSpeed = 1.0f;
    public Sprite visualSprite;
}
