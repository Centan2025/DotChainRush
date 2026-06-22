using System.Collections.Generic;
using UnityEngine;

public class GameBrain : MonoBehaviour
{
    public static GameBrain Instance { get; private set; }

    [Header("Engine Components")]
    public MetaLearner meta = new MetaLearner();
    public BalanceAI balance = new BalanceAI();
    public RunDirector run = new RunDirector();
    public BossAI boss = new BossAI();
    public Director aiDirector = new Director();
    public LevelStream levelStream = new LevelStream();
    public SimulationLab simLab = new SimulationLab();

    public LevelConfig CurrentLevelConfig { get; private set; }
    
    // Active player mutations (Upgrades chosen during the run)
    public Dictionary<string, float> activeMutations = new Dictionary<string, float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEngine();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEngine()
    {
        BalanceDB.InitializeDefaults();
        TelemetrySystem.ResetTelemetry();
        
        // Populate first default level configuration
        SetCurrentLevel(1);

        // Run simulation tests offline at launch to calibrate
        simLab.Run10000Tests(new LevelGenerator());
    }

    private void Update()
    {
        // Difficulty manager scales game speed/gravity dynamically based on telemetry
        if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            aiDirector.Adjust();
        }
    }

    public void SetCurrentLevel(int level)
    {
        CurrentLevelConfig = levelStream.Get(level);
        
        if (CurrentLevelConfig.isBossLevel)
        {
            boss.GenerateBossTraits(level * 777);
        }
    }

    public void OnLevelEnded(bool won)
    {
        if (CurrentLevelConfig == null) return;

        // Record telemetry data
        TelemetrySystem.RecordLevelResult(won, CurrentLevelConfig.allowedBalls);

        if (won)
        {
            // Train meta weights and auto-balance DB
            meta.Train();
            balance.Evaluate();
            
            // Sync dashboard
            SheetSync.SendTelemetry();
        }
    }

    public void AddMutation(string upgradeName, float value)
    {
        if (activeMutations.ContainsKey(upgradeName))
        {
            activeMutations[upgradeName] += value;
        }
        else
        {
            activeMutations[upgradeName] = value;
        }
        Debug.LogFormat("[GameBrain] Applied upgrade mutation: {0} = +{1}", upgradeName, value);
    }

    public float GetMutationValue(string upgradeName)
    {
        return activeMutations.ContainsKey(upgradeName) ? activeMutations[upgradeName] : 0f;
    }

    public void ResetRun()
    {
        activeMutations.Clear();
        run.GenerateNewRun(1, 12); // Hades run of depth 12
    }
}
