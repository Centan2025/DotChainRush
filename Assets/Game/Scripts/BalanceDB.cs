using System;
using System.Collections.Generic;
using UnityEngine;

public static class BalanceDB
{
    public static Dictionary<TopTipi, float> spawn = new Dictionary<TopTipi, float>();
    public static Dictionary<TopTipi, int> unlockOffset = new Dictionary<TopTipi, int>();

    static BalanceDB()
    {
        InitializeDefaults();
    }

    public static void InitializeDefaults()
    {
        spawn.Clear();
        unlockOffset.Clear();

        foreach (TopTipi type in Enum.GetValues(typeof(TopTipi)))
        {
            // Default spawn weights
            if (IsNormalColor(type))
            {
                spawn[type] = 1.0f;
                unlockOffset[type] = 0; // Unlocked from level 1
            }
            else if (IsBoss(type))
            {
                spawn[type] = 0.0f; // Bosses are spawned by events, not randomly
                unlockOffset[type] = 10; 
            }
            else
            {
                // Default special ball values
                spawn[type] = 0.15f;
                unlockOffset[type] = GetDefaultUnlockLevel(type);
            }
        }
    }

    public static bool IsNormalColor(TopTipi type)
    {
        return type == TopTipi.KirmiziTop || type == TopTipi.MaviTop || 
               type == TopTipi.YesilTop || type == TopTipi.SariTop || 
               type == TopTipi.MorTop;
    }

    public static bool IsBoss(TopTipi type)
    {
        return type == TopTipi.ElementalFuryBoss2 || type == TopTipi.KaosOrbBoss1 || 
               type == TopTipi.FlowMasterBoss4 || type == TopTipi.ZamanLorduBoss || 
               type == TopTipi.ChaosIncarnateBoss7 || type == TopTipi.PrestigeBoss || 
               type == TopTipi.TheVoidFinalBoss;
    }

    private static int GetDefaultUnlockLevel(TopTipi type)
    {
        switch (type)
        {
            case TopTipi.Gokkusagi: return 2;
            case TopTipi.HizliTop: return 3;
            case TopTipi.Bomba: return 6;
            case TopTipi.Buz: return 7;
            case TopTipi.Yercekimi: return 8;
            case TopTipi.Kilit:
            case TopTipi.Anahtar: return 9;
            case TopTipi.Zaman: return 11;
            case TopTipi.Ates:
            case TopTipi.BuzElement: return 12;
            case TopTipi.Doga: return 13;
            case TopTipi.Bosluk: return 14;
            case TopTipi.Altin2x: return 15;
            case TopTipi.Elektrik: return 21;
            case TopTipi.Su: return 22;
            case TopTipi.Toprak: return 23;
            case TopTipi.Isik: return 24;
            case TopTipi.Ayna: return 26;
            case TopTipi.KaraDelik: return 27;
            case TopTipi.SahteTop: return 28;
            case TopTipi.CiftYonlu: return 29;
            case TopTipi.Yapiskan: return 31;
            case TopTipi.Teleport: return 32;
            case TopTipi.Mutasyon: return 33;
            case TopTipi.ZincirUstasi: return 34;
            case TopTipi.PatlamaCekirdegi: return 35;
            case TopTipi.GokkusagiKani: return 36;
            case TopTipi.ZamanBukucu: return 37;
            case TopTipi.Kalkan: return 38;
            case TopTipi.Skor2x: return 39;
            case TopTipi.Can: return 41;
            case TopTipi.Gorunmez: return 51;
            case TopTipi.Kuantum: return 52;
            case TopTipi.Glitch: return 53;
            case TopTipi.TersYercekimi: return 55;
            case TopTipi.PatlayiciYagmur: return 56;
            case TopTipi.Virus: return 57;
            case TopTipi.GravityCore: return 58;
            case TopTipi.OneMistake: return 90;
            case TopTipi.HizCekirdegi: return 61;
            case TopTipi.Kritik: return 62;
            case TopTipi.OlumTopu: return 63;
            case TopTipi.GerceklikKirici: return 64;
            case TopTipi.KorruptBomba: return 65;
            case TopTipi.VoidRainbow: return 66;
            case TopTipi.ElitAgir: return 67;
            case TopTipi.SonsuzZaman: return 68;
            case TopTipi.KaosCekirdegi: return 69;
            case TopTipi.Omega: return 70;
            case TopTipi.ComboTopu: return 71;
            case TopTipi.Magnet: return 72;
            case TopTipi.Jackpot: return 73;
            case TopTipi.Ruzgar: return 74;
            case TopTipi.DonmaAlani: return 75;
            case TopTipi.Lazer: return 76;
            case TopTipi.Enerji: return 77;
            case TopTipi.Cogalan: return 78;
            case TopTipi.SiyahIsik: return 79;
            case TopTipi.Yildiz: return 80;
            case TopTipi.Nukleer: return 81;
            case TopTipi.BoyutKapisi: return 82;
            case TopTipi.Hyper: return 83;
            case TopTipi.Sinirsiz: return 84;
            case TopTipi.Plazma: return 85;
            case TopTipi.KaranlikMadde: return 86;
            case TopTipi.Rezonans: return 87;
            case TopTipi.SolucanDeligi: return 88;
            case TopTipi.Nebula: return 89;
            case TopTipi.YildizTozu: return 91;
            case TopTipi.Sonsuzluk: return 99;
            case TopTipi.Kutsal: return 100;
            default: return 5;
        }
    }
}
