using UnityEngine;

public enum ChaosEventType
{
    None,
    GravityStorm,
    TimeCollapse,
    MirrorWorld,
    QuantumError,
    VoidInvasion,
    VirusStorm,
    RealityShift,
    ChaosStorm
}

[CreateAssetMenu(fileName = "EventConfig", menuName = "Balancing/EventConfig")]
public class EventConfig : ScriptableObject
{
    public string eventId;
    public string eventName;
    public ChaosEventType chaosType;
    public float baseIntensity = 1.0f;
    public float duration = 15.0f;
    public float difficultyRating = 1.0f;
    public string description;
}
