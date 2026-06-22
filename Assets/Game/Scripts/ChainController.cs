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
    private readonly List<Dot> currentChain = new List<Dot>();
    private int currentChainColor = -1;
    private Camera mainCamera;
    private bool isDrawing = false;
    private bool wasTouchDrawing = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        mainCamera = Camera.main;

        // Configure LineRenderer visuals (Uniform thick glowing energy tube)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.15f);   // Thinner connection line matching smaller bubble size
        widthCurve.AddKey(1f, 0.15f);
        lineRenderer.widthCurve = widthCurve;
        lineRenderer.widthMultiplier = 1f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = -1; // Render behind the dots
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

        // Pulsating/vibrating neon plasma line effect when drawing
        if (isDrawing && currentChain.Count > 0)
        {
            lineRenderer.widthMultiplier = 0.8f + 0.2f * Mathf.Sin(Time.time * 40f);
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
            
            Color color = dot.IsSpecial 
                ? Color.white 
                : (ColorManager.Instance != null ? ColorManager.Instance.GetColor(currentChainColor) : Color.white);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }

    private void ContinueChain(Vector3 worldPos, Dot dot)
    {
        if (dot == null || currentChain.Count == 0) return;

        bool isValidColorMatch = (currentChainColor == -1) || 
                                 (dot.IsSpecial) || 
                                 (dot.ColorId == currentChainColor);

        if (isValidColorMatch)
        {
            int index = currentChain.IndexOf(dot);
            if (index == -1)
            {
                float dist = Vector2.Distance(currentChain[currentChain.Count - 1].transform.position, dot.transform.position);
                if (dist <= maxConnectDistance)
                {
                    if (currentChainColor == -1 && !dot.IsSpecial)
                    {
                        currentChainColor = dot.ColorId;
                        Color color = ColorManager.Instance != null ? ColorManager.Instance.GetColor(currentChainColor) : Color.white;
                        lineRenderer.startColor = color;
                        lineRenderer.endColor = color;
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
    }

    private void AddDotToChain(Dot dot)
    {
        currentChain.Add(dot);
        dot.Select();
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
                Color color = ColorManager.Instance != null ? ColorManager.Instance.GetColor(currentChainColor) : Color.white;
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
                break;
            }
        }
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
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
            return;
        }

        List<Vector3> points = new List<Vector3>();
        int subdivisions = 5;

        for (int i = 0; i < currentChain.Count - 1; i++)
        {
            Vector3 start = currentChain[i].transform.position;
            Vector3 end = currentChain[i + 1].transform.position;

            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
            float distance = Vector3.Distance(start, end);

            points.Add(start);

            for (int j = 1; j < subdivisions; j++)
            {
                float t = (float)j / subdivisions;
                Vector3 point = Vector3.Lerp(start, end, t);

                // Add time-based crackle using PerlinNoise for continuous animated lightning
                float noise = Mathf.PerlinNoise(Time.time * 24f, (i * subdivisions + j) * 0.4f) * 2f - 1f;
                float offset = noise * 0.16f * Mathf.Min(distance, 1.6f);

                point += perpendicular * offset;
                points.Add(point);
            }
        }
        points.Add(currentChain[currentChain.Count - 1].transform.position);

        lineRenderer.positionCount = points.Count;
        for (int k = 0; k < points.Count; k++)
        {
            lineRenderer.SetPosition(k, points[k]);
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
