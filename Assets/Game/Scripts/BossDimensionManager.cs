using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossAdaptation
{
    public string preferredPlayerStrategy = "Standard";
    public string counterAbility = "None";
    public int evolutionLevel = 1;

    public void AnalyzePlayerStrategy()
    {
        int bombPops = 0;
        int rainbowPops = 0;

        if (TelemetrySystem.pops.ContainsKey(TopTipi.Bomba)) bombPops += TelemetrySystem.pops[TopTipi.Bomba];
        if (TelemetrySystem.pops.ContainsKey(TopTipi.Ates)) bombPops += TelemetrySystem.pops[TopTipi.Ates];
        if (TelemetrySystem.pops.ContainsKey(TopTipi.Nukleer)) bombPops += TelemetrySystem.pops[TopTipi.Nukleer];

        if (TelemetrySystem.pops.ContainsKey(TopTipi.Gokkusagi)) rainbowPops += TelemetrySystem.pops[TopTipi.Gokkusagi];
        if (TelemetrySystem.pops.ContainsKey(TopTipi.VoidRainbow)) rainbowPops += TelemetrySystem.pops[TopTipi.VoidRainbow];

        int totalPops = 0;
        foreach (var count in TelemetrySystem.pops.Values)
        {
            totalPops += count;
        }

        if (bombPops > rainbowPops && bombPops > totalPops * 0.15f)
        {
            preferredPlayerStrategy = "Bomb Heavy";
            counterAbility = "BombShield";
        }
        else if (rainbowPops > bombPops && rainbowPops > totalPops * 0.15f)
        {
            preferredPlayerStrategy = "Rainbow Heavy";
            counterAbility = "RainbowDampener";
        }
        else if (TelemetrySystem.levelsPlayed > 0 && TelemetrySystem.failRate < 0.1f)
        {
            preferredPlayerStrategy = "High Speed";
            counterAbility = "ChainLimiter";
        }
        else
        {
            preferredPlayerStrategy = "Standard";
            counterAbility = "None";
        }

        evolutionLevel = Mathf.Min(3, 1 + TelemetrySystem.levelsPlayed / 10);
    }
}

public class BossDimensionManager : MonoBehaviour
{
    public static BossDimensionManager Instance { get; private set; }

    public bool IsBossLevelActive { get; private set; }
    public TopTipi ActiveBossType { get; private set; }
    public int ActiveLevelNumber { get; private set; }
    public float CurrentStability { get; private set; } = 1.0f;
    public BossAdaptation Adaptation { get; private set; } = new BossAdaptation();

    [Header("Chaos Core Visuals")]
    private GameObject chaosCoreInstance;
    private SpriteRenderer chaosCoreSR;
    private int chaosCoreColorId = 0;
    private float chaosColorTimer = 0f;

    [Header("Boss 2 Elements")]
    public string ActiveElement { get; private set; } = "None";
    private float elementTimer = 0f;
    private int elementalProgress = 0;

    [Header("Boss 3 Flow Master")]
    private int flowDisruptions = 0;
    private float flowDirection = 1f;

    [Header("Boss 5 Void Entity")]
    private int voidPops = 0;

    [Header("Boss 7 Gravity Core")]
    private int gravityProgress = 0;
    private float gravityShiftTimer = 0f;
    private Vector2 currentGravityDirection = Vector2.down;

    [Header("Boss 8 Virus Mind")]
    private int virusDestroyCount = 0;

    [Header("Boss 6 & Glitch rules")]
    public string GlitchRule { get; private set; } = "None";
    private float glitchRuleTimer = 0f;
    private string bossWarning = "";

    [Header("System Phase")]
    public int SystemPhase { get; private set; } = 1;

    private float timerLordTick = 0f;

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

    public void InitializeBossLevel(int level, TopTipi type)
    {
        IsBossLevelActive = true;
        ActiveLevelNumber = level;
        ActiveBossType = type;
        CurrentStability = 1.0f;

        Adaptation.AnalyzePlayerStrategy();

        // Reset variables
        elementalProgress = 0;
        flowDisruptions = 0;
        voidPops = 0;
        gravityProgress = 0;
        virusDestroyCount = 0;
        SystemPhase = 1;
        GlitchRule = "None";
        ActiveElement = "None";
        flowDirection = 1f;
        currentGravityDirection = Vector2.down;
        Physics2D.gravity = new Vector2(0, -9.81f);

        CleanupChaosCore();

        if (type == TopTipi.KaosOrbBoss1)
        {
            if (ChaosOrbBossSystem.Instance == null)
            {
                gameObject.AddComponent<ChaosOrbBossSystem>();
            }
            ChaosOrbBossSystem.Instance.InitializeBoss();
            SetupChaosCore();
        }
        else if (type == TopTipi.TheVoidFinalBoss && SystemPhase == 2)
        {
            SetupChaosCore();
        }

        if (type == TopTipi.ElementalFuryBoss2)
        {
            RotateElement();
        }

        if (type == TopTipi.Glitch)
        {
            RotateGlitchRule();
        }

        Debug.Log($"[BossDimensionManager] Initialized {type} at Level {level}. Strategy: {Adaptation.preferredPlayerStrategy}, Counter: {Adaptation.counterAbility}");
    }

    private void Update()
    {
        if (!IsBossLevelActive || GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // Boss 1: Chaos Orb Color Rotations and Gravity Pull
        if (ActiveBossType == TopTipi.KaosOrbBoss1 && chaosCoreInstance != null)
        {
            UpdateChaosCore();
        }

        // Boss 2: Elemental Fury rotation
        if (ActiveBossType == TopTipi.ElementalFuryBoss2)
        {
            UpdateElementalFury();
        }

        // Boss 3: Flow Master Horizonal Lane movement
        if (ActiveBossType == TopTipi.FlowMasterBoss4)
        {
            UpdateFlowMaster();
        }

        // Boss 4: Time Lord Stealing Time
        if (ActiveBossType == TopTipi.ZamanLorduBoss)
        {
            UpdateTimeLord();
        }

        // Boss 5: Void Entity Invisible Dots near cursor
        if (ActiveBossType == TopTipi.ChaosIncarnateBoss7 || ActiveBossType == TopTipi.Gorunmez)
        {
            UpdateVoidEntity();
        }

        // Boss 6: Glitch God rules
        if (ActiveBossType == TopTipi.Glitch)
        {
            UpdateGlitchGod();
        }

        // Boss 7: Gravity Core shift
        if (ActiveBossType == TopTipi.PrestigeBoss || ActiveBossType == TopTipi.GravityCore)
        {
            UpdateGravityCore();
        }

        // Boss 10: Final Boss - The System
        if (ActiveBossType == TopTipi.TheVoidFinalBoss)
        {
            UpdateFinalSystem();
        }
    }

    // --- Boss 1: Chaos Core ---
    private void SetupChaosCore()
    {
        chaosCoreInstance = new GameObject("ChaosCoreVisual");
        chaosCoreInstance.transform.position = Vector3.zero;
        chaosCoreSR = chaosCoreInstance.AddComponent<SpriteRenderer>();
        chaosCoreSR.sprite = GetCircleSprite();
        chaosCoreSR.sortingOrder = -1; // Under the balls
        chaosCoreInstance.transform.localScale = Vector3.one * 1.8f;

        // Give it a neon glow outline / shadow
        GameObject shadow = new GameObject("ChaosShadow");
        shadow.transform.SetParent(chaosCoreInstance.transform, false);
        shadow.transform.localPosition = Vector3.zero;
        shadow.transform.localScale = Vector3.one * 1.25f;
        SpriteRenderer shadowSR = shadow.AddComponent<SpriteRenderer>();
        shadowSR.sprite = GetCircleSprite();
        shadowSR.color = new Color(0.7f, 0f, 1f, 0.35f);
        shadowSR.sortingOrder = -2; // Below core
 
        chaosCoreColorId = UnityEngine.Random.Range(0, 5);
        chaosCoreSR.color = ColorManager.Instance != null ? ColorManager.Instance.GetColor(chaosCoreColorId) : Color.white;
        chaosColorTimer = 0f;
    }

    private void UpdateChaosCore()
    {
        chaosColorTimer += Time.deltaTime;
        if (chaosColorTimer >= 4f)
        {
            chaosColorTimer = 0f;
            chaosCoreColorId = UnityEngine.Random.Range(0, 5);
            Color nextColor = ColorManager.Instance != null ? ColorManager.Instance.GetColor(chaosCoreColorId) : Color.white;
            if (chaosCoreSR != null) chaosCoreSR.color = nextColor;
        }

        // Pulse scale
        float pulse = 1.8f + 0.15f * Mathf.Sin(Time.time * 5f);
        if (chaosCoreInstance != null) chaosCoreInstance.transform.localScale = Vector3.one * pulse;

        // Pull nearby dots towards center (where the Chaos Core is positioned)
        Vector3 corePos = Vector3.zero;
        if (DotSpawner.Instance != null)
        {
            foreach (Dot d in DotSpawner.Instance.ActiveDots)
            {
                if (d == null) continue;
                float dist = Vector2.Distance(corePos, d.transform.position);
                if (dist < 3.0f && dist > 0.4f)
                {
                    Rigidbody2D rb = d.GetComponent<Rigidbody2D>();
                    if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                    {
                        Vector2 forceDir = (corePos - d.transform.position).normalized;
                        rb.AddForce(forceDir * (5f / dist)); // strong pull when closer
                    }
                }
            }
        }
    }

    // --- Boss 2: Elemental Fury ---
    private void RotateElement()
    {
        string[] elements = { "FIRE", "WATER", "EARTH", "NATURE" };
        ActiveElement = elements[UnityEngine.Random.Range(0, elements.Length)];
        elementTimer = 0f;
        UIManager.Instance?.ShowComboFeedback($"BOSS ELEMENT: {ActiveElement}!", Color.cyan);
    }

    private void UpdateElementalFury()
    {
        elementTimer += Time.deltaTime;
        if (elementTimer >= 10f)
        {
            RotateElement();
        }
    }

    // --- Boss 3: Flow Master ---
    private void UpdateFlowMaster()
    {
        // Spawner lanes wrapping
        if (DotSpawner.Instance != null)
        {
            foreach (Dot d in DotSpawner.Instance.ActiveDots)
            {
                if (d == null) continue;
                Rigidbody2D rb = d.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Ensure kinematic movement
                    if (rb.bodyType != RigidbodyType2D.Kinematic)
                    {
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        rb.gravityScale = 0f;
                        float targetY = Mathf.Round(d.transform.position.y / 2f) * 2f;
                        d.transform.position = new Vector3(d.transform.position.x, Mathf.Clamp(targetY, -2f, 2f), 0f);
                    }

                    // Move horizontally
                    rb.linearVelocity = new Vector2(1.8f * flowDirection, 0f);

                    // Wrapping
                    if (flowDirection > 0f && d.transform.position.x > 2.5f)
                    {
                        d.transform.position = new Vector3(-2.5f, d.transform.position.y, 0f);
                    }
                    else if (flowDirection < 0f && d.transform.position.x < -2.5f)
                    {
                        d.transform.position = new Vector3(2.5f, d.transform.position.y, 0f);
                    }
                }
            }
        }
    }

    // --- Boss 4: Time Lord ---
    private void UpdateTimeLord()
    {
        timerLordTick += Time.deltaTime;
        if (timerLordTick >= 10f)
        {
            timerLordTick = 0f;
            RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
            if (rtc != null)
            {
                rtc.CurrentTime -= 5f;
                UIManager.Instance?.ShowComboFeedback("TIME STEAL! -5s", Color.red);
                UIManager.Instance?.TriggerScreenFlash(Color.red, 0.3f);
            }
        }
    }

    // --- Boss 5: Void Entity ---
    private void UpdateVoidEntity()
    {
        if (Camera.main == null) return;
        // Darken screen background if not set
        Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, new Color(0.02f, 0.01f, 0.05f), Time.deltaTime * 3f);

        if (UnityEngine.InputSystem.Mouse.current == null) return;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        if (DotSpawner.Instance != null)
        {
            foreach (Dot d in DotSpawner.Instance.ActiveDots)
            {
                if (d == null) continue;
                float dist = Vector2.Distance(mouseWorld, d.transform.position);

                SpriteRenderer mainSR = d.GetComponent<SpriteRenderer>();
                Transform vc = d.transform.Find("VisualCore");
                SpriteRenderer vcSR = vc != null ? vc.GetComponent<SpriteRenderer>() : null;

                float targetAlpha = (dist < 1.4f) ? 1.0f : 0.05f;

                if (mainSR != null)
                {
                    Color c = mainSR.color;
                    mainSR.color = new Color(c.r, c.g, c.b, targetAlpha);
                }
                if (vcSR != null)
                {
                    Color c = vcSR.color;
                    vcSR.color = new Color(c.r, c.g, c.b, targetAlpha);
                }
            }
        }
    }

    // --- Boss 6: Glitch God ---
    private void RotateGlitchRule()
    {
        string[] rules = { "RED = BLUE", "GREEN = YELLOW", "ALL SELECT" };
        GlitchRule = rules[UnityEngine.Random.Range(0, rules.Length)];
        glitchRuleTimer = 0f;
        UIManager.Instance?.ShowComboFeedback($"GLITCH LAW: {GlitchRule}!", Color.magenta);
        CameraShake(0.3f);
    }

    private void UpdateGlitchGod()
    {
        glitchRuleTimer += Time.deltaTime;
        if (glitchRuleTimer >= 7f)
        {
            RotateGlitchRule();
        }

        if (Camera.main == null) return;
        // Random RGB Split Offset Simulation
        if (UnityEngine.Random.value < 0.15f)
        {
            Camera.main.transform.position = new Vector3(
                UnityEngine.Random.Range(-0.06f, 0.06f),
                UnityEngine.Random.Range(-0.06f, 0.06f),
                -10f
            );
        }
        else
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(0, 0, -10f), Time.deltaTime * 10f);
        }
    }

    // --- Boss 7: Gravity Core ---
    private void UpdateGravityCore()
    {
        gravityShiftTimer += Time.deltaTime;
        if (gravityShiftTimer >= 8f)
        {
            gravityShiftTimer = 0f;
            int rot = UnityEngine.Random.Range(0, 4);
            switch (rot)
            {
                case 0:
                    currentGravityDirection = Vector2.down;
                    bossWarning = "GRAVITY: DOWN";
                    break;
                case 1:
                    currentGravityDirection = Vector2.right;
                    bossWarning = "GRAVITY: RIGHT";
                    break;
                case 2:
                    currentGravityDirection = Vector2.up;
                    bossWarning = "GRAVITY: UP";
                    break;
                case 3:
                    currentGravityDirection = Vector2.left;
                    bossWarning = "GRAVITY: LEFT";
                    break;
            }

            Physics2D.gravity = currentGravityDirection * 9.81f * 0.7f;
            UIManager.Instance?.ShowComboFeedback(bossWarning, Color.yellow);
            CameraShake(0.2f);
        }

        // Force update on active dots gravity
        if (DotSpawner.Instance != null)
        {
            foreach (Dot d in DotSpawner.Instance.ActiveDots)
            {
                if (d == null) continue;
                Rigidbody2D rb = d.GetComponent<Rigidbody2D>();
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.gravityScale = 0.5f; // keep scaling uniform for rotation
                }
            }
        }
    }

    // --- Boss 10: The System (Final Boss) ---
    private void UpdateFinalSystem()
    {
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        int lastPhase = SystemPhase;

        if (score >= 15000) SystemPhase = 4; // Void
        else if (score >= 10000) SystemPhase = 3; // Virus
        else if (score >= 5000) SystemPhase = 2; // Chaos
        else SystemPhase = 1; // Gravity

        if (SystemPhase != lastPhase)
        {
            UIManager.Instance?.ShowComboFeedback($"SYSTEM PHASE {SystemPhase} ONLINE!", Color.red);
            UIManager.Instance?.TriggerScreenFlash(Color.red, 0.5f);
            CameraShake(0.4f);

            CleanupChaosCore();
            Physics2D.gravity = new Vector2(0, -9.81f);

            if (SystemPhase == 2)
            {
                SetupChaosCore();
            }
        }

        // Execute phase specifics
        switch (SystemPhase)
        {
            case 1:
                UpdateGravityCore();
                break;
            case 2:
                UpdateChaosCore();
                break;
            case 4:
                UpdateVoidEntity();
                break;
        }
    }

    // --- Chain Matching Callbacks ---
    public void OnChainProcessed(List<Dot> chain)
    {
        if (!IsBossLevelActive || chain == null) return;

        int length = chain.Count;

        // Boss 1: Chaos Orb match color core
        if (ActiveBossType == TopTipi.KaosOrbBoss1)
        {
            if (ChaosOrbBossSystem.Instance != null && ChaosOrbBossSystem.Instance.IsActive)
            {
                // Let the new ChaosOrbBossSystem handle stability hits
                ChaosOrbBossSystem.Instance.OnDotPopped(chain[0], length);
            }
        }
        else if (ActiveBossType == TopTipi.TheVoidFinalBoss && SystemPhase == 2)
        {
            if (length >= 5)
            {
                // Verify if color matches core color
                bool matchedColor = false;
                foreach (Dot d in chain)
                {
                    if (d.ColorId == chaosCoreColorId)
                    {
                        matchedColor = true;
                        break;
                    }
                }

                if (matchedColor)
                {
                    CurrentStability = Mathf.Max(0f, CurrentStability - 0.2f);
                    UIManager.Instance?.ShowComboFeedback("CORE WEAKENED!", Color.green);
                    CameraShake(0.25f);
                }
            }
        }

        // Boss 2: Elemental Fury pop counter
        if (ActiveBossType == TopTipi.ElementalFuryBoss2)
        {
            int counterCount = 0;
            int hazardCount = 0;

            foreach (Dot d in chain)
            {
                if (ActiveElement == "FIRE")
                {
                    if (d.Type == DotType.Su) counterCount++;
                    if (d.Type == DotType.Ates || d.Type == DotType.Bomba) hazardCount++;
                }
                else if (ActiveElement == "WATER")
                {
                    if (d.Type == DotType.Doga) counterCount++;
                    if (d.Type == DotType.Su) hazardCount++;
                }
                else if (ActiveElement == "NATURE")
                {
                    if (d.Type == DotType.Ates) counterCount++;
                    if (d.Type == DotType.Doga) hazardCount++;
                }
                else if (ActiveElement == "EARTH")
                {
                    if (d.Type == DotType.Elektrik) counterCount++;
                    if (d.Type == DotType.Toprak) hazardCount++;
                }
            }

            if (counterCount > 0)
            {
                elementalProgress += counterCount;
                UIManager.Instance?.ShowComboFeedback($"COUNTER! +{counterCount} Progress", Color.green);
            }
            if (hazardCount > 0)
            {
                RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
                if (rtc != null) rtc.CurrentTime -= hazardCount * 3f;
                UIManager.Instance?.ShowComboFeedback($"HAZARD! -{hazardCount * 3}s", Color.red);
                UIManager.Instance?.TriggerScreenFlash(Color.red, 0.2f);
            }
        }

        // Boss 3: Flow Master disrupt
        if (ActiveBossType == TopTipi.FlowMasterBoss4)
        {
            bool hasDisrupt = false;
            foreach (Dot d in chain)
            {
                if (d.Type == DotType.Magnet || d.Type == DotType.Ayna || d.Type == DotType.Teleport)
                {
                    hasDisrupt = true;
                    if (d.Type == DotType.Magnet)
                    {
                        // Stop all movements in flow
                        flowDirection = 0f;
                        StartCoroutine(ResumeFlowDelayed(2.5f));
                    }
                    if (d.Type == DotType.Ayna)
                    {
                        flowDirection *= -1f;
                    }
                    if (d.Type == DotType.Teleport)
                    {
                        // Randomize lanes of all active dots
                        foreach (Dot activeD in DotSpawner.Instance.ActiveDots)
                        {
                            if (activeD != null)
                            {
                                activeD.transform.position = new Vector3(
                                    UnityEngine.Random.Range(-2.2f, 2.2f),
                                    UnityEngine.Random.Range(-2f, 2f),
                                    0f
                                );
                            }
                        }
                    }
                    break;
                }
            }

            if (hasDisrupt)
            {
                flowDisruptions++;
                UIManager.Instance?.ShowComboFeedback($"FLOW BROKEN ({flowDisruptions}/3)!", Color.magenta);
                CameraShake(0.3f);
            }
        }

        // Boss 5: Void Entity
        if (ActiveBossType == TopTipi.ChaosIncarnateBoss7 || ActiveBossType == TopTipi.Gorunmez || (ActiveBossType == TopTipi.TheVoidFinalBoss && SystemPhase == 4))
        {
            voidPops += length;
            UIManager.Instance?.ShowComboFeedback($"VOID POPS: {voidPops}/10", Color.cyan);
        }

        // Boss 7: Gravity Core matching target colors
        if (ActiveBossType == TopTipi.PrestigeBoss || ActiveBossType == TopTipi.GravityCore || (ActiveBossType == TopTipi.TheVoidFinalBoss && SystemPhase == 1))
        {
            gravityProgress += length;
            UIManager.Instance?.ShowComboFeedback($"GRAVITY CLEAR: {gravityProgress}/40", Color.cyan);
        }

        // Boss 8: Virus Mind duplication
        if (ActiveBossType == TopTipi.Virus || ActiveBossType == TopTipi.Omega || (ActiveBossType == TopTipi.TheVoidFinalBoss && SystemPhase == 3))
        {
            bool hasVirusInChain = false;
            foreach (Dot d in chain)
            {
                if (d.Type == DotType.Virus)
                {
                    hasVirusInChain = true;
                    virusDestroyCount++;
                }
            }

            // Duplication check: if we cleared normal dots and left virus untouched
            if (!hasVirusInChain)
            {
                int virusToSpawn = UnityEngine.Random.Range(1, 3);
                int spawned = 0;
                List<Dot> candidates = new List<Dot>(DotSpawner.Instance.ActiveDots);
                foreach (Dot d in candidates)
                {
                    if (d != null && d.Type != DotType.Virus && !d.IsBossDot && !d.IsObstacle)
                    {
                        d.Init(-1, DotType.Virus, null);
                        spawned++;
                        if (spawned >= virusToSpawn) break;
                    }
                }

                if (spawned > 0)
                {
                    UIManager.Instance?.ShowComboFeedback("VIRUS MULTIPLIED!", Color.red);
                }
            }
        }
    }

    private IEnumerator ResumeFlowDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (flowDirection == 0f)
        {
            flowDirection = 1f;
        }
    }

    // --- Override Spawner Spawning Positions & Rules ---
    public bool OverrideSpawn(Dot dot)
    {
        if (!IsBossLevelActive || dot == null) return false;

        // Boss 3 (Flow Master): spawn at left border in three conveyor lanes
        if (ActiveBossType == TopTipi.FlowMasterBoss4)
        {
            float[] lanes = { -2f, 0f, 2f };
            float targetY = lanes[UnityEngine.Random.Range(0, lanes.Length)];
            dot.transform.position = new Vector3(-2.4f, targetY, 0f);

            Rigidbody2D rb = dot.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(1.8f * flowDirection, 0f);
            }
            return true;
        }

        // Boss 8: Spawning virus dots 25% of the time
        if (ActiveBossType == TopTipi.Virus || (ActiveBossType == TopTipi.TheVoidFinalBoss && SystemPhase == 3))
        {
            if (UnityEngine.Random.value < 0.25f)
            {
                dot.Init(-1, DotType.Virus, null);
            }
        }

        return false;
    }

    // --- Win Conditions ---
    public bool CheckBossWinCondition()
    {
        if (!IsBossLevelActive) return false;

        // Final Boss check
        if (ActiveBossType == TopTipi.TheVoidFinalBoss)
        {
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            return score >= 20000;
        }

        switch (ActiveBossType)
        {
            case TopTipi.KaosOrbBoss1:
                return ChaosOrbBossSystem.Instance != null && 
                       ChaosOrbBossSystem.Instance.ChaosStability <= 0f &&
                       ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= 20800 &&
                       ChaosOrbBossSystem.Instance.SuccessfulCombos >= 5 &&
                       ChaosOrbBossSystem.Instance.HasSurvivedFinalChaosPhase;

            case TopTipi.ElementalFuryBoss2:
                return elementalProgress >= 12;

            case TopTipi.FlowMasterBoss4:
                return flowDisruptions >= 3;

            case TopTipi.ZamanLorduBoss:
                return ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= 15000;

            case TopTipi.ChaosIncarnateBoss7:
            case TopTipi.Gorunmez:
                return voidPops >= 10;

            case TopTipi.Glitch:
                return ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= 20000;

            case TopTipi.PrestigeBoss:
            case TopTipi.GravityCore:
                return gravityProgress >= 40;

            case TopTipi.Virus:
                // Check if all virus dots are cleared from the playfield
                if (DotSpawner.Instance != null)
                {
                    bool virusFound = false;
                    foreach (Dot d in DotSpawner.Instance.ActiveDots)
                    {
                        if (d != null && d.Type == DotType.Virus)
                        {
                            virusFound = true;
                            break;
                        }
                    }
                    return !virusFound && virusDestroyCount > 0;
                }
                return false;

            case TopTipi.Omega:
                return ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= 25000;

            default:
                return ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= 15000;
        }
    }

    public string GetBossWarning()
    {
        if (!IsBossLevelActive) return "";

        if (ActiveBossType == TopTipi.TheVoidFinalBoss)
        {
            return $"SYSTEM SHIELD: Phase {SystemPhase}/4 Active";
        }

        if (Adaptation.counterAbility != "None" && Adaptation.counterAbility != "")
        {
            return $"BOSS COUNTER: {Adaptation.counterAbility.ToUpper()} ACTIVE!";
        }

        switch (ActiveBossType)
        {
            case TopTipi.KaosOrbBoss1:
                return $"CORE COLOR: {(chaosCoreColorId == 0 ? "RED" : chaosCoreColorId == 1 ? "BLUE" : chaosCoreColorId == 2 ? "GREEN" : chaosCoreColorId == 3 ? "YELLOW" : "PURPLE")}";
            case TopTipi.ElementalFuryBoss2:
                return $"ELEMENT: {ActiveElement}";
            case TopTipi.FlowMasterBoss4:
                return $"FLOW BROKEN: {flowDisruptions}/3";
            case TopTipi.Glitch:
                return $"GLITCH RULE: {GlitchRule}";
            case TopTipi.PrestigeBoss:
            case TopTipi.GravityCore:
                return bossWarning;
            case TopTipi.Virus:
                return "INFECTIOUS VIRUS ACTIVE!";
            default:
                return "";
        }
    }

    public string GetBossHUDValue()
    {
        if (!IsBossLevelActive) return "";

        switch (ActiveBossType)
        {
            case TopTipi.KaosOrbBoss1:
                if (ChaosOrbBossSystem.Instance != null)
                    return $"STABILITY: {Mathf.RoundToInt(ChaosOrbBossSystem.Instance.ChaosStability)}%";
                return $"STABILITY: {Mathf.RoundToInt(CurrentStability * 100)}%";
            case TopTipi.ElementalFuryBoss2:
                return $"COUNTERS: {elementalProgress}/12";
            case TopTipi.FlowMasterBoss4:
                return $"FLOW STABILITY: {Mathf.Max(0, 3 - flowDisruptions)}/3";
            case TopTipi.ChaosIncarnateBoss7:
            case TopTipi.Gorunmez:
                return $"VOID POPS: {voidPops}/10";
            case TopTipi.PrestigeBoss:
            case TopTipi.GravityCore:
                return $"GRAVITY CLEAR: {gravityProgress}/40";
            case TopTipi.Virus:
                if (DotSpawner.Instance != null)
                {
                    int totalVirus = 0;
                    foreach (Dot d in DotSpawner.Instance.ActiveDots)
                    {
                        if (d != null && d.Type == DotType.Virus) totalVirus++;
                    }
                    return $"VIRUS LOAD: {totalVirus} active";
                }
                return "";
            default:
                return "";
        }
    }

    public bool GetGlitchConnection(int c1, int c2)
    {
        if (ActiveBossType != TopTipi.Glitch) return false;
        
        if (GlitchRule == "ALL SELECT") return true;
        if (GlitchRule == "RED = BLUE")
        {
            return (c1 == 0 || c1 == 1) && (c2 == 0 || c2 == 1);
        }
        if (GlitchRule == "GREEN = YELLOW")
        {
            return (c1 == 2 || c1 == 3) && (c2 == 2 || c2 == 3);
        }
        return false;
    }

    public void CleanupBoss()
    {
        IsBossLevelActive = false;
        Physics2D.gravity = new Vector2(0, -9.81f);
        CleanupChaosCore();
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.12f, 0.12f, 0.16f); // Restore normal BG
            Camera.main.transform.position = new Vector3(0, 0, -10f); // Reset offset
        }
    }

    private void CleanupChaosCore()
    {
        if (chaosCoreInstance != null)
        {
            Destroy(chaosCoreInstance);
            chaosCoreInstance = null;
        }
    }

    private Sprite GetCircleSprite()
    {
        Sprite s = null;
        if (DotChainRushLibrary.Instance != null)
        {
            s = DotChainRushLibrary.Instance.GetTopSprite(TopTipi.KaosOrbBoss1);
            if (s == null) s = DotChainRushLibrary.Instance.GetTopSprite(TopTipi.KirmiziTop);
        }
        if (s == null)
        {
            var renderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                if (r != null && r.sprite != null && r.sprite.name.Contains("Circle"))
                {
                    s = r.sprite;
                    break;
                }
            }
        }
        return s;
    }

    private void CameraShake(float duration)
    {
        ComboManager.Instance?.TriggerCameraShake(duration, 0.08f);
    }
}
