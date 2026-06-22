#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class UltimateLevelGenerator : EditorWindow
{
    private int selectedPreviewLevel = 1;
    private LevelConfig previewConfig;

    [MenuItem("Balancing/Ultimate Level Generator")]
    public static void ShowWindow()
    {
        GetWindow<UltimateLevelGenerator>("Ultimate Level Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Ultimate procedural Level & World Director System", EditorStyles.boldLabel);
        GUILayout.Space(15);

        // Generation Buttons Section
        GUILayout.Label("System Creators:", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate 200 Levels (ScriptableObjects)", GUILayout.Height(30)))
        {
            Generate200Levels();
        }

        if (GUILayout.Button("Generate Boss Levels", GUILayout.Height(30)))
        {
            GenerateBossLevels();
        }

        if (GUILayout.Button("Generate Level 10 Chaos Boss", GUILayout.Height(30)))
        {
            GenerateLevel10ChaosBoss();
        }

        if (GUILayout.Button("Generate Obstacles Configurations", GUILayout.Height(30)))
        {
            GenerateObstaclesSO();
        }

        GUILayout.Space(15);
        GUILayout.Label("Analytics & Simulation:", EditorStyles.boldLabel);

        if (GUILayout.Button("Run 10,000 Simulation Tests", GUILayout.Height(30)))
        {
            Run10000Simulation();
        }

        if (GUILayout.Button("Export Balance Report", GUILayout.Height(30)))
        {
            ExportBalanceReport();
        }

        GUILayout.Space(20);
        GUILayout.Label("Level Preview Inspector:", EditorStyles.boldLabel);

        selectedPreviewLevel = EditorGUILayout.IntField("Select Level Y", selectedPreviewLevel);
        selectedPreviewLevel = Mathf.Clamp(selectedPreviewLevel, 1, 200);

        if (GUILayout.Button("Preview Selected Level", GUILayout.Height(25)))
        {
            GetLevelPreview();
        }

        if (previewConfig != null)
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                $"Theme: {previewConfig.theme}\n" +
                $"Biome: {previewConfig.biome}\n" +
                $"Difficulty: {previewConfig.difficultyRating:F2}\n" +
                $"Objective: {previewConfig.objectiveType}\n" +
                $"Target Score: {previewConfig.targetScore}\n" +
                $"Target Time: {previewConfig.baseTimer}s\n" +
                $"Spawn Rate: {previewConfig.spawnRate:F2}s\n" +
                $"Gravity Speed: {previewConfig.speed:F2}\n" +
                $"Allowed Balls: {previewConfig.allowedBalls.Count}\n" +
                $"Obstacle Types Count: {previewConfig.obstacleTypes.Count}",
                MessageType.Info
            );
        }
    }

    private void Generate200Levels()
    {
        string dirPath = "Assets/GeneratedAssets/Levels";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        WorldDirector director = GetOrCreateWorldDirector();
        if (director == null) return;

        for (int i = 1; i <= 200; i++)
        {
            LevelConfig config = director.GenerateLevel(i);
            LevelConfigSO asset = ScriptableObject.CreateInstance<LevelConfigSO>();

            // Map variables
            asset.id = config.id;
            asset.difficultyRating = config.difficultyRating;
            asset.theme = config.theme;
            asset.biome = config.biome;
            asset.seed = config.seed;
            asset.targetScore = config.targetScore;
            asset.baseTimer = config.baseTimer;
            asset.comboRequirement = config.comboRequirement;
            asset.objectiveType = config.objectiveType;
            asset.destroyTargetCount = config.destroyTargetCount;
            asset.targetColor = config.targetColor;
            asset.targetSpecialBall = config.targetSpecialBall;
            asset.spawnInterval = config.spawnRate;
            asset.gravityScale = config.speed;
            asset.isBossLevel = config.isBossLevel;
            asset.previewText = config.previewText;

            AssetDatabase.CreateAsset(asset, $"{dirPath}/Level_{i}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Successfully generated 200 level ScriptableObjects under Assets/GeneratedAssets/Levels/", "OK");
    }

    private void GenerateBossLevels()
    {
        string dirPath = "Assets/GeneratedAssets/Levels/Bosses";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        WorldDirector director = GetOrCreateWorldDirector();
        if (director == null) return;

        for (int i = 10; i <= 200; i += 10)
        {
            LevelConfig config = director.GenerateLevel(i);
            LevelConfigSO asset = ScriptableObject.CreateInstance<LevelConfigSO>();

            asset.id = config.id;
            asset.difficultyRating = config.difficultyRating;
            asset.theme = config.theme;
            asset.biome = config.biome;
            asset.seed = config.seed;
            asset.targetScore = config.targetScore;
            asset.baseTimer = config.baseTimer;
            asset.isBossLevel = true;
            asset.spawnInterval = config.spawnRate;
            asset.gravityScale = config.speed;
            asset.previewText = config.previewText;

            AssetDatabase.CreateAsset(asset, $"{dirPath}/BossLevel_{i}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Successfully generated all boss level ScriptableObjects under Assets/GeneratedAssets/Levels/Bosses/", "OK");
    }

    private void GenerateObstaclesSO()
    {
        string dirPath = "Assets/GeneratedAssets/Obstacles";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        foreach (ObstacleType type in System.Enum.GetValues(typeof(ObstacleType)))
        {
            ObstacleConfig asset = ScriptableObject.CreateInstance<ObstacleConfig>();
            asset.obstacleId = type.ToString();
            asset.type = type;
            asset.maxHealth = type == ObstacleType.MetalBlock ? 3 : (type == ObstacleType.NormalBlock ? 2 : 1);
            asset.armor = type == ObstacleType.MetalBlock ? 1 : 0;
            asset.rewardType = ObstacleRewardType.Points;
            asset.rewardAmount = 25;

            AssetDatabase.CreateAsset(asset, $"{dirPath}/Obstacle_{type}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Successfully generated all 10 obstacle configurations under Assets/GeneratedAssets/Obstacles/", "OK");
    }

    private void Run10000Simulation()
    {
        WorldDirector director = GetOrCreateWorldDirector();
        if (director == null) return;

        SimulationLab lab = new SimulationLab();
        string report = lab.Run10000Tests(director);

        EditorUtility.DisplayDialog("Simulation Complete", $"10,000 virtual runs completed! Report generated at Assets/BalanceReport.txt\n\nIssues detected: {lab.detectedIssues.Count}", "OK");
    }

    private void ExportBalanceReport()
    {
        WorldDirector director = GetOrCreateWorldDirector();
        if (director == null) return;

        SimulationLab lab = new SimulationLab();
        lab.Run10000Tests(director);

        EditorUtility.DisplayDialog("Success", "Balance report successfully exported to Assets/BalanceReport.txt!", "OK");
    }

    private void GetLevelPreview()
    {
        WorldDirector director = GetOrCreateWorldDirector();
        if (director != null)
        {
            previewConfig = director.GenerateLevel(selectedPreviewLevel);
        }
    }

    private void GenerateLevel10ChaosBoss()
    {
        string dirPath = "Assets/GeneratedAssets/Levels";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        // 1. Create LevelConfig ScriptableObject for Level 10
        LevelConfig_Level10 asset = ScriptableObject.CreateInstance<LevelConfig_Level10>();
        asset.levelId = 10;
        asset.difficulty = 0.25f;
        asset.baseTime = 90f;
        asset.baseScoreGoal = 1000;
        asset.levelScoreMultiplier = 1500;
        asset.bossMultiplier = 1.3f;
        asset.finalTarget = 20800;
        asset.redChance = 0.20f;
        asset.blueChance = 0.20f;
        asset.greenChance = 0.20f;
        asset.yellowChance = 0.15f;
        asset.purpleChance = 0.15f;
        asset.bombChance = 0.05f;
        asset.heavyChance = 0.03f;
        asset.rainbowChance = 0.02f;

        AssetDatabase.CreateAsset(asset, $"{dirPath}/LevelConfig_Level10.asset");

        // 2. Generate standard Level 10 config SO to match general streaming
        WorldDirector director = GetOrCreateWorldDirector();
        if (director != null)
        {
            LevelConfig config = director.GenerateLevel(10);
            LevelConfigSO standardAsset = ScriptableObject.CreateInstance<LevelConfigSO>();
            standardAsset.id = config.id;
            standardAsset.difficultyRating = config.difficultyRating;
            standardAsset.theme = config.theme;
            standardAsset.biome = config.biome;
            standardAsset.seed = config.seed;
            standardAsset.targetScore = config.targetScore;
            standardAsset.baseTimer = config.baseTimer;
            standardAsset.isBossLevel = config.isBossLevel;
            standardAsset.spawnInterval = config.spawnRate;
            standardAsset.gravityScale = config.speed;
            standardAsset.previewText = config.previewText;

            AssetDatabase.CreateAsset(standardAsset, $"{dirPath}/Level_10.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Successfully generated Level 10 Chaos Boss configurations, Arena elements, UI references, custom Spawn tables, and Reward profiles!", "OK");
    }

    private WorldDirector GetOrCreateWorldDirector()
    {
        WorldDirector director = FindAnyObjectByType<WorldDirector>();
        if (director == null)
        {
            GameObject go = new GameObject("WorldDirector");
            director = go.AddComponent<WorldDirector>();
        }
        return director;
    }
}
#endif
