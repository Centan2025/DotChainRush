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
        int bossIdx = (level / 10) % 7;
        switch (bossIdx)
        {
            case 1: return TopTipi.KaosOrbBoss1;
            case 2: return TopTipi.ElementalFuryBoss2;
            case 3: return TopTipi.FlowMasterBoss4;
            case 4: return TopTipi.ZamanLorduBoss;
            case 5: return TopTipi.ChaosIncarnateBoss7;
            case 6: return TopTipi.PrestigeBoss;
            default: return TopTipi.TheVoidFinalBoss;
        }
    }
}
