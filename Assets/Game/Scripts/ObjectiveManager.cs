using UnityEngine;
using System.Collections;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Runtime Progress")]
    public int currentDestroyCount = 0;
    public int highestComboAchieved = 0;
    public int targetColorPoppedCount = 0;
    public int targetSpecialBallPoppedCount = 0;
    public float survivalTimeAccumulated = 0f;

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

    public void ResetObjectives()
    {
        currentDestroyCount = 0;
        highestComboAchieved = 0;
        targetColorPoppedCount = 0;
        targetSpecialBallPoppedCount = 0;
        survivalTimeAccumulated = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        survivalTimeAccumulated += Time.deltaTime;

        // Check if objective is met every frame for safety
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckLevelCompletion();
        }
    }

    public void RegisterObstacleDestroyed()
    {
        currentDestroyCount++;
    }

    public void RegisterCombo(int length)
    {
        if (length > highestComboAchieved)
        {
            highestComboAchieved = length;
        }
    }

    public void RegisterDotPopped(Dot dot)
    {
        if (GameBrain.Instance == null || GameBrain.Instance.CurrentLevelConfig == null) return;
        var cfg = GameBrain.Instance.CurrentLevelConfig;

        // Color objective check
        if (cfg.objectiveType == LevelObjectiveType.Color && dot.ColorId == GetColorId(cfg.targetColor))
        {
            targetColorPoppedCount++;
        }

        // Special ball objective check
        if (cfg.objectiveType == LevelObjectiveType.SpecialBall && (TopTipi)dot.Type == cfg.targetSpecialBall)
        {
            targetSpecialBallPoppedCount++;
        }
    }

    private int GetColorId(TopTipi type)
    {
        if (type == TopTipi.KirmiziTop) return 0;
        if (type == TopTipi.MaviTop) return 1;
        if (type == TopTipi.YesilTop) return 2;
        if (type == TopTipi.SariTop) return 3;
        if (type == TopTipi.MorTop) return 4;
        return -1;
    }

    public bool IsObjectiveMet()
    {
        if (GameBrain.Instance == null || GameBrain.Instance.CurrentLevelConfig == null) return false;
        var cfg = GameBrain.Instance.CurrentLevelConfig;

        // Boss wins are handled by BossDimensionManager
        if (cfg.isBossLevel && BossDimensionManager.Instance != null && BossDimensionManager.Instance.IsBossLevelActive)
        {
            return BossDimensionManager.Instance.CheckBossWinCondition();
        }

        switch (cfg.objectiveType)
        {
            case LevelObjectiveType.Score:
                return ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= cfg.targetScore;

            case LevelObjectiveType.Destroy:
                return currentDestroyCount >= cfg.destroyTargetCount;

            case LevelObjectiveType.Combo:
                return highestComboAchieved >= cfg.comboRequirement;

            case LevelObjectiveType.Survival:
                // Survived for the target time limit
                return survivalTimeAccumulated >= cfg.baseTimer;

            case LevelObjectiveType.Time:
                // Get target score under time limit (handled by: if time runs out, fail, but if score reached before time, win)
                return ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= cfg.targetScore;

            case LevelObjectiveType.Color:
                return targetColorPoppedCount >= cfg.destroyTargetCount; // uses destroyTargetCount as target pop count

            case LevelObjectiveType.SpecialBall:
                return targetSpecialBallPoppedCount >= cfg.destroyTargetCount; // uses destroyTargetCount as target pop count

            default:
                return false;
        }
    }

    public string GetObjectiveStatusText()
    {
        if (GameBrain.Instance == null || GameBrain.Instance.CurrentLevelConfig == null) return "";
        var cfg = GameBrain.Instance.CurrentLevelConfig;

        switch (cfg.objectiveType)
        {
            case LevelObjectiveType.Score:
                int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
                return $"Score: {currentScore} / {cfg.targetScore}";

            case LevelObjectiveType.Destroy:
                return $"Destroy Blocks: {currentDestroyCount} / {cfg.destroyTargetCount}";

            case LevelObjectiveType.Combo:
                return $"Max Combo: {highestComboAchieved} / {cfg.comboRequirement}";

            case LevelObjectiveType.Survival:
                float remainingSurvival = Mathf.Max(0f, cfg.baseTimer - survivalTimeAccumulated);
                return $"Survive: {remainingSurvival:F1}s";

            case LevelObjectiveType.Time:
                int scoreVal = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
                return $"Score: {scoreVal} / {cfg.targetScore}";

            case LevelObjectiveType.Color:
                string colorName = cfg.targetColor.ToString().Replace("Top", "");
                return $"Clear {colorName} Balls: {targetColorPoppedCount} / {cfg.destroyTargetCount}";

            case LevelObjectiveType.SpecialBall:
                return $"Clear {cfg.targetSpecialBall}: {targetSpecialBallPoppedCount} / {cfg.destroyTargetCount}";

            default:
                return "";
        }
    }
}
