using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class ChainController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxConnectDistance = 2.3f;
    [SerializeField] private LayerMask dotLayerMask;

    private LineRenderer lineRenderer;
    private LineRenderer coreLineRenderer;
    private readonly List<Dot> currentChain = new List<Dot>();
    private int currentChainColor = -1;
    private Camera mainCamera;
    private bool isDrawing = false;
    private bool wasTouchDrawing = false;
    private float connectionCooldownTimer = 0f;
    private float activeCooldownDuration = 0.15f;
    private Coroutine blockFeedbackCoroutine;
    private bool isRedFeedbackActive = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        mainCamera = Camera.main;

        // Configure LineRenderer visuals (Uniform wider glowing energy tube)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.38f);   // Even thicker connection line matching smaller bubble size
        widthCurve.AddKey(1f, 0.38f);
        lineRenderer.widthCurve = widthCurve;
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = -1; // Render behind the dots

        // Initialize coreLineRenderer for the thin electric blue/white center line
        GameObject coreObj = new GameObject("CoreLightningLine");
        coreObj.transform.SetParent(transform);
        coreLineRenderer = coreObj.AddComponent<LineRenderer>();
        coreLineRenderer.sharedMaterial = lineRenderer.sharedMaterial;
        coreLineRenderer.useWorldSpace = true;
        coreLineRenderer.sortingOrder = 0; // Render on top of the outer connection line
        coreLineRenderer.textureMode = lineRenderer.textureMode;

        AnimationCurve coreWidthCurve = new AnimationCurve();
        coreWidthCurve.AddKey(0f, 0.11f); // Even thicker core line
        coreWidthCurve.AddKey(1f, 0.11f);
        coreLineRenderer.widthCurve = coreWidthCurve;
        coreLineRenderer.widthMultiplier = 1f;
        coreLineRenderer.positionCount = 0;

        // Electric colors: Electric Blue / Cyan to White gradient, with translucent/faint visibility (alpha = 0.68)
        Gradient electricGradient = new Gradient();
        electricGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.3f, 0.7f, 1f), 0f),    // Cyan/electric blue
                new GradientColorKey(Color.white, 0.5f),               // White hot core
                new GradientColorKey(new Color(0f, 0.5f, 1f), 1f)      // Deep electric blue
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.68f, 0f), 
                new GradientAlphaKey(0.68f, 1f) 
            }
        );
        coreLineRenderer.colorGradient = electricGradient;
    }

    private void OnEnable()
    {
        TouchInputProcessor.OnInputDown += StartChain;
        TouchInputProcessor.OnInputDragged += ContinueChain;
        TouchInputProcessor.OnInputUp += HandleInputUp;
    }

    private void OnDisable()
    {
        TouchInputProcessor.OnInputDown -= StartChain;
        TouchInputProcessor.OnInputDragged -= ContinueChain;
        TouchInputProcessor.OnInputUp -= HandleInputUp;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            if (isDrawing) EndChain(false);
            return;
        }

        // Update connection cooldown timer
        if (connectionCooldownTimer > 0f)
        {
            connectionCooldownTimer -= Time.deltaTime;
            if (connectionCooldownTimer < 0f) connectionCooldownTimer = 0f;

            // Update selection progress of the last dot in the chain
            if (currentChain.Count > 0)
            {
                Dot lastDot = currentChain[currentChain.Count - 1];
                if (lastDot != null)
                {
                    float progress = activeCooldownDuration > 0f 
                        ? 1f - (connectionCooldownTimer / activeCooldownDuration)
                        : 1f;
                    lastDot.SelectionProgress = progress;
                }
            }
        }
        else
        {
            if (currentChain.Count > 0)
            {
                Dot lastDot = currentChain[currentChain.Count - 1];
                if (lastDot != null)
                {
                    lastDot.SelectionProgress = 1.0f;
                }
            }
        }

        // Pulsating/vibrating electric plasma line effect when drawing
        if (isDrawing && currentChain.Count > 0)
        {
            // Rapid high-frequency flickering representing raw electrical current
            float baseFlicker = Mathf.Sin(Time.time * 90f) * 0.15f + (Random.value - 0.5f) * 0.1f;
            lineRenderer.widthMultiplier = 0.85f + baseFlicker;
            if (coreLineRenderer != null)
            {
                coreLineRenderer.widthMultiplier = 0.85f + baseFlicker * 1.6f;
            }
            GenerateLightningPoints();
        }
    }

    private void StartChain(Vector3 worldPos, Dot dot)
    {
        if (dot != null)
        {
            isDrawing = true;
            currentChainColor = dot.IsSpecial ? -1 : dot.ColorId;
            AddDotToChain(dot);
            ApplyDefaultColors();
        }
    }

    private void ContinueChain(Vector3 worldPos, Dot dot)
    {
        if (dot == null || currentChain.Count == 0) return;

        bool isValidColorMatch = (currentChainColor == -1) || 
                                 (dot.IsSpecial) || 
                                 (dot.ColorId == currentChainColor);

        if (!isValidColorMatch && BossDimensionManager.Instance != null && BossDimensionManager.Instance.IsBossLevelActive)
        {
            isValidColorMatch = BossDimensionManager.Instance.GetGlitchConnection(currentChainColor, dot.ColorId);
        }

        if (isValidColorMatch)
        {
            int index = currentChain.IndexOf(dot);
            if (index == -1)
            {
                // Wait for connection cooldown before adding another dot
                if (connectionCooldownTimer > 0f)
                {
                    Debug.Log($"[ChainController] Cooldown active. Remaining: {connectionCooldownTimer:F2}s | Target: {dot.name}");
                    return;
                }

                float dist = Vector2.Distance(currentChain[currentChain.Count - 1].transform.position, dot.transform.position);
                if (dist <= maxConnectDistance)
                {
                    // Check segment intersection to prevent crossing lines
                    Vector2 newStart = currentChain[currentChain.Count - 1].transform.position;
                    Vector2 newEnd = dot.transform.position;
                    bool intersects = false;
                    for (int i = 0; i < currentChain.Count - 2; i++)
                    {
                        Vector2 existingStart = currentChain[i].transform.position;
                        Vector2 existingEnd = currentChain[i + 1].transform.position;
                        if (AreSegmentsIntersecting(newStart, newEnd, existingStart, existingEnd))
                        {
                            intersects = true;
                            break;
                        }
                    }

                    if (intersects)
                    {
                        TriggerBlockVisualFeedback();
                        return;
                    }

                    // ChainLimiter check
                    if (BossDimensionManager.Instance != null && 
                        BossDimensionManager.Instance.IsBossLevelActive && 
                        BossDimensionManager.Instance.Adaptation.counterAbility == "ChainLimiter" && 
                        currentChain.Count >= 6)
                    {
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowComboFeedback("CHAIN LIMIT! Max 6!", Color.red);
                        }
                        return;
                    }

                    if (currentChainColor == -1 && !dot.IsSpecial)
                    {
                        currentChainColor = dot.ColorId;
                        ApplyDefaultColors();
                    }
                    
                    AddDotToChain(dot);
                }
            }
            else if (index == currentChain.Count - 2)
            {
                RemoveLastDotFromChain();
                RecalculateChainColor();
            }
        }
    }

    private void HandleInputUp()
    {
        if (isDrawing) EndChain(true);
    }

    private void EndChain(bool processMatch)
    {
        isDrawing = false;

        if (processMatch && currentChain.Count >= 3)
        {
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.ProcessChain(currentChain);
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(currentChain.Count);
            }

            foreach (Dot dot in currentChain)
            {
                if (dot != null)
                {
                    dot.DestroyDot();
                }
            }
        }
        else
        {
            foreach (Dot dot in currentChain)
            {
                if (dot != null)
                {
                    dot.Deselect();
                }
            }
        }

        currentChain.Clear();
        currentChainColor = -1;
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = 1.0f;
        if (coreLineRenderer != null)
        {
            coreLineRenderer.positionCount = 0;
            coreLineRenderer.widthMultiplier = 1.0f;
        }
        isRedFeedbackActive = false;
        if (blockFeedbackCoroutine != null)
        {
            StopCoroutine(blockFeedbackCoroutine);
            blockFeedbackCoroutine = null;
        }
        ApplyDefaultColors();
    }
    }

    private void AddDotToChain(Dot dot)
    {
        currentChain.Add(dot);
        dot.Select();

        // Get connection cooldown from difficulty manager
        if (DifficultyManager.Instance != null)
        {
            activeCooldownDuration = DifficultyManager.Instance.ConnectionCooldown;
        }
        else
        {
            activeCooldownDuration = 0.15f;
        }

        connectionCooldownTimer = activeCooldownDuration;
        dot.SelectionProgress = activeCooldownDuration > 0f ? 0f : 1f;

        Debug.Log($"[ChainController] Dot added to chain: {dot.name}. Set cooldown: {activeCooldownDuration:F2}s. Active Level: {(DifficultyManager.Instance != null ? DifficultyManager.Instance.ActiveLevel : -1)}");

        UpdateLineVisuals();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayConnectSound(currentChain.Count);
        }
    }

    private void RemoveLastDotFromChain()
    {
        if (currentChain.Count > 0)
        {
            Dot last = currentChain[currentChain.Count - 1];
            last.Deselect();
            currentChain.RemoveAt(currentChain.Count - 1);
            UpdateLineVisuals();

            // Reset cooldown timer on backtracking
            connectionCooldownTimer = 0f;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayConnectSound(currentChain.Count);
            }
        }
    }

    private void RecalculateChainColor()
    {
        currentChainColor = -1;
        foreach (Dot dot in currentChain)
        {
            if (dot != null && !dot.IsSpecial)
            {
                currentChainColor = dot.ColorId;
                break;
            }
        }
        ApplyDefaultColors();
    }

    private void UpdateLineVisuals()
    {
        GenerateLightningPoints();
    }

    private void GenerateLightningPoints()
    {
        if (currentChain.Count < 2)
        {
            if (currentChain.Count == 1)
            {
                lineRenderer.positionCount = 1;
                lineRenderer.SetPosition(0, currentChain[0].transform.position);
                if (coreLineRenderer != null)
                {
                    coreLineRenderer.positionCount = 1;
                    coreLineRenderer.SetPosition(0, currentChain[0].transform.position);
                }
            }
            else
            {
                lineRenderer.positionCount = 0;
                if (coreLineRenderer != null)
                {
                    coreLineRenderer.positionCount = 0;
                }
            }
            return;
        }

        List<Vector3> outerPoints = new List<Vector3>();
        List<Vector3> corePoints = new List<Vector3>();
        int subdivisions = 5;

        for (int i = 0; i < currentChain.Count - 1; i++)
        {
            Vector3 start = currentChain[i].transform.position;
            Vector3 end = currentChain[i + 1].transform.position;

            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
            float distance = Vector3.Distance(start, end);

            outerPoints.Add(start);
            corePoints.Add(start);

            for (int j = 1; j < subdivisions; j++)
            {
                float t = (float)j / subdivisions;
                Vector3 point = Vector3.Lerp(start, end, t);

                // Double-octave jagged noise for natural electric lightning look
                // 1st octave: Base displacement (moderate frequency)
                float noise1 = Mathf.PerlinNoise(Time.time * 45f, (i * subdivisions + j) * 0.8f) * 2f - 1f;
                // 2nd octave: Sharp micro-jagged spikes (very high frequency)
                float noise2 = Mathf.PerlinNoise(Time.time * 125f + 5f, (i * subdivisions + j) * 2.8f) * 2f - 1f;

                float combinedNoise = (noise1 * 0.75f) + (noise2 * 0.25f);
                float offset = combinedNoise * 0.15f * Mathf.Min(distance, 1.6f);

                // For the inner electric core filament, use another high-frequency noise combo with less offset
                float coreNoise1 = Mathf.PerlinNoise(Time.time * 55f + 12f, (i * subdivisions + j) * 0.9f) * 2f - 1f;
                float coreNoise2 = Mathf.PerlinNoise(Time.time * 150f + 25f, (i * subdivisions + j) * 3.3f) * 2f - 1f;

                float combinedCore = (coreNoise1 * 0.7f) + (coreNoise2 * 0.3f);
                float coreOffset = combinedCore * 0.05f * Mathf.Min(distance, 1.6f);

                outerPoints.Add(point + perpendicular * offset);
                corePoints.Add(point + perpendicular * coreOffset);
            }
        }
        Vector3 lastPoint = currentChain[currentChain.Count - 1].transform.position;
        outerPoints.Add(lastPoint);
        corePoints.Add(lastPoint);

        lineRenderer.positionCount = outerPoints.Count;
        for (int k = 0; k < outerPoints.Count; k++)
        {
            lineRenderer.SetPosition(k, outerPoints[k]);
        }

        if (coreLineRenderer != null)
        {
            if (coreLineRenderer.sharedMaterial == null && lineRenderer.sharedMaterial != null)
            {
                coreLineRenderer.sharedMaterial = lineRenderer.sharedMaterial;
            }
            coreLineRenderer.positionCount = corePoints.Count;
            for (int k = 0; k < corePoints.Count; k++)
            {
                coreLineRenderer.SetPosition(k, corePoints[k]);
            }
        }
    }

    private bool AreSegmentsIntersecting(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Approximately(d, 0f)) return false; // Parallel or collinear

        float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        // Tolerances in [0.03, 0.97] to allow connection at joint vertices without triggering intersection
        return (u >= 0.03f && u <= 0.97f && v >= 0.03f && v <= 0.97f);
    }

    private void TriggerBlockVisualFeedback()
    {
        if (blockFeedbackCoroutine != null) StopCoroutine(blockFeedbackCoroutine);
        blockFeedbackCoroutine = StartCoroutine(BlockFeedbackCoroutine());
    }

    private System.Collections.IEnumerator BlockFeedbackCoroutine()
    {
        isRedFeedbackActive = true;

        // Set core to bright red
        if (coreLineRenderer != null)
        {
            Gradient redCoreGradient = new Gradient();
            redCoreGradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.red, 0f), 
                    new GradientColorKey(new Color(1f, 0.4f, 0.4f), 0.5f), 
                    new GradientColorKey(Color.red, 1f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.95f, 0f), 
                    new GradientAlphaKey(0.95f, 1f) 
                }
            );
            coreLineRenderer.colorGradient = redCoreGradient;
        }

        // Set main line to red
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowComboFeedback("BLOCKED!", Color.red);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDangerWarningSound(true);
        }

        // Shake camera slightly
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.TriggerCameraShake(0.08f, 0.12f);
        }

        yield return new WaitForSeconds(0.2f);

        isRedFeedbackActive = false;
        ApplyDefaultColors();
    }

    private void ApplyDefaultColors()
    {
        if (isRedFeedbackActive) return;

        // Apply standard outer line color
        if (currentChain.Count > 0)
        {
            Color color = Color.white;
            if (currentChainColor != -1 && ColorManager.Instance != null)
            {
                color = ColorManager.Instance.GetColor(currentChainColor);
            }
            color.a = 0.52f;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        // Reset core gradient to electric blue/white
        if (coreLineRenderer != null)
        {
            Gradient electricGradient = new Gradient();
            electricGradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.3f, 0.7f, 1f), 0f), 
                    new GradientColorKey(Color.white, 0.5f), 
                    new GradientColorKey(new Color(0f, 0.5f, 1f), 1f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.68f, 0f), 
                    new GradientAlphaKey(0.68f, 1f) 
                }
            );
            coreLineRenderer.colorGradient = electricGradient;
        }
    }

    private Dot GetDotAtPosition(Vector2 position)
    {
        float touchRadius = 0.15f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, touchRadius, dotLayerMask);

        Dot closestDot = null;
        float closestDist = float.MaxValue;
        foreach (var hit in hits)
        {
            Dot dot = hit.GetComponent<Dot>();
            if (dot != null && !dot.IsObstacle)
            {
                float dist = Vector2.Distance(position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestDot = dot;
                }
            }
        }
        return closestDot;
    }
}
