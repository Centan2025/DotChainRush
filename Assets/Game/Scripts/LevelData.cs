using UnityEngine;

public struct LevelInfo
{
    public int Level;
    public string Title;
    public string Subtitle;
    public string PhaseName;
    public int TargetScore;
    public float SpawnInterval;
    public float GravityScale;
    public float SpecialDotChance;
    public float FastDotChance;
    public float ObstacleChance;
    public float BombChance;
    public float TimeDotChance;
    public string PreviewText;
}

public static class LevelData
{
    public static LevelInfo GetLevel(int lvl)
    {
        if (lvl < 1) lvl = 1;

        // Level 100+ infinite scaling
        if (lvl > 100)
        {
            return GetInfiniteLevel(lvl);
        }

        return lvl switch
        {
            // ════════════════════════════════════════════
            // PHASE 1: TUTORIAL (Level 1-5)
            // ════════════════════════════════════════════
            1 => Make(1, "First Spark", "TUTORIAL I", "Öğretme", 1000,
                spawnInterval: 0.85f, gravity: 0.12f,
                preview: "Dokun → Bağla → Patlat! Aynı renkteki topları birbirine bağlayarak puan topla."),

            2 => Make(2, "Color Flow", "TUTORIAL II", "Öğretme", 2500,
                spawnInterval: 0.78f, gravity: 0.15f, special: 0.05f,
                preview: "Gökkuşağı Joker toplar sahneye çıkıyor! Her renge bağlanabilirler."),

            3 => Make(3, "Fast Pulse", "TUTORIAL III", "Öğretme", 4000,
                spawnInterval: 0.72f, gravity: 0.18f, special: 0.05f, fast: 0.10f,
                preview: "Hızlı Toplar geldi! Arkalarında neon iz bırakarak düşüyorlar."),

            4 => Make(4, "Pressure Rising", "TUTORIAL IV", "Öğretme", 6000,
                spawnInterval: 0.65f, gravity: 0.22f, special: 0.06f, fast: 0.12f,
                preview: "Tehlike Bölgesi açılıyor! Toplar üst sınıra ulaşırsa oyun biter."),

            5 => Make(5, "First Storm", "TUTORIAL V", "Öğretme", 8000,
                spawnInterval: 0.58f, gravity: 0.25f, special: 0.07f, fast: 0.15f,
                preview: "İlk büyük sınav! 20+ combo yaparsan FEVER MODE açılır! x10 puan!"),

            // ════════════════════════════════════════════
            // PHASE 2: NEW MECHANICS (Level 6-10)
            // ════════════════════════════════════════════
            6 => Make(6, "Bomb Dot", "MECHANICS I", "Yeni Mekanikler", 10000,
                spawnInterval: 0.55f, gravity: 0.27f, special: 0.07f, fast: 0.15f, bomb: 0.03f,
                preview: "Bomba Toplar sahneye giriyor! Patladığında etrafındaki topları da yok eder!"),

            7 => Make(7, "Frozen Dot", "MECHANICS II", "Yeni Mekanikler", 12000,
                spawnInterval: 0.52f, gravity: 0.28f, special: 0.07f, fast: 0.15f, obstacle: 0.08f, bomb: 0.03f,
                preview: "Buz Toplar zincirlenemez! Yanındaki aynı renk toplar patlarsa çözülür."),

            8 => Make(8, "Gravity Shift", "MECHANICS III", "Yeni Mekanikler", 14000,
                spawnInterval: 0.50f, gravity: 0.30f, special: 0.08f, fast: 0.18f, obstacle: 0.10f, bomb: 0.03f,
                preview: "Yerçekimi kayması! Her 20 saniyede yerçekimi yönü değişiyor."),

            9 => Make(9, "Color Lock", "MECHANICS IV", "Yeni Mekanikler", 16000,
                spawnInterval: 0.48f, gravity: 0.32f, special: 0.08f, fast: 0.18f, obstacle: 0.10f, bomb: 0.04f,
                preview: "Renk kilidi aktif! Bir renk kilitlenir, önce anahtar topu bul!"),

            10 => Make(10, "Chaos Orb", "BOSS DIMENSION", "First Reality Distortion Event", 20800,
                spawnInterval: 0.45f, gravity: 0.35f, special: 0.10f, fast: 0.20f, obstacle: 0.12f, bomb: 0.04f,
                preview: "BOSS: Kaos Küresi! Oyun sana karşı savaşıyor. Kaos stabilitesini sıfıra indir ve 20800 puana ulaş!"),

            // ════════════════════════════════════════════
            // PHASE 3: ELEMENTS (Level 11-20)
            // ════════════════════════════════════════════
            11 => Make(11, "Fire Burst", "ELEMENT I", "Element Sistemi", 22000,
                spawnInterval: 0.44f, gravity: 0.35f, special: 0.10f, fast: 0.20f, obstacle: 0.12f, bomb: 0.04f, time: 0.03f,
                preview: "Ateş Dotları! Patladığında alan açar ve etrafındakileri yakar!"),

            12 => Make(12, "Ice Flow", "ELEMENT II", "Element Sistemi", 24000,
                spawnInterval: 0.43f, gravity: 0.36f, special: 0.10f, fast: 0.20f, obstacle: 0.13f, bomb: 0.04f, time: 0.03f,
                preview: "Buz elementi aktif! Buz dotları yakınındaki topları yavaşlatır."),

            13 => Make(13, "Nature Bloom", "ELEMENT III", "Element Sistemi", 26000,
                spawnInterval: 0.42f, gravity: 0.37f, special: 0.11f, fast: 0.20f, obstacle: 0.13f, bomb: 0.04f, time: 0.04f,
                preview: "Doğa dotları kendini çoğaltır! Patlatırsan ekstra toplar doğar."),

            14 => Make(14, "Void Pulse", "ELEMENT IV", "Element Sistemi", 28000,
                spawnInterval: 0.41f, gravity: 0.38f, special: 0.11f, fast: 0.21f, obstacle: 0.14f, bomb: 0.05f, time: 0.04f,
                preview: "Boşluk dotları yakınındaki topları yok eder! Dikkatli kullan."),

            15 => Make(15, "Gold Rush", "ELEMENT V", "Element Sistemi", 30000,
                spawnInterval: 0.40f, gravity: 0.38f, special: 0.12f, fast: 0.21f, obstacle: 0.14f, bomb: 0.05f, time: 0.05f,
                preview: "Altın dotlar 2x puan verir! Combo ile birleştir, rekor kır!"),

            16 => Make(16, "Elemental Mix", "ELEMENT VI", "Element Sistemi", 32000,
                spawnInterval: 0.40f, gravity: 0.39f, special: 0.12f, fast: 0.22f, obstacle: 0.15f, bomb: 0.05f, time: 0.05f,
                preview: "Tüm elementler karışıyor! Strateji önemli."),

            17 => Make(17, "Rising Heat", "ELEMENT VII", "Element Sistemi", 34000,
                spawnInterval: 0.39f, gravity: 0.40f, special: 0.12f, fast: 0.23f, obstacle: 0.15f, bomb: 0.05f, time: 0.05f,
                preview: "Sıcaklık yükseliyor! Toplar daha hızlı düşüyor, hız arttı."),

            18 => Make(18, "Deep Freeze", "ELEMENT VIII", "Element Sistemi", 36000,
                spawnInterval: 0.38f, gravity: 0.40f, special: 0.13f, fast: 0.23f, obstacle: 0.16f, bomb: 0.05f, time: 0.05f,
                preview: "Derin don! Engel topların sıklığı artıyor, alan daralıyor."),

            19 => Make(19, "Storm Caller", "ELEMENT IX", "Element Sistemi", 38000,
                spawnInterval: 0.37f, gravity: 0.41f, special: 0.13f, fast: 0.24f, obstacle: 0.16f, bomb: 0.06f, time: 0.05f,
                preview: "Fırtına çağırıcı! Tüm elementler tam güçte. Hazır ol!"),

            20 => Make(20, "Elemental Fury", "BOSS II", "Element Sistemi", 42000,
                spawnInterval: 0.36f, gravity: 0.42f, special: 0.14f, fast: 0.25f, obstacle: 0.17f, bomb: 0.06f, time: 0.06f,
                preview: "BOSS: Elemental Öfke! Tüm element güçleri bir arada. Hayatta kal!"),

            // ════════════════════════════════════════════
            // PHASE 4: FLOW BREAKERS (Level 21-30)
            // ════════════════════════════════════════════
            21 => Make(21, "Mirror Rain", "FLOW I", "Akış Bozucular", 44000,
                spawnInterval: 0.36f, gravity: 0.42f, special: 0.14f, fast: 0.25f, obstacle: 0.17f, bomb: 0.06f, time: 0.06f,
                preview: "Ayna Yağmuru! Toplar artık iki taraftan düşüyor."),

            22 => Make(22, "Black Hole", "FLOW II", "Akış Bozucular", 46000,
                spawnInterval: 0.35f, gravity: 0.43f, special: 0.14f, fast: 0.25f, obstacle: 0.17f, bomb: 0.06f, time: 0.06f,
                preview: "Kara Delik! Ekranın merkezinde çekim alanı oluşuyor."),

            23 => Make(23, "Fake Dot", "FLOW III", "Akış Bozucular", 48000,
                spawnInterval: 0.34f, gravity: 0.43f, special: 0.15f, fast: 0.25f, obstacle: 0.18f, bomb: 0.06f, time: 0.06f,
                preview: "Sahte Dotlar! Yanlış renk gösteriyorlar, dikkatli ol!"),

            24 => Make(24, "Chain Storm", "FLOW IV", "Akış Bozucular", 50000,
                spawnInterval: 0.34f, gravity: 0.44f, special: 0.15f, fast: 0.26f, obstacle: 0.18f, bomb: 0.07f, time: 0.06f,
                preview: "Zincir Fırtınası! Ekran sürekli hareket ediyor."),

            25 => Make(25, "The Collector", "BOSS III", "Akış Bozucular", 55000,
                spawnInterval: 0.33f, gravity: 0.45f, special: 0.15f, fast: 0.26f, obstacle: 0.18f, bomb: 0.07f, time: 0.06f,
                preview: "BOSS: Koleksiyoncu! Dotları emer ve güç kazanır. Hızlı ol!"),

            26 => Make(26, "Twist & Turn", "FLOW V", "Akış Bozucular", 52000,
                spawnInterval: 0.33f, gravity: 0.45f, special: 0.15f, fast: 0.26f, obstacle: 0.18f, bomb: 0.07f, time: 0.06f,
                preview: "Bükül ve dön! Toplar artık beklenmedik yönlere gidiyor."),

            27 => Make(27, "Neon Chaos", "FLOW VI", "Akış Bozucular", 54000,
                spawnInterval: 0.32f, gravity: 0.46f, special: 0.16f, fast: 0.26f, obstacle: 0.19f, bomb: 0.07f, time: 0.06f,
                preview: "Neon Kaos! Renkler daha canlı, hız daha yüksek."),

            28 => Make(28, "Pressure Cooker", "FLOW VII", "Akış Bozucular", 56000,
                spawnInterval: 0.32f, gravity: 0.46f, special: 0.16f, fast: 0.27f, obstacle: 0.19f, bomb: 0.07f, time: 0.07f,
                preview: "Basınç artıyor! Spawn hızı ve yerçekimi ciddi şekilde arttı."),

            29 => Make(29, "Razor Edge", "FLOW VIII", "Akış Bozucular", 58000,
                spawnInterval: 0.31f, gravity: 0.47f, special: 0.16f, fast: 0.27f, obstacle: 0.19f, bomb: 0.08f, time: 0.07f,
                preview: "Jilet kenarı! Hata payı minimum. Her hamle önemli."),

            30 => Make(30, "Flow Master", "BOSS IV", "Akış Bozucular", 62000,
                spawnInterval: 0.30f, gravity: 0.48f, special: 0.17f, fast: 0.28f, obstacle: 0.20f, bomb: 0.08f, time: 0.07f,
                preview: "BOSS: Akış Ustası! Tüm akış bozucu mekanikler bir arada!"),

            // ════════════════════════════════════════════
            // PHASE 5: MUTATION / ROGUELIKE (Level 31-50)
            // ════════════════════════════════════════════
            31 => Make(31, "First Mutation", "MUTANT I", "Mutasyon", 58000,
                spawnInterval: 0.30f, gravity: 0.48f, special: 0.17f, fast: 0.28f, obstacle: 0.20f, bomb: 0.08f, time: 0.07f,
                preview: "Mutasyon başlıyor! Her level sonunda 3 buff'tan birini seç!"),

            32 => Make(32, "Gene Splice", "MUTANT II", "Mutasyon", 60000,
                spawnInterval: 0.30f, gravity: 0.48f, special: 0.17f, fast: 0.28f, obstacle: 0.20f, bomb: 0.08f, time: 0.07f,
                preview: "Gen birleştirme! Buff'ların gücü artıyor."),

            33 => Make(33, "Chain Boost", "MUTANT III", "Mutasyon", 62000,
                spawnInterval: 0.29f, gravity: 0.49f, special: 0.17f, fast: 0.28f, obstacle: 0.20f, bomb: 0.08f, time: 0.07f,
                preview: "Zincir güçlendirmesi! +30% chain score seçeneği!"),

            34 => Make(34, "Time Warp", "MUTANT IV", "Mutasyon", 64000,
                spawnInterval: 0.29f, gravity: 0.49f, special: 0.18f, fast: 0.29f, obstacle: 0.20f, bomb: 0.08f, time: 0.07f,
                preview: "Zaman bükümü! +1 saniye freeze buff'ı mevcut."),

            35 => Make(35, "Rainbow Storm", "MUTANT V", "Mutasyon", 66000,
                spawnInterval: 0.29f, gravity: 0.50f, special: 0.18f, fast: 0.29f, obstacle: 0.21f, bomb: 0.09f, time: 0.08f,
                preview: "Gökkuşağı fırtınası! Rainbow şansı +10% buff seçeneği!"),

            36 => Make(36, "Power Surge", "MUTANT VI", "Mutasyon", 68000,
                spawnInterval: 0.28f, gravity: 0.50f, special: 0.18f, fast: 0.29f, obstacle: 0.21f, bomb: 0.09f, time: 0.08f,
                preview: "Güç dalgalanması! Buff'lar güçleniyor, zorluk artıyor."),

            37 => Make(37, "Adapt or Die", "MUTANT VII", "Mutasyon", 70000,
                spawnInterval: 0.28f, gravity: 0.51f, special: 0.19f, fast: 0.29f, obstacle: 0.21f, bomb: 0.09f, time: 0.08f,
                preview: "Uyum sağla ya da yok ol! Buff seçimlerin hayati önemde."),

            38 => Make(38, "Catalyst", "MUTANT VIII", "Mutasyon", 72000,
                spawnInterval: 0.28f, gravity: 0.51f, special: 0.19f, fast: 0.30f, obstacle: 0.21f, bomb: 0.09f, time: 0.08f,
                preview: "Katalizör! Reaksiyonlar hızlanıyor, puan fırsatları artıyor."),

            39 => Make(39, "Overload", "MUTANT IX", "Mutasyon", 74000,
                spawnInterval: 0.27f, gravity: 0.52f, special: 0.19f, fast: 0.30f, obstacle: 0.22f, bomb: 0.09f, time: 0.08f,
                preview: "Aşırı yükleme! Her şey hız kazanıyor."),

            40 => Make(40, "Mutation Lord", "BOSS V", "Mutasyon", 78000,
                spawnInterval: 0.27f, gravity: 0.52f, special: 0.20f, fast: 0.30f, obstacle: 0.22f, bomb: 0.10f, time: 0.08f,
                preview: "BOSS: Mutasyon Lordu! Tüm mutasyonlar aktif!"),

            41 => Make(41, "Double Down", "MUTANT X", "Mutasyon", 76000,
                spawnInterval: 0.27f, gravity: 0.52f, special: 0.20f, fast: 0.30f, obstacle: 0.22f, bomb: 0.10f, time: 0.08f,
                preview: "İkiye katla! Riskler ve ödüller büyüyor."),

            42 => Make(42, "Synergy", "MUTANT XI", "Mutasyon", 78000,
                spawnInterval: 0.27f, gravity: 0.53f, special: 0.20f, fast: 0.30f, obstacle: 0.22f, bomb: 0.10f, time: 0.08f,
                preview: "Sinerji! Buff kombinasyonları ekstra güç veriyor."),

            43 => Make(43, "Wildcard", "MUTANT XII", "Mutasyon", 80000,
                spawnInterval: 0.26f, gravity: 0.53f, special: 0.20f, fast: 0.31f, obstacle: 0.22f, bomb: 0.10f, time: 0.09f,
                preview: "Joker! Beklenmedik buff kombinasyonları ortaya çıkıyor."),

            44 => Make(44, "Evolution", "MUTANT XIII", "Mutasyon", 82000,
                spawnInterval: 0.26f, gravity: 0.54f, special: 0.21f, fast: 0.31f, obstacle: 0.22f, bomb: 0.10f, time: 0.09f,
                preview: "Evrim! Buff'ların evrimleşmiş halleri açılıyor."),

            45 => Make(45, "Apex Predator", "MUTANT XIV", "Mutasyon", 84000,
                spawnInterval: 0.26f, gravity: 0.54f, special: 0.21f, fast: 0.31f, obstacle: 0.23f, bomb: 0.10f, time: 0.09f,
                preview: "Apex yırtıcı! En güçlü buff setlerini topla."),

            46 => Make(46, "Unstable Core", "MUTANT XV", "Mutasyon", 86000,
                spawnInterval: 0.26f, gravity: 0.55f, special: 0.21f, fast: 0.31f, obstacle: 0.23f, bomb: 0.11f, time: 0.09f,
                preview: "Kararsız çekirdek! Bombalar daha sık, tehlike büyüyor."),

            47 => Make(47, "Critical Mass", "MUTANT XVI", "Mutasyon", 88000,
                spawnInterval: 0.25f, gravity: 0.55f, special: 0.22f, fast: 0.32f, obstacle: 0.23f, bomb: 0.11f, time: 0.09f,
                preview: "Kritik kütle! Her şey bir patlama noktasında."),

            48 => Make(48, "Rogue Wave", "MUTANT XVII", "Mutasyon", 90000,
                spawnInterval: 0.25f, gravity: 0.56f, special: 0.22f, fast: 0.32f, obstacle: 0.23f, bomb: 0.11f, time: 0.09f,
                preview: "Haydut dalgası! Büyük fırsatlar, büyük riskler."),

            49 => Make(49, "Singularity", "MUTANT XVIII", "Mutasyon", 92000,
                spawnInterval: 0.25f, gravity: 0.56f, special: 0.22f, fast: 0.32f, obstacle: 0.24f, bomb: 0.11f, time: 0.10f,
                preview: "Tekillik! Tüm kurallar bükülüyor."),

            50 => Make(50, "Mutation Master", "BOSS VI", "Mutasyon", 98000,
                spawnInterval: 0.24f, gravity: 0.57f, special: 0.23f, fast: 0.33f, obstacle: 0.24f, bomb: 0.12f, time: 0.10f,
                preview: "BOSS: Mutasyon Ustası! Maximum mutasyon gücü. Hayatta kalabilecek misin?"),

            // ════════════════════════════════════════════
            // PHASE 6: CHAOS ERA (Level 51-75)
            // ════════════════════════════════════════════
            51 => Make(51, "Invisible Dot", "CHAOS I", "Kaos Dönemi", 95000,
                spawnInterval: 0.24f, gravity: 0.57f, special: 0.23f, fast: 0.33f, obstacle: 0.24f, bomb: 0.12f, time: 0.10f,
                preview: "Görünmez Dotlar! Sadece yaklaştığında ortaya çıkıyorlar."),

            52 => Make(52, "Static Shock", "CHAOS II", "Kaos Dönemi", 97000,
                spawnInterval: 0.24f, gravity: 0.57f, special: 0.23f, fast: 0.33f, obstacle: 0.24f, bomb: 0.12f, time: 0.10f,
                preview: "Statik şok! Dotlar elektrik saçıyor, dikkat!"),

            53 => Make(53, "Color Blind", "CHAOS III", "Kaos Dönemi", 99000,
                spawnInterval: 0.24f, gravity: 0.58f, special: 0.23f, fast: 0.33f, obstacle: 0.24f, bomb: 0.12f, time: 0.10f,
                preview: "Renk körlüğü! Renkler geçici olarak karışıyor."),

            54 => Make(54, "Speed Demon", "CHAOS IV", "Kaos Dönemi", 101000,
                spawnInterval: 0.23f, gravity: 0.58f, special: 0.24f, fast: 0.34f, obstacle: 0.25f, bomb: 0.12f, time: 0.10f,
                preview: "Hız şeytanı! Her şey çok daha hızlı."),

            55 => Make(55, "Gravity Wave", "CHAOS V", "Kaos Dönemi", 103000,
                spawnInterval: 0.23f, gravity: 0.59f, special: 0.24f, fast: 0.34f, obstacle: 0.25f, bomb: 0.12f, time: 0.10f,
                preview: "Yerçekimi dalgası! Her 10 saniyede tüm dotlar yukarı sıçrar."),

            56 => Make(56, "Phantom Chain", "CHAOS VI", "Kaos Dönemi", 105000,
                spawnInterval: 0.23f, gravity: 0.59f, special: 0.24f, fast: 0.34f, obstacle: 0.25f, bomb: 0.13f, time: 0.10f,
                preview: "Hayalet zincir! Zincirlerin geçici olarak kopabiliyor."),

            57 => Make(57, "Wormhole", "CHAOS VII", "Kaos Dönemi", 107000,
                spawnInterval: 0.23f, gravity: 0.60f, special: 0.24f, fast: 0.34f, obstacle: 0.25f, bomb: 0.13f, time: 0.10f,
                preview: "Solucan deliği! Dotlar ekranın bir ucundan diğerine geçiyor."),

            58 => Make(58, "Pulse Storm", "CHAOS VIII", "Kaos Dönemi", 109000,
                spawnInterval: 0.22f, gravity: 0.60f, special: 0.25f, fast: 0.35f, obstacle: 0.25f, bomb: 0.13f, time: 0.11f,
                preview: "Darbe fırtınası! Patlama dalgaları sahneyi sarsıyor."),

            59 => Make(59, "Entropy", "CHAOS IX", "Kaos Dönemi", 111000,
                spawnInterval: 0.22f, gravity: 0.61f, special: 0.25f, fast: 0.35f, obstacle: 0.26f, bomb: 0.13f, time: 0.11f,
                preview: "Entropi! Düzensizlik zirvede, kaos büyüyor."),

            60 => Make(60, "Time Boss", "BOSS VII", "Kaos Dönemi", 116000,
                spawnInterval: 0.22f, gravity: 0.61f, special: 0.25f, fast: 0.35f, obstacle: 0.26f, bomb: 0.14f, time: 0.11f,
                preview: "BOSS: Zaman Lordu! Süreyi azaltıyor ve zamanı bükiyor!"),

            61 => Make(61, "Glitch Zone", "CHAOS X", "Kaos Dönemi", 113000,
                spawnInterval: 0.22f, gravity: 0.62f, special: 0.25f, fast: 0.35f, obstacle: 0.26f, bomb: 0.14f, time: 0.11f,
                preview: "Glitch bölgesi! Dotlar titreşiyor ve beklenmedik davranıyor."),

            62 => Make(62, "Resonance", "CHAOS XI", "Kaos Dönemi", 115000,
                spawnInterval: 0.22f, gravity: 0.62f, special: 0.25f, fast: 0.35f, obstacle: 0.26f, bomb: 0.14f, time: 0.11f,
                preview: "Rezonans! Aynı renk dotlar birbirini çekiyor."),

            63 => Make(63, "Shatter Point", "CHAOS XII", "Kaos Dönemi", 117000,
                spawnInterval: 0.21f, gravity: 0.63f, special: 0.26f, fast: 0.36f, obstacle: 0.26f, bomb: 0.14f, time: 0.11f,
                preview: "Kırılma noktası! Engeller daha dayanıklı hale geliyor."),

            64 => Make(64, "Dark Matter", "CHAOS XIII", "Kaos Dönemi", 119000,
                spawnInterval: 0.21f, gravity: 0.63f, special: 0.26f, fast: 0.36f, obstacle: 0.27f, bomb: 0.14f, time: 0.11f,
                preview: "Karanlık madde! Yeni bir güç kaynağı ortaya çıkıyor."),

            65 => Make(65, "Nova Burst", "CHAOS XIV", "Kaos Dönemi", 121000,
                spawnInterval: 0.21f, gravity: 0.64f, special: 0.26f, fast: 0.36f, obstacle: 0.27f, bomb: 0.15f, time: 0.12f,
                preview: "Nova patlaması! Dev patlamalar, dev puanlar!"),

            66 => Make(66, "Quantum Flux", "CHAOS XV", "Kaos Dönemi", 123000,
                spawnInterval: 0.21f, gravity: 0.64f, special: 0.26f, fast: 0.36f, obstacle: 0.27f, bomb: 0.15f, time: 0.12f,
                preview: "Kuantum akışı! Dotlar iki durumda aynı anda bulunuyor."),

            67 => Make(67, "Meltdown", "CHAOS XVI", "Kaos Dönemi", 125000,
                spawnInterval: 0.21f, gravity: 0.65f, special: 0.27f, fast: 0.36f, obstacle: 0.27f, bomb: 0.15f, time: 0.12f,
                preview: "Erime! Sıcaklık dorukta, kontrol zorlaşıyor."),

            68 => Make(68, "Cascade", "CHAOS XVII", "Kaos Dönemi", 127000,
                spawnInterval: 0.20f, gravity: 0.65f, special: 0.27f, fast: 0.37f, obstacle: 0.28f, bomb: 0.15f, time: 0.12f,
                preview: "Çağlayan! Zincirleme reaksiyonlar patlıyor."),

            69 => Make(69, "Supernova", "CHAOS XVIII", "Kaos Dönemi", 129000,
                spawnInterval: 0.20f, gravity: 0.66f, special: 0.27f, fast: 0.37f, obstacle: 0.28f, bomb: 0.15f, time: 0.12f,
                preview: "Süpernova! Patlama gücü dorukta."),

            70 => Make(70, "Dual Color", "CHAOS XIX", "Kaos Dönemi", 132000,
                spawnInterval: 0.20f, gravity: 0.66f, special: 0.27f, fast: 0.37f, obstacle: 0.28f, bomb: 0.16f, time: 0.12f,
                preview: "Çift renk! Dotlar iki renk taşıyor. Strateji yeniden tanımlanıyor."),

            71 => Make(71, "Plasma Core", "CHAOS XX", "Kaos Dönemi", 134000,
                spawnInterval: 0.20f, gravity: 0.67f, special: 0.28f, fast: 0.37f, obstacle: 0.28f, bomb: 0.16f, time: 0.12f,
                preview: "Plazma çekirdeği! Enerji seviyesi kritik."),

            72 => Make(72, "Hyper Drive", "CHAOS XXI", "Kaos Dönemi", 136000,
                spawnInterval: 0.20f, gravity: 0.67f, special: 0.28f, fast: 0.38f, obstacle: 0.28f, bomb: 0.16f, time: 0.13f,
                preview: "Hiper sürüş! Hız sınırları aşılıyor."),

            73 => Make(73, "Annihilation", "CHAOS XXII", "Kaos Dönemi", 138000,
                spawnInterval: 0.19f, gravity: 0.68f, special: 0.28f, fast: 0.38f, obstacle: 0.29f, bomb: 0.16f, time: 0.13f,
                preview: "Yok oluş! Madde ve anti-madde çarpışıyor."),

            74 => Make(74, "Event Horizon", "CHAOS XXIII", "Kaos Dönemi", 140000,
                spawnInterval: 0.19f, gravity: 0.68f, special: 0.28f, fast: 0.38f, obstacle: 0.29f, bomb: 0.17f, time: 0.13f,
                preview: "Olay ufku! Kara deliğin kenarındasın. Dönüşü yok!"),

            75 => Make(75, "Chaos Incarnate", "BOSS VIII", "Kaos Dönemi", 145000,
                spawnInterval: 0.19f, gravity: 0.69f, special: 0.29f, fast: 0.38f, obstacle: 0.29f, bomb: 0.17f, time: 0.13f,
                preview: "BOSS: Kaos Bedeni! Tüm kaos güçleri bir bedende toplanmış!"),

            // ════════════════════════════════════════════
            // PHASE 7: ENDGAME (Level 76-100)
            // ════════════════════════════════════════════
            76 => Make(76, "Beyond Limits", "ENDGAME I", "Son Oyun", 142000,
                spawnInterval: 0.19f, gravity: 0.69f, special: 0.29f, fast: 0.38f, obstacle: 0.29f, bomb: 0.17f, time: 0.13f,
                preview: "Sınırların ötesinde! Artık bir uzman olarak kabul edildin."),

            77 => Make(77, "Absolute Zero", "ENDGAME II", "Son Oyun", 144000,
                spawnInterval: 0.19f, gravity: 0.70f, special: 0.29f, fast: 0.39f, obstacle: 0.29f, bomb: 0.17f, time: 0.13f,
                preview: "Mutlak sıfır! Her şey dondurucu hızda ilerliyor."),

            78 => Make(78, "Inferno", "ENDGAME III", "Son Oyun", 146000,
                spawnInterval: 0.19f, gravity: 0.70f, special: 0.29f, fast: 0.39f, obstacle: 0.30f, bomb: 0.17f, time: 0.13f,
                preview: "Cehennem! Ateş her yerde, kaçış yok."),

            79 => Make(79, "Nexus Point", "ENDGAME IV", "Son Oyun", 148000,
                spawnInterval: 0.18f, gravity: 0.71f, special: 0.30f, fast: 0.39f, obstacle: 0.30f, bomb: 0.18f, time: 0.14f,
                preview: "Bağlantı noktası! Tüm elementler burada birleşiyor."),

            80 => Make(80, "Chaos Mode", "ENDGAME V", "Son Oyun", 152000,
                spawnInterval: 0.18f, gravity: 0.71f, special: 0.30f, fast: 0.39f, obstacle: 0.30f, bomb: 0.18f, time: 0.14f,
                preview: "KAOS MODU! Her şey karışık, her şey aynı anda. Sadece en iyiler ayakta kalır."),

            81 => Make(81, "Hell Gate", "ENDGAME VI", "Son Oyun", 154000,
                spawnInterval: 0.18f, gravity: 0.72f, special: 0.30f, fast: 0.40f, obstacle: 0.30f, bomb: 0.18f, time: 0.14f,
                preview: "Cehennem kapısı! İçeri girersen geri dönemezsin."),

            82 => Make(82, "Demon Core", "ENDGAME VII", "Son Oyun", 156000,
                spawnInterval: 0.18f, gravity: 0.72f, special: 0.30f, fast: 0.40f, obstacle: 0.30f, bomb: 0.18f, time: 0.14f,
                preview: "Şeytan çekirdeği! Patlama gücü artık kontrol edilemez."),

            83 => Make(83, "Omega Pulse", "ENDGAME VIII", "Son Oyun", 158000,
                spawnInterval: 0.18f, gravity: 0.73f, special: 0.30f, fast: 0.40f, obstacle: 0.31f, bomb: 0.18f, time: 0.14f,
                preview: "Omega darbesi! Son dalga yaklaşıyor."),

            84 => Make(84, "Oblivion", "ENDGAME IX", "Son Oyun", 160000,
                spawnInterval: 0.18f, gravity: 0.73f, special: 0.30f, fast: 0.40f, obstacle: 0.31f, bomb: 0.19f, time: 0.14f,
                preview: "Unutuş! Zaman ve mekan çöküyor."),

            85 => Make(85, "Ragnarok", "ENDGAME X", "Son Oyun", 162000,
                spawnInterval: 0.18f, gravity: 0.74f, special: 0.30f, fast: 0.40f, obstacle: 0.31f, bomb: 0.19f, time: 0.14f,
                preview: "Ragnarök! Tanrıların savaşı başlıyor."),

            86 => Make(86, "Apocalypse", "ENDGAME XI", "Son Oyun", 164000,
                spawnInterval: 0.18f, gravity: 0.74f, special: 0.30f, fast: 0.40f, obstacle: 0.31f, bomb: 0.19f, time: 0.15f,
                preview: "Kıyamet! Son günler yaklaştı."),

            87 => Make(87, "Titan Fall", "ENDGAME XII", "Son Oyun", 166000,
                spawnInterval: 0.17f, gravity: 0.75f, special: 0.30f, fast: 0.40f, obstacle: 0.31f, bomb: 0.19f, time: 0.15f,
                preview: "Titan düşüşü! Devler yıkılıyor."),

            88 => Make(88, "Armageddon", "ENDGAME XIII", "Son Oyun", 168000,
                spawnInterval: 0.17f, gravity: 0.75f, special: 0.30f, fast: 0.40f, obstacle: 0.31f, bomb: 0.19f, time: 0.15f,
                preview: "Armagedon! Son savaş başlıyor."),

            89 => Make(89, "Extinction", "ENDGAME XIV", "Son Oyun", 170000,
                spawnInterval: 0.17f, gravity: 0.76f, special: 0.30f, fast: 0.40f, obstacle: 0.32f, bomb: 0.20f, time: 0.15f,
                preview: "Yokoluş! Sadece en güçlü hayatta kalır."),

            90 => Make(90, "One Mistake", "ENDGAME XV", "Son Oyun", 175000,
                spawnInterval: 0.17f, gravity: 0.76f, special: 0.30f, fast: 0.40f, obstacle: 0.32f, bomb: 0.20f, time: 0.15f,
                preview: "TEK HATA! Yanlış zincir combo'yu sıfırlar ve hız artar!"),

            91 => Make(91, "Last Stand", "ENDGAME XVI", "Son Oyun", 177000,
                spawnInterval: 0.17f, gravity: 0.77f, special: 0.30f, fast: 0.40f, obstacle: 0.32f, bomb: 0.20f, time: 0.15f,
                preview: "Son direniş! Her saniye önemli."),

            92 => Make(92, "Zero Hour", "ENDGAME XVII", "Son Oyun", 179000,
                spawnInterval: 0.17f, gravity: 0.77f, special: 0.30f, fast: 0.40f, obstacle: 0.32f, bomb: 0.20f, time: 0.15f,
                preview: "Sıfır saati! Geri sayım son tura giriyor."),

            93 => Make(93, "Nemesis", "ENDGAME XVIII", "Son Oyun", 181000,
                spawnInterval: 0.17f, gravity: 0.78f, special: 0.30f, fast: 0.40f, obstacle: 0.32f, bomb: 0.20f, time: 0.15f,
                preview: "Nemesis! En büyük düşmanınla yüzleş."),

            94 => Make(94, "Doomsday", "ENDGAME XIX", "Son Oyun", 183000,
                spawnInterval: 0.17f, gravity: 0.78f, special: 0.30f, fast: 0.40f, obstacle: 0.32f, bomb: 0.20f, time: 0.16f,
                preview: "Kıyamet günü! Artık geri dönüş yok."),

            95 => Make(95, "Final Countdown", "ENDGAME XX", "Son Oyun", 186000,
                spawnInterval: 0.16f, gravity: 0.79f, special: 0.30f, fast: 0.40f, obstacle: 0.32f, bomb: 0.20f, time: 0.16f,
                preview: "Son geri sayım! 5 level kaldı..."),

            96 => Make(96, "Judgement", "ENDGAME XXI", "Son Oyun", 189000,
                spawnInterval: 0.16f, gravity: 0.79f, special: 0.30f, fast: 0.40f, obstacle: 0.33f, bomb: 0.21f, time: 0.16f,
                preview: "Yargı! Her hareketin yargılanıyor."),

            97 => Make(97, "Purgatory", "ENDGAME XXII", "Son Oyun", 192000,
                spawnInterval: 0.16f, gravity: 0.80f, special: 0.30f, fast: 0.40f, obstacle: 0.33f, bomb: 0.21f, time: 0.16f,
                preview: "Araf! Ne cennet ne cehennem. Sonsuz savaş."),

            98 => Make(98, "Ascension", "ENDGAME XXIII", "Son Oyun", 195000,
                spawnInterval: 0.16f, gravity: 0.80f, special: 0.30f, fast: 0.40f, obstacle: 0.33f, bomb: 0.21f, time: 0.16f,
                preview: "Yükseliş! Son zirveye tırmanıyorsun."),

            99 => Make(99, "Edge of Void", "ENDGAME XXIV", "Son Oyun", 198000,
                spawnInterval: 0.16f, gravity: 0.81f, special: 0.30f, fast: 0.40f, obstacle: 0.33f, bomb: 0.21f, time: 0.16f,
                preview: "Boşluğun kenarı! Bir adım daha ve sonsuzluğa düşersin."),

            100 => Make(100, "THE VOID", "FINAL BOSS", "Son Oyun", 250000,
                spawnInterval: 0.15f, gravity: 0.82f, special: 0.30f, fast: 0.40f, obstacle: 0.33f, bomb: 0.22f, time: 0.16f,
                preview: "FİNAL: THE VOID! Sonsuz büyüyen karadelik, tüm özel dotlar, 3 fazlı savaş. PRESTIGE açılır!"),

            _ => GetInfiniteLevel(lvl)
        };
    }

    private static LevelInfo GetInfiniteLevel(int lvl)
    {
        int extra = lvl - 100;
        string romanNumeral = ToRoman(extra);

        return new LevelInfo
        {
            Level = lvl,
            Title = $"Prestige {extra}",
            Subtitle = $"PRESTIGE {romanNumeral}",
            PhaseName = "Sonsuz",
            TargetScore = 50000 + extra * 2500,
            SpawnInterval = Mathf.Max(0.12f, 1f / (1f + lvl * 0.03f)),
            GravityScale = Mathf.Min(0.85f, 0.42f + lvl * 0.004f),
            SpecialDotChance = Mathf.Min(0.30f, 0.10f + lvl * 0.005f),
            FastDotChance = Mathf.Min(0.40f, 0.25f + lvl * 0.003f),
            ObstacleChance = Mathf.Min(0.35f, 0.20f + lvl * 0.003f),
            BombChance = Mathf.Min(0.25f, 0.10f + lvl * 0.003f),
            TimeDotChance = Mathf.Min(0.18f, 0.08f + lvl * 0.002f),
            PreviewText = $"Prestige {extra}! Zorluk: {lvl * 0.08f:F1}x. Daha hızlı, daha fazla özel dot, daha az hata payı!"
        };
    }

    private static LevelInfo Make(int level, string title, string subtitle, string phase, int target,
        float spawnInterval = 0.8f, float gravity = 0.15f,
        float special = 0f, float fast = 0f, float obstacle = 0f,
        float bomb = 0f, float time = 0f, string preview = "")
    {
        return new LevelInfo
        {
            Level = level,
            Title = title,
            Subtitle = subtitle,
            PhaseName = phase,
            TargetScore = target,
            SpawnInterval = spawnInterval,
            GravityScale = gravity,
            SpecialDotChance = special,
            FastDotChance = fast,
            ObstacleChance = obstacle,
            BombChance = bomb,
            TimeDotChance = time,
            PreviewText = preview
        };
    }

    private static string ToRoman(int number)
    {
        if (number <= 0) return "I";
        string[] thousands = { "", "M", "MM", "MMM" };
        string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
        string[] tens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
        string[] ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

        return thousands[Mathf.Min(number / 1000, 3)]
             + hundreds[Mathf.Min((number % 1000) / 100, 9)]
             + tens[Mathf.Min((number % 100) / 10, 9)]
             + ones[Mathf.Min(number % 10, 9)];
    }

    public static string GetLevelDisplayTitle(int lvl)
    {
        LevelInfo info = GetLevel(lvl);
        return info.Title;
    }

    public static string GetLevelDisplaySubtitle(int lvl)
    {
        LevelInfo info = GetLevel(lvl);
        return info.Subtitle;
    }
}
