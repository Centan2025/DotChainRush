using UnityEngine;
using UnityEditor;

public class Level10GeneratorWindow : EditorWindow
{
    [MenuItem("DotChainRush/Generate Level 10 Chaos Boss")]
    public static void ShowWindow()
    {
        GetWindow<Level10GeneratorWindow>("Level 10 Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level 10: Chaos Orb Dimension", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generate Level 10 Data and Prefabs"))
        {
            GenerateLevel10();
        }
    }

    private void GenerateLevel10()
    {
        Debug.Log("Generating Level 10 Chaos Boss System...");

        // Ensure we have a BossDimensionManager
        GameObject bdm = GameObject.Find("BossDimensionManager");
        if (bdm == null)
        {
            bdm = new GameObject("BossDimensionManager");
            bdm.AddComponent<BossDimensionManager>();
        }

        // Add ChaosOrbBossSystem
        if (bdm.GetComponent<ChaosOrbBossSystem>() == null)
        {
            bdm.AddComponent<ChaosOrbBossSystem>();
        }

        // Ensure ObstacleDirector
        GameObject obstacleDirector = GameObject.Find("Level10ObstacleDirector");
        if (obstacleDirector == null)
        {
            obstacleDirector = new GameObject("Level10ObstacleDirector");
            obstacleDirector.AddComponent<Level10ObstacleDirector>();
        }

        // Ensure ArenaBuilder
        GameObject arenaBuilder = GameObject.Find("ArenaBuilder");
        if (arenaBuilder == null)
        {
            arenaBuilder = new GameObject("ArenaBuilder");
            arenaBuilder.AddComponent<ArenaBuilder>();
        }

        // Add UI Managers
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            if (canvas.GetComponentInChildren<BossDimensionUI>() == null)
            {
                GameObject bossUI = new GameObject("BossDimensionUI");
                bossUI.transform.SetParent(canvas.transform, false);
                bossUI.AddComponent<BossDimensionUI>();
            }

            if (canvas.GetComponentInChildren<RewardChoiceUI>() == null)
            {
                GameObject rewardUI = new GameObject("RewardChoiceUI");
                rewardUI.transform.SetParent(canvas.transform, false);
                rewardUI.AddComponent<RewardChoiceUI>();
            }
        }

        Debug.Log("Level 10 Systems Generated in Scene!");
    }
}
