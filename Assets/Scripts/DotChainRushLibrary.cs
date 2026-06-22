using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TopTipi
{
    Kozmik, KirmiziTop, MaviTop, YesilTop, SariTop, MorTop, Gokkusagi, HizliTop, AgirTop, Bomba, Buz, Yercekimi,
    Kilit, Anahtar, Zaman, KaosOrbBoss1, ElementalFuryBoss2, Ates, BuzElement, Doga, Bosluk, Altin2x, Elektrik, Su, Toprak,
    Isik, Ayna, KaraDelik, SahteTop, CiftYonlu, Yapiskan, Teleport, FlowMasterBoss4, Mutasyon, ZincirUstasi, PatlamaCekirdegi, GokkusagiKani, ZamanBukucu,
    Kalkan, Skor2x, Can, Gorunmez, Kuantum, Glitch, ZamanLorduBoss, TersYercekimi, PatlayiciYagmur, ChaosIncarnateBoss7, PrestigeBoss, Virus, GravityCore,
    OneMistake, HizCekirdegi, Kritik, OlumTopu, GerceklikKirici, KorruptBomba, VoidRainbow, ElitAgir, SonsuzZaman, KaosCekirdegi, Omega, TheVoidFinalBoss, ComboTopu,
    Magnet, Jackpot, Ruzgar, DonmaAlani, Lazer, Enerji, Cogalan, SiyahIsik, Yildiz, Nukleer, BoyutKapisi, Hyper, Sinirsiz,
    Plazma, KaranlikMadde, Rezonans, SolucanDeligi, Nebula, YildizTozu, Sonsuzluk, Kutsal
}

[System.Serializable]
public struct TopGorselEslestirme
{
    public TopTipi topTipi;
    public Sprite topSprite;
}

public class DotChainRushLibrary : MonoBehaviour
{
    public static DotChainRushLibrary Instance { get; private set; }

    [Header("Ana Sprite Sheet Görselini Buraya Atın")]
    public Texture2D anaGorsel;

    [Header("Kütüphane Listesi")]
    public TopGorselEslestirme[] tumToplar;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Butona gerek kalmadan, Inspector'da sağ üstteki üç noktaya basıp "Gorselleri Eslestir" diyebilirsin!
    [ContextMenu("Görselleri Eşleştir")]
    public void OtomatikEslestir()
    {
#if UNITY_EDITOR
        if (anaGorsel == null) return;

        string dosyaYolu = AssetDatabase.GetAssetPath(anaGorsel);
        object[] altOgeler = AssetDatabase.LoadAllAssetsAtPath(dosyaYolu);
        int enumUzunlugu = Enum.GetValues(typeof(TopTipi)).Length;
        tumToplar = new TopGorselEslestirme[enumUzunlugu];

        // Initialize array first
        for (int i = 0; i < enumUzunlugu; i++)
        {
            tumToplar[i] = new TopGorselEslestirme { topTipi = (TopTipi)i, topSprite = null };
        }

        foreach (object oge in altOgeler)
        {
            if (oge is Sprite sprite)
            {
                // Sprite isimlendirmesinden sonundaki indeksi oku (Örn: "anaGorsel_5" veya "sprite_5")
                string name = sprite.name;
                int underscoreIndex = name.LastIndexOf('_');
                if (underscoreIndex != -1 && underscoreIndex < name.Length - 1)
                {
                    string indexStr = name.Substring(underscoreIndex + 1);
                    if (int.TryParse(indexStr, out int spriteIndex))
                    {
                        if (spriteIndex >= 0 && spriteIndex < enumUzunlugu)
                        {
                            tumToplar[spriteIndex] = new TopGorselEslestirme { topTipi = (TopTipi)spriteIndex, topSprite = sprite };
                            Debug.Log($"Mapped Sprite '{name}' to TopTipi {(TopTipi)spriteIndex} ({spriteIndex})");
                        }
                    }
                }
            }
        }
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log("Toplar başarıyla isim indeksine göre eşleştirildi!");
#endif
    }

    public Sprite GetTopSprite(TopTipi arananTop)
    {
        if (tumToplar == null) return null;
        foreach (var top in tumToplar)
        {
            if (top.topTipi == arananTop) return top.topSprite;
        }
        return null;
    }
}