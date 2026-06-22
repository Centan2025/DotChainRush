using System.Collections.Generic;
using UnityEngine;

public class ProceduralBossAI
{
    public BossConfig GenerateBoss(int levelNumber, int seed)
    {
        BossConfig boss = ScriptableObject.CreateInstance<BossConfig>();
        boss.bossType = GetBossType(levelNumber);
        boss.maxHP = 3 + (levelNumber / 10) * 2;
        boss.difficultyMultiplier = 1.0f + (levelNumber * 0.005f);

        // Assign 2 to 4 unique procedural traits
        Random.State oldState = Random.state;
        Random.InitState(seed);

        int traitCount = Random.Range(2, 5);
        for (int i = 0; i < traitCount; i++)
        {
            BossTrait trait = (BossTrait)Random.Range(0, 6);
            if (!boss.traits.Contains(trait))
            {
                boss.traits.Add(trait);
            }
        }

        Random.state = oldState;

        // Validation check via simulation: If traits are too penalizing, reduce HP
        if (boss.traits.Contains(BossTrait.TimeSqueezer) && boss.traits.Contains(BossTrait.Shielded))
        {
            boss.maxHP = Mathf.Max(3, boss.maxHP - 2); // Eased HP to keep it fair
        }

        return boss;
    }

    private TopTipi GetBossType(int level)
    {
        switch (level)
        {
            case 10: return TopTipi.KaosOrbBoss1;
            case 30: return TopTipi.ElementalFuryBoss2;
            case 50: return TopTipi.FlowMasterBoss4;
            case 70: return TopTipi.ZamanLorduBoss;
            case 100: return TopTipi.ChaosIncarnateBoss7;
            case 120: return TopTipi.Glitch;
            case 140: return TopTipi.GravityCore;
            case 160: return TopTipi.Virus;
            case 180: return TopTipi.Omega;
            case 200: return TopTipi.TheVoidFinalBoss;
            default:
                int idx = (level / 10) % 10;
                switch (idx)
                {
                    case 1: return TopTipi.KaosOrbBoss1;
                    case 3: return TopTipi.ElementalFuryBoss2;
                    case 5: return TopTipi.FlowMasterBoss4;
                    case 7: return TopTipi.ZamanLorduBoss;
                    case 0: return TopTipi.ChaosIncarnateBoss7;
                    case 2: return TopTipi.Glitch;
                    case 4: return TopTipi.GravityCore;
                    case 6: return TopTipi.Virus;
                    case 8: return TopTipi.Omega;
                    default: return TopTipi.TheVoidFinalBoss;
                }
        }
    }
}
