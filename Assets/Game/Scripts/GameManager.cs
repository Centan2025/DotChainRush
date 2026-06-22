using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Boot,
    MainMenu,
    Playing,
    Paused,
    LevelComplete,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private Button restartButton;

    public GameState CurrentState { get; private set; } = GameState.Boot;
    public delegate void StateChangedHandler(GameState oldState, GameState newState);
    public static event StateChangedHandler OnStateChanged;

    private float timeElapsed;
    public bool IsPlaying => CurrentState == GameState.Playing;
    
    // Fever Mode variables
    public bool IsFeverActive { get; private set; }
    private float feverTimer = 0f;
    private int feverChainProgress = 0; // Accumulates chain progress towards fever (0..20)
    private const int FeverChainThreshold = 20;

    // Stats variables
    public int BestCombo { get; private set; }
    private bool isLevelingUp = false;
    public int BossHP { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CleanStraySceneColliders();

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        ChangeState(GameState.Playing);
        RestartGame();
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        GameState oldState = CurrentState;
        CurrentState = newState;

        // Apply state transition rules
        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.LevelComplete:
            case GameState.GameOver:
                // Special states handling
                break;
        }

        OnStateChanged?.Invoke(oldState, newState);
    }

    private void CleanStraySceneColliders()
    {
        Collider2D[] colliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (Collider2D col in colliders)
        {
            GameObject go = col.gameObject;
            // Ensure we are cleaning up active scene objects, not prefabs or core game components
            if (go.scene.name != null && 
                go.name != "ScreenBoundaries" && 
                !go.name.Contains("Circle") && 
                !go.name.Contains("Prefab"))
            {
                Vector2 pos = go.transform.position;
                // If the collider is in the middle play area, destroy it
                if (pos.x > -2.6f && pos.x < 2.6f && pos.y > -4.0f && pos.y < 4.5f)
                {
                    Debug.LogWarning($"[Diagnostic] Found and destroyed stray collider in the middle of the screen: Name={go.name}, Position={pos}, Type={col.GetType().Name}");
                    Destroy(go);
                }
            }
        }
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.nKey.wasPressedThisFrame)
            {
                Debug.Log("[Debug Cheat] Skipping to next level...");
                StartCoroutine(LevelUpCoroutine(true));
            }
            if (UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame && DifficultyManager.Instance != null)
            {
                int prevLevel = Mathf.Max(1, DifficultyManager.Instance.ActiveLevel - 1);
                Debug.Log($"[Debug Cheat] Going back to level {prevLevel}...");
                DifficultyManager.Instance.SetLevel(prevLevel);
                RestartGame();
            }
        }
        #endif

        if (CurrentState != GameState.Playing) return;

        timeElapsed += Time.deltaTime;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimer(timeElapsed);
        }

        // Fever countdown and progress bar update
        if (UIManager.Instance != null)
        {
            if (IsFeverActive)
            {
                feverTimer -= Time.deltaTime;
                UIManager.Instance.UpdateFeverProgress(feverTimer, 5f);
                if (feverTimer <= 0f)
                {
                    DeactivateFeverMode();
                }
            }
            else
            {
                // Show accumulated chain progress towards fever threshold
                UIManager.Instance.UpdateFeverProgress(feverChainProgress, FeverChainThreshold);
            }
        }
    }

    public void RegisterChain(int chainLength)
    {
        if (chainLength > BestCombo)
        {
            BestCombo = chainLength;
        }

        if (!IsFeverActive)
        {
            // Accumulate chain progress towards fever
            feverChainProgress = Mathf.Min(feverChainProgress + chainLength, FeverChainThreshold);

            if (feverChainProgress >= FeverChainThreshold)
            {
                feverChainProgress = 0;
                ActivateFeverMode();
            }
        }
    }

    private void ActivateFeverMode()
    {
        IsFeverActive = true;
        feverTimer = 5.0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetFeverActive(true);
            UIManager.Instance.TriggerScreenFlash(Color.yellow, 0.4f);
            UIManager.Instance.ShowComboFeedback("FEVER MODE ACTIVE!", Color.yellow);
        }

        // Instantly slow gravity of all currently active dots
        if (DotSpawner.Instance != null)
        {
            foreach (Dot dot in DotSpawner.Instance.ActiveDots)
            {
                Rigidbody2D dotRb = dot.GetComponent<Rigidbody2D>();
                if (dotRb != null)
                {
                    dotRb.gravityScale = 0.08f;
                }
            }
        }
    }

    private void DeactivateFeverMode()
    {
        IsFeverActive = false;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetFeverActive(false);
        }

        // Restore normal gravity on active dots
        if (DotSpawner.Instance != null)
        {
            float baseGravity = DifficultyManager.Instance != null ? DifficultyManager.Instance.GravityScale : 0.5f;
            foreach (Dot dot in DotSpawner.Instance.ActiveDots)
            {
                Rigidbody2D dotRb = dot.GetComponent<Rigidbody2D>();
                if (dotRb != null)
                {
                    dotRb.gravityScale = dot.IsFastDot ? baseGravity * 2.2f : baseGravity;
                }
            }
        }
    }

    public void AddScore(int dotCount)
    {
        if (!IsPlaying) return;

        int points = dotCount * 10;
        if (IsFeverActive) points *= 10;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(points);
        }
    }

    public void CheckLevelCompletion()
    {
        if (!IsPlaying || isLevelingUp) return;

        if (DifficultyManager.Instance != null && ScoreManager.Instance != null)
        {
            if (ScoreManager.Instance.CurrentScore >= DifficultyManager.Instance.ActiveGoal)
            {
                StartCoroutine(LevelUpCoroutine());
            }
        }
    }

    private System.Collections.IEnumerator LevelUpCoroutine(bool isCheat = false)
    {
        isLevelingUp = true;
        DeactivateFeverMode();
        ChangeState(GameState.LevelComplete);

        if (GameBrain.Instance != null)
        {
            GameBrain.Instance.OnLevelEnded(true);
        }

        if (isCheat)
        {
            if (DotSpawner.Instance != null)
            {
                DotSpawner.Instance.ClearAll();
            }
        }
        else
        {
            // Visual flash and audio celebration
            if (UIManager.Instance != null)
            {
                UIManager.Instance.TriggerScreenFlash(Color.cyan, 0.6f);
            }
            if (AudioManager.Instance != null && DifficultyManager.Instance != null)
            {
                AudioManager.Instance.PlayMilestoneSound(3); // Level-up chord sound
            }

            yield return new WaitForSeconds(0.4f);

            // Melt and sink all active dots downwards
            if (DotSpawner.Instance != null)
            {
                System.Collections.Generic.List<Dot> dotsToMelt = new System.Collections.Generic.List<Dot>(DotSpawner.Instance.ActiveDots);
                foreach (Dot dot in dotsToMelt)
                {
                    if (dot != null && dot.gameObject.activeInHierarchy)
                    {
                        if (ScoreManager.Instance != null && !dot.IsObstacle)
                        {
                            // Award 5 bonus points per dot cleared
                            ScoreManager.Instance.AddPoints(5);
                        }
                        dot.MeltAndSinkDown();
                    }
                }
                DotSpawner.Instance.ActiveDots.Clear();
            }

            yield return new WaitForSeconds(0.8f);

            // Transfer remaining time into bonus points (10 pts per second) one by one quickly
            RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
            CircularTimer ct = FindAnyObjectByType<CircularTimer>();
            float remainingTime = 0f;
            if (rtc != null) remainingTime = rtc.CurrentTime;
            else if (ct != null) remainingTime = ct.CurrentTime;

            int secondsToTransfer = Mathf.CeilToInt(remainingTime);
            if (secondsToTransfer > 0)
            {
                float timeStep = remainingTime / secondsToTransfer;
                for (int i = 0; i < secondsToTransfer; i++)
                {
                    if (rtc != null) rtc.CurrentTime -= timeStep;
                    if (ct != null) ct.CurrentTime -= timeStep;

                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddPoints(10);
                    }

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayConnectSound(i % 15);
                    }

                    yield return new WaitForSeconds(0.02f);
                }
            }

            if (rtc != null) rtc.CurrentTime = 0f;
            if (ct != null) ct.CurrentTime = 0f;

            yield return new WaitForSeconds(0.5f);
        }

        if (DifficultyManager.Instance != null && UIManager.Instance != null)
        {
            int nextLevel = DifficultyManager.Instance.ActiveLevel + 1;
            string previewText = DifficultyManager.Instance.GetLevelPreviewText(nextLevel);

            UIManager.Instance.ShowLevelUpOverlay(nextLevel, previewText, () =>
            {
                DifficultyManager.Instance.SetLevel(nextLevel);
                UIManager.Instance.UpdateGoal(DifficultyManager.Instance.ActiveGoal);
                UIManager.Instance.UpdateLevel(DifficultyManager.Instance.ActiveLevel);
                
                feverChainProgress = 0;
                UIManager.Instance.UpdateFeverProgress(0f, FeverChainThreshold);

                RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
                if (rtc != null)
                {
                    rtc.ResetTimer();
                }

                CircularTimer ct = FindAnyObjectByType<CircularTimer>();
                if (ct != null)
                {
                    ct.ResetTimer();
                }

                ChangeState(GameState.Playing);
                isLevelingUp = false;
            });
        }
        else
        {
            ChangeState(GameState.Playing);
            isLevelingUp = false;
        }
    }

    public void RestartGame()
    {
        timeElapsed = 0f;
        ChangeState(GameState.Playing);
        BestCombo = 0;
        IsFeverActive = false;
        isLevelingUp = false;
        feverChainProgress = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetGameOverActive(false);
            UIManager.Instance.SetFeverActive(false);
            UIManager.Instance.UpdateTimer(timeElapsed);
            UIManager.Instance.UpdateFeverProgress(feverChainProgress, FeverChainThreshold);
        }

        RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
        if (rtc != null)
        {
            rtc.ResetTimer();
        }

        CircularTimer ct = FindAnyObjectByType<CircularTimer>();
        if (ct != null)
        {
            ct.ResetTimer();
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        if (DotSpawner.Instance != null)
        {
            DotSpawner.Instance.InitializeSpawner();
        }

        if (GameBrain.Instance != null && DifficultyManager.Instance != null)
        {
            GameBrain.Instance.SetCurrentLevel(DifficultyManager.Instance.ActiveLevel);
            if (GameBrain.Instance.CurrentLevelConfig != null && GameBrain.Instance.CurrentLevelConfig.isBossLevel)
            {
                BossHP = GameBrain.Instance.CurrentLevelConfig.bossHP;
            }
        }

        if (DifficultyManager.Instance != null && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoal(DifficultyManager.Instance.ActiveGoal);
            UIManager.Instance.UpdateLevel(DifficultyManager.Instance.ActiveLevel);
        }
    }

    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;

        if (GameBrain.Instance != null)
        {
            GameBrain.Instance.OnLevelEnded(false);
        }

        ChangeState(GameState.GameOver);
        DeactivateFeverMode();

        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        
        // Load encrypted persistency stats
        int savedHighScore = SaveSystem.LoadInt("HighScore", 0);
        int savedBestCombo = SaveSystem.LoadInt("BestCombo", 0);
        int playCount = SaveSystem.LoadInt("PlayCount", 0);
        float totalScores = SaveSystem.LoadFloat("TotalScores", 0f);

        bool isNewHighScore = finalScore > savedHighScore;
        if (isNewHighScore)
        {
            SaveSystem.SaveInt("HighScore", finalScore);
            savedHighScore = finalScore;
        }

        if (BestCombo > savedBestCombo)
        {
            SaveSystem.SaveInt("BestCombo", BestCombo);
            savedBestCombo = BestCombo;
        }

        // Track average scores
        playCount++;
        totalScores += finalScore;
        SaveSystem.SaveInt("PlayCount", playCount);
        SaveSystem.SaveFloat("TotalScores", totalScores);

        float averageScore = totalScores / playCount;
        int improvementPercentage = 0;
        if (averageScore > 0f)
        {
            float diff = (finalScore - averageScore) / averageScore * 100f;
            improvementPercentage = Mathf.RoundToInt(diff);
        }
        else
        {
            improvementPercentage = 0;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetGameOverActive(true, finalScore, BestCombo, isNewHighScore, improvementPercentage);
            UIManager.Instance.UpdateTimer(timeElapsed);
        }

        if (DotSpawner.Instance != null)
        {
            DotSpawner.Instance.ClearAll();
        }
    }

    public void DamageBoss()
    {
        if (GameBrain.Instance != null && GameBrain.Instance.CurrentLevelConfig != null && GameBrain.Instance.CurrentLevelConfig.isBossLevel)
        {
            BossHP--;
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.TriggerCameraShake(0.35f, 0.4f);
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowComboFeedback("BOSS HIT!", $"HP: {BossHP}", "", Color.red);
            }

            if (BossHP <= 0)
            {
                StartCoroutine(LevelUpCoroutine());
            }
        }
    }
}
