using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    public int id;
    public string theme;
    public float spawnRate; // spawn interval in seconds
    public float speed;     // gravity scale
    public int targetScore;
    public List<TopTipi> allowedBalls = new List<TopTipi>();
    public bool isBossLevel;
    public TopTipi bossType;
    public int bossHP;
    public string previewText;
}

public class LevelGenerator
{
    public List<LevelConfig> GenerateAllLevels()
    {
        var list = new List<LevelConfig>();
        for (int i = 1; i <= 200; i++)
        {
            list.Add(GenerateLevel(i));
        }
        return list;
    }

    public LevelConfig GenerateLevel(int i)
    {
        var lvl = new LevelConfig();
        lvl.id = i;
        lvl.isBossLevel = (i % 10 == 0);
        lvl.theme = GetThemeForLevel(i);

        // Core physics curves
        lvl.spawnRate = Mathf.Max(0.15f, 0.85f - (i * 0.0035f));
        lvl.speed = Mathf.Min(0.85f, 0.12f + (i * 0.0035f));
        lvl.targetScore = 1000 + i * 1500;

        // Construct pool of allowed balls
        lvl.allowedBalls = BuildPool(i);

        if (lvl.isBossLevel)
        {
            lvl.bossType = GetBossTypeForLevel(i);
            lvl.bossHP = 3 + (i / 10) * 2; // Hit-based health
            lvl.targetScore = Mathf.RoundToInt(lvl.targetScore * 1.3f);
            lvl.previewText = $"BOSS: {GetBossName(lvl.bossType)}! Dev küreyi yok etmek için comboları bağla!";
        }
        else
        {
            lvl.previewText = GetLevelDescription(i, lvl.allowedBalls);
        }

        return lvl;
    }

    private string GetThemeForLevel(int level)
    {
        if (level <= 10) return "Spark Neon";
        if (level <= 20) return "Elemental Fury";
        if (level <= 40) return "Mutant Strain";
        if (level <= 60) return "Entropy Drift";
        if (level <= 80) return "Chaos Core";
        if (level <= 100) return "Edge of Void";
        return "Ultimate Prestige";
    }

    private List<TopTipi> BuildPool(int level)
    {
        var pool = new List<TopTipi>
        {
            // Base colors always allowed
            TopTipi.KirmiziTop,
            TopTipi.MaviTop,
            TopTipi.YesilTop,
            TopTipi.SariTop,
            TopTipi.MorTop
        };

        // Scan BalanceDB for special balls that are unlocked at this level
        List<TopTipi> candidates = new List<TopTipi>();
        foreach (var pair in BalanceDB.unlockOffset)
        {
            TopTipi ball = pair.Key;
            int baseUnlock = pair.Value;

            if (BalanceDB.IsNormalColor(ball) || BalanceDB.IsBoss(ball))
                continue;

            // Unlock condition
            if (level >= baseUnlock)
            {
                candidates.Add(ball);
            }
        }

        // Shuffle candidates based on level seed to make pools varied per level
        UnityEngine.Random.State oldState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(level * 555);

        for (int i = 0; i < candidates.Count; i++)
        {
            int swapIdx = UnityEngine.Random.Range(i, candidates.Count);
            var temp = candidates[i];
            candidates[i] = candidates[swapIdx];
            candidates[swapIdx] = temp;
        }

        // Add up to 4-7 unlocked special ball types to the level pool
        int specialsToAdd = Mathf.Min(candidates.Count, 3 + (level / 20));
        for (int k = 0; k < specialsToAdd; k++)
        {
            pool.Add(candidates[k]);
        }

        UnityEngine.Random.state = oldState;
        return pool;
    }

    private TopTipi GetBossTypeForLevel(int level)
    {
        int bossIndex = (level / 10) % 7;
        switch (bossIndex)
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

    private string GetBossName(TopTipi type)
    {
        switch (type)
        {
            case TopTipi.KaosOrbBoss1: return "Kaos Küresi";
            case TopTipi.ElementalFuryBoss2: return "Elementlerin Öfkesi";
            case TopTipi.FlowMasterBoss4: return "Akış Ustası";
            case TopTipi.ZamanLorduBoss: return "Zaman Lordu";
            case TopTipi.ChaosIncarnateBoss7: return "Kaos Bedeni";
            case TopTipi.PrestigeBoss: return "Prestij Lordu";
            case TopTipi.TheVoidFinalBoss: return "BOŞLUK FİNAL BOSS";
            default: return "Gizemli Koruyucu";
        }
    }

    private string GetLevelDescription(int level, List<TopTipi> pool)
    {
        if (level == 1) return "Dokun → Bağla → Patlat! Aynı renkteki topları birbirine bağla.";
        if (level == 2) return "Gökkuşağı Joker toplar sahneye çıkıyor! Her renge bağlanabilirler.";
        if (level == 3) return "Hızlı Toplar geldi! Arkalarında neon iz bırakarak hızlıca düşüyorlar.";

        // Grab one special ball in pool to preview
        foreach (var b in pool)
        {
            if (!BalanceDB.IsNormalColor(b))
            {
                return $"Bu seviyede {b} topunun gücünü kullanabilirsin! Akıllıca combolarla birleştir.";
            }
        }

        return $"Seviye {level}: Neon reaksiyonları hızlanıyor. Hızlı ol!";
    }
}
