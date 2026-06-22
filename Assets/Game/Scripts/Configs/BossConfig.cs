using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossConfig", menuName = "Balancing/BossConfig")]
public class BossConfig : ScriptableObject
{
    public TopTipi bossType;
    public int maxHP = 5;
    public float difficultyMultiplier = 1.0f;
    public List<BossTrait> traits = new List<BossTrait>();
}
