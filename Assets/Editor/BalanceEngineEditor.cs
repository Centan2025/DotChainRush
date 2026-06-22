#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class BalanceEngineEditor : EditorWindow
{
    [MenuItem("Balancing/AAA Curve & Simulation Lab")]
    public static void ShowWindow()
    {
        GetWindow<BalanceEngineEditor>("Simulation Lab");
    }

    private void OnGUI()
    {
        GUILayout.Label("AAA Balancing & Procedural Difficulty Ecosystem", EditorStyles.boldLabel);
        GUILayout.Space(15);

        if (GUILayout.Button("GENERATE 200 LEVELS (ScriptableObjects)", GUILayout.Height(35)))
        {
            GenerateLevelsAssetDatabase();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("RUN SIMULATION (10k Virtual Runs)", GUILayout.Height(35)))
        {
            Run10kVirtualSimulation();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("EXPORT BALANCE REPORT", GUILayout.Height(35)))
        {
            ExportDiagnosticReport();
        }
    }

    private void GenerateLevelsAssetDatabase()
    {
        string dirPath = "Assets/GeneratedAssets/Levels";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        // Gather all existing BallConfigs or build mock configs for generation
        List<BallConfig> balls = GetBallConfigs();

        AutoDifficultyCurve curveCalculator = new AutoDifficultyCurve();
        ProceduralBossAI bossGenerator = new ProceduralBossAI();

        for (int i = 1; i <= 200; i++)
        {
            LevelConfigSO asset = ScriptableObject.CreateInstance<LevelConfigSO>();
            asset.id = i;
            asset.isBossLevel = (i % 10 == 0);
            asset.theme = asset.isBossLevel ? "Boss Arena" : "Neon Grid";
            asset.baseTimer = 90f;
            asset.difficultyFactor = curveCalculator.CalculateDifficultyFactor(i);
            asset.spawnInterval = Mathf.Max(0.15f, 0.85f - (i * 0.0035f));
            asset.gravityScale = Mathf.Min(0.85f, 0.12f + (i * 0.0035f));
            asset.targetScore = 1000 + i * 1500;

            // Associate ball configurations unlocked at this level
            foreach (var ball in balls)
            {
                if (ball.unlockLevel + ball.unlockLevelOffset <= i)
                {
                    asset.allowedBalls.Add(ball);
                }
            }

            if (asset.isBossLevel)
            {
                asset.bossConfig = bossGenerator.GenerateBoss(i, i * 777);
                asset.targetScore = Mathf.RoundToInt(asset.targetScore * 1.3f);
                asset.previewText = $"BOSS {i}: Dev {asset.bossConfig.bossType} topunu yok et!";
            }
            else
            {
                asset.previewText = $"Seviye {i}: Hızlı refleksler, neon zincirler!";
            }

            string assetPath = $"{dirPath}/Level_{i}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Başarılı", "200 Seviye asset dosyası Assets/GeneratedAssets/Levels/ altında başarıyla oluşturuldu!", "Tamam");
    }

    private void Run10kVirtualSimulation()
    {
        List<LevelConfigSO> levels = GetGeneratedLevels();
        if (levels.Count == 0)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen önce 'GENERATE 200 LEVELS' butonuna basarak seviye dosyalarını oluşturun!", "Tamam");
            return;
        }

        SimulationTester tester = new SimulationTester();
        SimulationReport report = tester.RunSimulation(levels, 10000);

        string msg = $"Simülasyon Tamamlandı!\n\nOrtalama Kazanma Oranı: {report.averageWinRate:P1}\nZorluk Eğrisi Skoru: {report.difficultyCurveScore:F2}\nKırık/Dengesiz Seviyeler: {report.brokenLevels.Count}\nZaman Aşımı Sayısı: {report.timeOverflowsCount}";
        EditorUtility.DisplayDialog("Simülasyon Raporu", msg, "Tamam");
    }

    private void ExportDiagnosticReport()
    {
        string exportPath = "Assets/BalanceReport.txt";
        List<LevelConfigSO> levels = GetGeneratedLevels();

        using (StreamWriter sw = new StreamWriter(exportPath))
        {
            sw.WriteLine("--- DOT GAME AAA BALANCE DIAGNOSTIC REPORT ---");
            sw.WriteLine($"Generated at: {System.DateTime.Now}");
            sw.WriteLine($"Total levels evaluated: {levels.Count}");
            sw.WriteLine();
            
            foreach (var lvl in levels)
            {
                sw.WriteLine($"Level {lvl.id} | Goal: {lvl.targetScore} | Gravity: {lvl.gravityScale:F3} | Interval: {lvl.spawnInterval:F3}s | Difficulty Factor: {lvl.difficultyFactor:F2}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Başarılı", "Dengeleme raporu Assets/BalanceReport.txt konumuna başarıyla ihraç edildi!", "Harika");
    }

    private List<BallConfig> GetBallConfigs()
    {
        List<BallConfig> list = new List<BallConfig>();
        string[] guids = AssetDatabase.FindAssets("t:BallConfig");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BallConfig config = AssetDatabase.LoadAssetAtPath<BallConfig>(path);
            if (config != null) list.Add(config);
        }

        // If no configs in DB, auto-populate default set
        if (list.Count == 0)
        {
            string dir = "Assets/GeneratedAssets/Balls";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            foreach (TopTipi type in System.Enum.GetValues(typeof(TopTipi)))
            {
                BallConfig asset = ScriptableObject.CreateInstance<BallConfig>();
                asset.type = type;
                asset.baseSpawnWeight = BalanceDB.IsNormalColor(type) ? 1.0f : 0.15f;
                asset.unlockLevel = (int)type; // simple level mapping
                AssetDatabase.CreateAsset(asset, $"{dir}/{type}.asset");
                list.Add(asset);
            }
            AssetDatabase.SaveAssets();
        }

        return list;
    }

    private List<LevelConfigSO> GetGeneratedLevels()
    {
        List<LevelConfigSO> list = new List<LevelConfigSO>();
        string[] guids = AssetDatabase.FindAssets("t:LevelConfigSO");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelConfigSO config = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(path);
            if (config != null) list.Add(config);
        }
        // Sort by ID
        list.Sort((x, y) => x.id.CompareTo(y.id));
        return list;
    }
}
#endif
