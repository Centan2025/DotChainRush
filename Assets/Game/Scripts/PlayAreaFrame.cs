using UnityEngine;

/// <summary>
/// Procedurally creates a rounded-corner frame (LineRenderer) around the play area.
/// The border has a traveling neon glow animation.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class PlayAreaFrame : MonoBehaviour
{
    [Header("Frame Settings")]
    [SerializeField] private float cornerRadius = 0.35f;
    [SerializeField] private int cornerSegments = 8;
    [SerializeField] private float lineWidth = 0.06f;

    [Header("Animation Settings")]
    [SerializeField] private Color dimColor = new Color(0.15f, 0.08f, 0.4f, 0.5f);
    [SerializeField] private Color glowColor = new Color(0.5f, 0.2f, 1f, 1f);
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
        float footerHeight = 0.6f;
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

        // Significantly thinner line (0.02f base thickness)
        float breatheWidth = 0.018f + 0.004f * Mathf.Sin(Time.time * 2.0f);
        lineRenderer.startWidth = breatheWidth;
        lineRenderer.endWidth = breatheWidth;

        // Dynamic soft breathing opacity (glow pulsing) that fades in and out slowly
        float baseAlpha = 0.45f + 0.35f * Mathf.Sin(Time.time * 1.5f); // pulses between 0.10 and 0.80 opacity

        // Very slow, elegant scroll rate for the Gemini spectrum (0.12f speed)
        float timeOffset = (Time.time * 0.08f) % 1f;

        // Create the 8 key points of the Gemini Spectrum
        // Gemini Colors: Deep Blue -> Indigo/Purple -> Hot Pink/Red -> Warm Orange -> Pastel Yellow -> Soft Teal/Green -> Cyan -> Deep Blue
        Color[] geminiColors = new Color[]
        {
            new Color(0.12f, 0.45f, 1.0f, 0.95f),   // Blue
            new Color(0.48f, 0.18f, 0.95f, 0.95f),  // Purple/Violet
            new Color(0.92f, 0.12f, 0.52f, 0.95f),  // Pink/Red
            new Color(0.98f, 0.38f, 0.08f, 0.95f),  // Orange
            new Color(0.95f, 0.82f, 0.12f, 0.95f),  // Pastel Yellow
            new Color(0.12f, 0.85f, 0.42f, 0.95f),  // Green
            new Color(0.08f, 0.82f, 0.85f, 0.95f),  // Cyan
            new Color(0.12f, 0.45f, 1.0f, 0.95f)    // Back to Blue for loop
        };

        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[8];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[8];

        for (int i = 0; i < 8; i++)
        {
            // Calculate shifted position to create rotation effect
            float t = (float)i / 7f; // Distribute keys evenly along the line 0..1
            float shiftedT = Mathf.Repeat(t + timeOffset, 1.0f);

            // Interpolate colors based on shifted position to get the color at this physical fraction of the line
            Color targetColor = GetGeminiColorAt(shiftedT, geminiColors);
            
            // Soft neon look: boost brightness slightly, keep alphas soft but visible
            colorKeys[i] = new GradientColorKey(targetColor, t);
            alphaKeys[i] = new GradientAlphaKey(baseAlpha, t);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }

    private Color GetGeminiColorAt(float t, Color[] spectrum)
    {
        float floatIdx = t * (spectrum.Length - 1);
        int idx = Mathf.FloorToInt(floatIdx);
        float nextIdx = Mathf.Ceil(floatIdx);
        float lerpT = floatIdx - idx;

        return Color.Lerp(spectrum[idx], spectrum[(int)nextIdx % spectrum.Length], lerpT);
    }

    private Vector3[] BuildRoundedRect(float left, float right, float bottom, float top, float radius, int segments)
    {
        // Clamp radius to half the smaller dimension
        float maxRadius = Mathf.Min((right - left) * 0.5f, (top - bottom) * 0.5f);
        radius = Mathf.Min(radius, maxRadius);

        int totalPoints = (segments + 1) * 4 + 1; // 4 corners + closing point
        Vector3[] points = new Vector3[totalPoints];
        int idx = 0;

        // Bottom-left corner (from bottom to left)
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI + (Mathf.PI * 0.5f) * ((float)i / segments);
            points[idx++] = new Vector3(
                left + radius + Mathf.Cos(angle) * radius,
                bottom + radius + Mathf.Sin(angle) * radius,
                0f
            );
        }

        // Bottom-right corner
        for (int i = 0; i <= segments; i++)
        {
            float angle = 1.5f * Mathf.PI + (Mathf.PI * 0.5f) * ((float)i / segments);
            points[idx++] = new Vector3(
                right - radius + Mathf.Cos(angle) * radius,
                bottom + radius + Mathf.Sin(angle) * radius,
                0f
            );
        }

        // Top-right corner
        for (int i = 0; i <= segments; i++)
        {
            float angle = 0f + (Mathf.PI * 0.5f) * ((float)i / segments);
            points[idx++] = new Vector3(
                right - radius + Mathf.Cos(angle) * radius,
                top - radius + Mathf.Sin(angle) * radius,
                0f
            );
        }

        // Top-left corner
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI * 0.5f + (Mathf.PI * 0.5f) * ((float)i / segments);
            points[idx++] = new Vector3(
                left + radius + Mathf.Cos(angle) * radius,
                top - radius + Mathf.Sin(angle) * radius,
                0f
            );
        }

        // Close the loop
        points[idx] = points[0];

        return points;
    }
}
