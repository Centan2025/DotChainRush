using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChaosBossPhase
{
    Observation,
    ChaosExpansion,
    RealityCollapse
}

public class ChaosOrbBossSystem : MonoBehaviour
{
    public static ChaosOrbBossSystem Instance { get; private set; }

    public ChaosBossPhase CurrentPhase { get; private set; } = ChaosBossPhase.Observation;
    public float ChaosStability { get; private set; } = 100f; // 100% to 0%
    public float ArenaIntegrity { get; private set; } = 100f; // 100% to 0%
    public int SuccessfulCombos { get; private set; } = 0;
    public float TimeSpeedMultiplier { get; private set; } = 1.0f;
    public bool IsActive { get; private set; } = false;
    public bool HasSurvivedFinalChaosPhase { get; private set; } = false;

    private bool hasStabilityDecreased = false;

    [Header("Phase 1 - Observation")]
    private float colorDistortionTimer = 0f;
    private float colorDistortionCooldown = 10f;
    private float fakeBallSpawnChance = 0.10f;

    [Header("Phase 2 - Chaos Expansion")]
    private float timeDistortionTimer = 0f;
    private float timeDistortionInterval = 20f;

    [Header("Phase 3 - Reality Collapse")]
    private float finalAttackTimer = 0f;
    private float finalAttackInterval = 8f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeBoss()
    {
        IsActive = true;
        ChaosStability = 100f;
        ArenaIntegrity = 100f;
        SuccessfulCombos = 0;
        TimeSpeedMultiplier = 1.0f;
        CurrentPhase = ChaosBossPhase.Observation;
        hasStabilityDecreased = false;
        HasSurvivedFinalChaosPhase = false;
        
        colorDistortionTimer = 0f;
        timeDistortionTimer = 0f;
        finalAttackTimer = 0f;

        // Play Boss Intro audio
        AudioManager.Instance?.PlayMilestoneSound(1); // Placeholder for intro sound

        Debug.Log("[ChaosOrbBoss] Phase 1: Observation started. Stability: " + ChaosStability);
    }

    private void Update()
    {
        if (!IsActive || GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        UpdatePhases();

        // Passive stability recovery
        if (hasStabilityDecreased)
        {
            ChaosStability = Mathf.Min(100f, ChaosStability + Time.deltaTime * 0.5f);
            if (ChaosStability >= 100f)
            {
                Debug.Log("[ChaosOrbBoss] Stability reached 100%! Defeat triggered.");
                TriggerDefeat();
                return;
            }
        }

        // Arena Collapse tracking
        float collapseRate = 0f;
        if (CurrentPhase == ChaosBossPhase.ChaosExpansion) collapseRate = 0.5f;
        else if (CurrentPhase == ChaosBossPhase.RealityCollapse) collapseRate = 2.0f;

        if (collapseRate > 0f)
        {
            ArenaIntegrity = Mathf.Max(0f, ArenaIntegrity - Time.deltaTime * collapseRate);
            if (ArenaIntegrity <= 0f)
            {
                Debug.Log("[ChaosOrbBoss] Arena Collapsed! Defeat triggered.");
                TriggerDefeat();
                return;
            }
        }

        if (BossDimensionUI.Instance != null)
        {
            BossDimensionUI.Instance.UpdateIntegrityBar(ArenaIntegrity);
            BossDimensionUI.Instance.UpdateStabilityBar(ChaosStability);
            BossDimensionUI.Instance.UpdatePhaseIndicator(CurrentPhase);
        }

        if (CurrentPhase == ChaosBossPhase.Observation || CurrentPhase == ChaosBossPhase.ChaosExpansion || CurrentPhase == ChaosBossPhase.RealityCollapse)
        {
            UpdateColorDistortion();
        }

        if (CurrentPhase == ChaosBossPhase.ChaosExpansion || CurrentPhase == ChaosBossPhase.RealityCollapse)
        {
            UpdateTimeDistortion();
        }

        if (CurrentPhase == ChaosBossPhase.RealityCollapse)
        {
            UpdateFinalAttack();
        }
    }

    private void UpdatePhases()
    {
        if (ChaosStability <= 70f && ChaosStability > 30f && CurrentPhase != ChaosBossPhase.ChaosExpansion)
        {
            CurrentPhase = ChaosBossPhase.ChaosExpansion;
            UIManager.Instance?.ShowComboFeedback("PHASE 2: CHAOS EXPANSION", Color.magenta);
            ComboManager.Instance?.TriggerCameraShake(0.5f, 0.2f);
            Level10ObstacleDirector.Instance?.SetPhase(2);
            AudioManager.Instance?.PlayMilestoneSound(2); // Phase change sound hook
            Debug.Log("[ChaosOrbBoss] Phase 2: Chaos Expansion started.");
        }
        else if (ChaosStability <= 30f && CurrentPhase != ChaosBossPhase.RealityCollapse)
        {
            CurrentPhase = ChaosBossPhase.RealityCollapse;
            HasSurvivedFinalChaosPhase = true;
            UIManager.Instance?.ShowComboFeedback("PHASE 3: REALITY COLLAPSE!", Color.red);
            ComboManager.Instance?.TriggerCameraShake(0.8f, 0.3f);
            Level10ObstacleDirector.Instance?.SetPhase(3);
            Physics2D.gravity = new Vector2(0, -12f); // Gravity Shift & Speed Increase
            UIManager.Instance?.TriggerScreenFlash(Color.red, 0.8f);
            AudioManager.Instance?.PlayMilestoneSound(2); // Phase change sound hook
            Debug.Log("[ChaosOrbBoss] Phase 3: Reality Collapse started.");
        }
    }

    private void UpdateColorDistortion()
    {
        colorDistortionTimer += Time.deltaTime;
        if (colorDistortionTimer >= colorDistortionCooldown)
        {
            colorDistortionTimer = 0f;
            TriggerColorDistortion();
        }
    }

    private void TriggerColorDistortion()
    {
        if (DotSpawner.Instance == null) return;

        UIManager.Instance?.ShowComboFeedback("COLOR DISTORTION!", Color.yellow);
        AudioManager.Instance?.PlayMilestoneSound(4); // Ability sound trigger
        foreach (Dot dot in DotSpawner.Instance.ActiveDots)
        {
            if (dot != null && Random.value < 0.3f && BalanceDB.IsNormalColor((TopTipi)dot.Type))
            {
                SpriteRenderer sr = dot.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    int randomColorId = Random.Range(0, 5);
                    sr.color = ColorManager.Instance != null ? ColorManager.Instance.GetColor(randomColorId) : Color.white;
                }
            }
        }
    }

    private void UpdateTimeDistortion()
    {
        timeDistortionTimer += Time.deltaTime;
        if (timeDistortionTimer >= timeDistortionInterval)
        {
            timeDistortionTimer = 0f;
            TriggerTimeDistortion();
        }
    }

    private void TriggerTimeDistortion()
    {
        TimeSpeedMultiplier *= 1.3f;
        UIManager.Instance?.ShowComboFeedback($"TIME SPEED x{TimeSpeedMultiplier:F1}!", Color.cyan);
        AudioManager.Instance?.PlayMilestoneSound(4);
    }

    private void UpdateFinalAttack()
    {
        finalAttackTimer += Time.deltaTime;
        if (finalAttackTimer >= finalAttackInterval)
        {
            finalAttackTimer = 0f;
            TriggerFinalChaosAttack();
        }
    }

    private void TriggerFinalChaosAttack()
    {
        int attackChoice = Random.Range(0, 4);
        AudioManager.Instance?.PlayMilestoneSound(4); // Attack alert sound
        switch (attackChoice)
        {
            case 0:
                // Gravity Flip
                Physics2D.gravity = new Vector2(0, 9.81f);
                UIManager.Instance?.ShowComboFeedback("GRAVITY FLIP!", Color.red);
                StartCoroutine(ResetGravityDelayed(3f));
                break;
            case 1:
                // Ball Rain
                if (DotSpawner.Instance != null)
                {
                    UIManager.Instance?.ShowComboFeedback("BALL RAIN!", Color.blue);
                    for (int i = 0; i < 5; i++)
                    {
                        DotSpawner.Instance.SpawnDot(new Vector3(Random.Range(-2f, 2f), DotSpawner.Instance.SpawnY, 0));
                    }
                }
                break;
            case 2:
                // Color Shuffle
                TriggerColorDistortion();
                UIManager.Instance?.ShowComboFeedback("COLOR SHUFFLE!", Color.yellow);
                break;
            case 3:
                // Time Drain
                UIManager.Instance?.ShowComboFeedback("TIME DRAIN!", Color.magenta);
                RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
                if (rtc != null) rtc.CurrentTime -= 4f;
                break;
        }
        ComboManager.Instance?.TriggerCameraShake(0.4f, 0.15f);
    }

    private IEnumerator ResetGravityDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (CurrentPhase == ChaosBossPhase.RealityCollapse)
        {
            Physics2D.gravity = new Vector2(0, -12f);
        }
        else
        {
            Physics2D.gravity = new Vector2(0, -9.81f);
        }
    }

    public void OnDotPopped(Dot dot, int chainLength)
    {
        if (!IsActive) return;

        // Counter: Time-related balls reset the speed multiplier
        if (dot.Type == (DotType)TopTipi.Zaman || dot.Type == (DotType)TopTipi.ZamanBukucu || dot.Type == (DotType)TopTipi.SonsuzZaman)
        {
            TimeSpeedMultiplier = 1.0f;
            UIManager.Instance?.ShowComboFeedback("TIME STABILIZED!", Color.green);
        }

        if (chainLength >= 3)
        {
            SuccessfulCombos++;
        }

        hasStabilityDecreased = true;

        if (dot.Type == (DotType)TopTipi.Gokkusagi || !BalanceDB.IsNormalColor((TopTipi)dot.Type))
        {
            ReduceStability(chainLength * 0.5f, "Rainbow/Special");
        }
        else
        {
            ReduceStability(chainLength * 0.2f, "Normal Chain");
        }
    }

    private void ReduceStability(float amount, string damageSource)
    {
        ChaosStability = Mathf.Max(0f, ChaosStability - amount);
        TelemetrySystem.RecordBossDamageSource(damageSource);

        if (BossDimensionUI.Instance != null)
        {
            BossDimensionUI.Instance.UpdateStabilityBar(ChaosStability);
            BossDimensionUI.Instance.UpdatePhaseIndicator(CurrentPhase);
        }
        
        if (ChaosStability <= 0)
        {
            DefeatBoss();
        }
    }

    private void TriggerDefeat()
    {
        IsActive = false;
        Physics2D.gravity = new Vector2(0, -9.81f);
        AudioManager.Instance?.PlayMilestoneSound(5); // Defeat chord
        GameManager.Instance?.GameOver();
    }

    private void DefeatBoss()
    {
        IsActive = false;
        Physics2D.gravity = new Vector2(0, -9.81f);
        UIManager.Instance?.ShowComboFeedback("CHAOS ORB DEFEATED!", Color.green);
        AudioManager.Instance?.PlayMilestoneSound(3); // Victory chord
        
        float timeTaken = 0f;
        RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
        if (rtc != null) timeTaken = 90f - rtc.CurrentTime;
        
        TelemetrySystem.RecordBossResult(true, timeTaken, GameManager.Instance != null ? GameManager.Instance.BestCombo : 0);
    }

    public bool ShouldSpawnFakeBall()
    {
        if (!IsActive) return false;
        if (CurrentPhase == ChaosBossPhase.Observation || CurrentPhase == ChaosBossPhase.RealityCollapse)
        {
            return Random.value < fakeBallSpawnChance;
        }
        return false;
    }
}
