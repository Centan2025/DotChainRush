using UnityEngine;

/// <summary>
/// Procedurally creates a rounded-corner frame (LineRenderer) around the play area.
/// The border has a traveling neon glow animation.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class PlayAreaFrame : MonoBehaviour
{
    [Header("Frame Settings")]
    [SerializeField] private float cornerRadius = 0.08f;
    [SerializeField] private int cornerSegments = 8;
    [SerializeField] private float lineWidth = 0.06f;

    [Header("Animation Settings")]
    [SerializeField] private Color dimColor = new Color(0.48f, 0.38f, 0.15f, 0.45f);
    [SerializeField] private Color glowColor = new Color(1.0f, 0.85f, 0.35f, 1.0f);
    [SerializeField] private float travelSpeed = 2.0f;
    [SerializeField] private float glowWidth = 0.15f; // fraction of perimeter that glows

    private LineRenderer lineRenderer;
    private Vector3[] framePoints;
    private float totalPerimeter;
    private float[] cumulativeDistances;

    // Cached bounds
    private float leftX, rightX, bottomY, topY;

    private void Start()
    {
        CreateFrame();
    }

    private void CreateFrame()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float aspect = cam.aspect;

        float orthoHeight = cam.orthographicSize * 2f;
        float orthoWidth = orthoHeight * aspect;

        float halfWidth = orthoWidth / 2f;
        float halfHeight = orthoHeight / 2f;

        // Match ScreenBoundsAdaptor's play area margins
        float headerHeight = 1.2f;
        float footerHeight = 1.4f;
        float sidePadding = 0.08f;

        leftX = -halfWidth + sidePadding;
        rightX = halfWidth - sidePadding;
        bottomY = -halfHeight + footerHeight;
        topY = halfHeight - headerHeight;

        // Build rounded rectangle points
        framePoints = BuildRoundedRect(leftX, rightX, bottomY, topY, cornerRadius, cornerSegments);

        // Setup LineRenderer
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = framePoints.Length;
        lineRenderer.SetPositions(framePoints);
        lineRenderer.loop = false; // We close it manually
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.sortingOrder = 5;
        Shader defaultShader = Shader.Find("Sprites/Default");
        if (defaultShader == null) defaultShader = Shader.Find("UI/Default");
        if (defaultShader == null) defaultShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        
        lineRenderer.material = new Material(defaultShader != null ? defaultShader : Shader.Find("Hidden/Internal-Colored"));
        lineRenderer.startColor = dimColor;
        lineRenderer.endColor = dimColor;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.numCapVertices = 4;

        // Pre-compute cumulative distances for animation
        cumulativeDistances = new float[framePoints.Length];
        cumulativeDistances[0] = 0f;
        for (int i = 1; i < framePoints.Length; i++)
        {
            cumulativeDistances[i] = cumulativeDistances[i - 1] + Vector3.Distance(framePoints[i - 1], framePoints[i]);
        }
        totalPerimeter = cumulativeDistances[framePoints.Length - 1];
    }

    private void Update()
    {
        if (lineRenderer == null || framePoints == null || totalPerimeter <= 0f) return;

        // Base line thickness breathing
        float breatheWidth = 0.024f + 0.003f * Mathf.Sin(Time.time * 2.0f);
        lineRenderer.startWidth = breatheWidth;
        lineRenderer.endWidth = breatheWidth;

        // Periodic vertical sweep: cycle every 6 seconds
        float cycleTime = 6.0f;
        float elapsed = Time.time % cycleTime;
        float sweepDuration = 2.4f;

        float sweepY = -999f;
        float sweepAlpha = 0f;
        bool isSweeping = elapsed < sweepDuration;
        if (isSweeping)
        {
            float progress = elapsed / sweepDuration;

            // Fade in/out the sweep intensity smoothly to prevent any sudden popping
            if (progress < 0.2f)
            {
                sweepAlpha = progress / 0.2f;
            }
            else if (progress > 0.8f)
            {
                sweepAlpha = (1.0f - progress) / 0.2f;
            }
            else
            {
                sweepAlpha = 1.0f;
            }

            // Smooth step interpolation for sweep position
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            sweepY = Mathf.Lerp(topY + 0.8f, bottomY - 0.8f, easedProgress);
        }

        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[8];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[8];

        for (int i = 0; i < 8; i++)
        {
            float t = (float)i / 7f; // fraction along line length

            // Map 1D fraction t to 3D world position
            float targetDist = t * totalPerimeter;
            Vector3 pos = GetPointAtDistance(targetDist);

            float intensity = 0f;
            if (isSweeping)
            {
                float dist = Mathf.Abs(pos.y - sweepY);
                float reflectWidth = 1.6f; // Extremely wide glow area for ultra-smooth transition
                if (dist < reflectWidth)
                {
                    // Cosine-squared falloff curve for a super soft light distribution
                    float shape = Mathf.Cos((dist / reflectWidth) * Mathf.PI * 0.5f);
                    intensity = shape * shape * sweepAlpha;
                }
            }

            Color targetColor;
            float targetAlpha;

            if (intensity > 0.6f)
            {
                float core = (intensity - 0.6f) / 0.4f;
                targetColor = Color.Lerp(glowColor, Color.white, core);
                targetAlpha = Mathf.Lerp(dimColor.a, 1.0f, intensity);
            }
            else
            {
                targetColor = Color.Lerp(dimColor, glowColor, intensity);
                targetAlpha = Mathf.Lerp(dimColor.a, 0.8f, intensity);
            }

            colorKeys[i] = new GradientColorKey(targetColor, t);
            alphaKeys[i] = new GradientAlphaKey(targetAlpha, t);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }

    private Vector3 GetPointAtDistance(float dist)
    {
        if (framePoints == null || framePoints.Length == 0) return Vector3.zero;
        if (dist <= 0f) return framePoints[0];
        if (dist >= totalPerimeter) return framePoints[framePoints.Length - 1];

        for (int i = 1; i < framePoints.Length; i++)
        {
            if (cumulativeDistances[i] >= dist)
            {
                float segmentLength = cumulativeDistances[i] - cumulativeDistances[i - 1];
                if (segmentLength <= 0f) return framePoints[i];
                float segmentT = (dist - cumulativeDistances[i - 1]) / segmentLength;
                return Vector3.Lerp(framePoints[i - 1], framePoints[i], segmentT);
            }
        }
        return framePoints[framePoints.Length - 1];
    }

    private Vector3[] BuildRoundedRect(float left, float right, float bottom, float top, float radius, int segments)
    {
        // Clamp radius to half the smaller dimension
        float maxRadius = Mathf.Min((right - left) * 0.5f, (top - bottom) * 0.5f);
        radius = Mathf.Min(radius, maxRadius);

        // 8 points for 45-degree beveled corners + 1 closing point
        Vector3[] points = new Vector3[9];

        points[0] = new Vector3(left + radius, bottom, 0f);
        points[1] = new Vector3(right - radius, bottom, 0f);
        points[2] = new Vector3(right, bottom + radius, 0f);
        points[3] = new Vector3(right, top - radius, 0f);
        points[4] = new Vector3(right - radius, top, 0f);
        points[5] = new Vector3(left + radius, top, 0f);
        points[6] = new Vector3(left, top - radius, 0f);
        points[7] = new Vector3(left, bottom + radius, 0f);
        points[8] = points[0]; // Close loop

        return points;
    }
}
