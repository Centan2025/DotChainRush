using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosEventSystem : MonoBehaviour
{
    public static ChaosEventSystem Instance { get; private set; }

    [Header("Event Timing")]
    public float eventInterval = 25f;
    private float eventTimer = 0f;
    private bool isEventActive = false;
    private ChaosEventType currentEventType = ChaosEventType.None;
    private Coroutine activeEventCoroutine;

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

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        eventTimer += Time.deltaTime;
        if (eventTimer >= eventInterval)
        {
            eventTimer = 0f;
            TriggerAISelectedEvent();
        }
    }

    public void TriggerAISelectedEvent()
    {
        if (isEventActive) return;

        // 1) Selection AI logic based on player skill & history
        float skill = GameBrain.Instance != null && GameBrain.Instance.meta != null ? GameBrain.Instance.meta.playerSkill : 1.0f;
        float failRate = TelemetrySystem.failRate;

        // Determine candidate events
        List<ChaosEventType> candidates = new List<ChaosEventType>();
        
        if (skill > 1.3f)
        {
            candidates.Add(ChaosEventType.ChaosStorm);
            candidates.Add(ChaosEventType.TimeCollapse);
            candidates.Add(ChaosEventType.QuantumError);
        }
        else if (failRate > 0.40f)
        {
            // Player is failing a lot, give them a mild or no event
            candidates.Add(ChaosEventType.None);
            candidates.Add(ChaosEventType.RealityShift); // relatively harmless color reshuffle
        }
        else
        {
            candidates.Add(ChaosEventType.GravityStorm);
            candidates.Add(ChaosEventType.MirrorWorld);
            candidates.Add(ChaosEventType.VoidInvasion);
            candidates.Add(ChaosEventType.VirusStorm);
        }

        ChaosEventType selected = candidates[Random.Range(0, candidates.Count)];
        if (selected != ChaosEventType.None)
        {
            StartEvent(selected);
        }
    }

    public void StartEvent(ChaosEventType eventType)
    {
        if (isEventActive) StopActiveEvent();

        currentEventType = eventType;
        isEventActive = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TriggerScreenFlash(Color.red, 0.4f);
            UIManager.Instance.ShowComboFeedback("CHAOS EVENT:", eventType.ToString().ToUpper(), "", Color.red);
        }

        activeEventCoroutine = StartCoroutine(ExecuteEventCoroutine(eventType));
        Debug.Log("[ChaosEventSystem] Triggered: " + eventType);
    }

    public void StopActiveEvent()
    {
        if (activeEventCoroutine != null)
        {
            StopCoroutine(activeEventCoroutine);
            activeEventCoroutine = null;
        }

        // Restore normal rules
        Physics2D.gravity = new Vector2(0f, -9.81f);
        isEventActive = false;
        currentEventType = ChaosEventType.None;
    }

    private void StopActiveEventSilent()
    {
        isEventActive = false;
        currentEventType = ChaosEventType.None;
    }

    private IEnumerator ExecuteEventCoroutine(ChaosEventType type)
    {
        float duration = 12.0f;
        float elapsed = 0f;

        switch (type)
        {
            case ChaosEventType.GravityStorm:
                // Periodically shift gravity direction
                while (elapsed < duration)
                {
                    Vector2 randGravity = new Vector2(Random.Range(-5f, 5f), Random.Range(-9.8f, -2f));
                    Physics2D.gravity = randGravity;
                    Debug.Log("[Chaos] Gravity Storm Shift: " + randGravity);
                    yield return new WaitForSeconds(3.0f);
                    elapsed += 3.0f;
                }
                break;

            case ChaosEventType.TimeCollapse:
                // Drain time rapidly
                while (elapsed < duration)
                {
                    RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
                    if (rtc != null) rtc.CurrentTime -= 1.0f; // drain 1s every update
                    CircularTimer ct = FindAnyObjectByType<CircularTimer>();
                    if (ct != null) ct.CurrentTime -= 1.0f;

                    yield return new WaitForSeconds(0.5f);
                    elapsed += 0.5f;
                }
                break;

            case ChaosEventType.MirrorWorld:
                // Horizontally flip touch inputs (represented by flipping bounds or mirroring dot placement)
                if (DotSpawner.Instance != null)
                {
                    DotSpawner.Instance.SetSpawnRange(2.2f, -2.2f); // inverted spawn columns!
                }
                yield return new WaitForSeconds(duration);
                if (DotSpawner.Instance != null)
                {
                    DotSpawner.Instance.SetSpawnRange(-2.2f, 2.2f); // restore
                }
                break;

            case ChaosEventType.QuantumError:
                // Teleport dots randomly
                while (elapsed < duration)
                {
                    if (DotSpawner.Instance != null)
                    {
                        var dots = DotSpawner.Instance.ActiveDots;
                        if (dots.Count > 0)
                        {
                            Dot target = dots[Random.Range(0, dots.Count)];
                            if (target != null)
                            {
                                target.transform.position = new Vector3(Random.Range(-2.2f, 2.2f), Random.Range(-2.5f, 2.0f), 0f);
                                Debug.Log("[Chaos] Quantum Teleported dot: " + target.name);
                            }
                        }
                    }
                    yield return new WaitForSeconds(1.5f);
                    elapsed += 1.5f;
                }
                break;

            case ChaosEventType.VoidInvasion:
                // Spawns void blocks at random intervals
                while (elapsed < duration)
                {
                    if (WorldDirector.Instance != null)
                    {
                        LevelConfig mockCfg = new LevelConfig();
                        mockCfg.obstacleTypes.Add(ObstacleType.VoidBlock);
                        // WorldDirector's internal spawn method
                        WorldDirector.Instance.SpawnLevelElements(mockCfg);
                    }
                    yield return new WaitForSeconds(4.0f);
                    elapsed += 4.0f;
                }
                break;

            case ChaosEventType.VirusStorm:
                // Spawns a virus block on screen
                if (WorldDirector.Instance != null)
                {
                    LevelConfig mockCfg = new LevelConfig();
                    mockCfg.obstacleTypes.Add(ObstacleType.VirusBlock);
                    WorldDirector.Instance.SpawnLevelElements(mockCfg);
                }
                yield return new WaitForSeconds(duration);
                break;

            case ChaosEventType.RealityShift:
                // Shuffle all colors of active dots
                if (DotSpawner.Instance != null)
                {
                    foreach (var dot in DotSpawner.Instance.ActiveDots)
                    {
                        if (dot != null && !dot.IsObstacle && !dot.IsBossDot)
                        {
                            int randColor = Random.Range(0, 5);
                            dot.Init(randColor, dot.Type, d => { if (DotSpawner.Instance != null) DotSpawner.Instance.DespawnDot(d); });
                        }
                    }
                }
                yield return new WaitForSeconds(duration);
                break;

            case ChaosEventType.ChaosStorm:
                // Gravity shift + rapid colors shift combined
                Physics2D.gravity = new Vector2(0f, 4f); // invert gravity completely!
                while (elapsed < duration)
                {
                    if (DotSpawner.Instance != null && DotSpawner.Instance.ActiveDots.Count > 0)
                    {
                        Dot target = DotSpawner.Instance.ActiveDots[Random.Range(0, DotSpawner.Instance.ActiveDots.Count)];
                        if (target != null && !target.IsObstacle)
                        {
                            target.Init(Random.Range(0, 5), target.Type, d => { if (DotSpawner.Instance != null) DotSpawner.Instance.DespawnDot(d); });
                        }
                    }
                    yield return new WaitForSeconds(1.0f);
                    elapsed += 1.0f;
                }
                break;
        }

        StopActiveEventSilent();
    }
}
