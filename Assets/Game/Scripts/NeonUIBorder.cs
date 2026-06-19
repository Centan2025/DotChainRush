using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
public class NeonUIBorder : MaskableGraphic
{
    [Header("Border Settings")]
    [SerializeField] private float thickness = 6f;
    [SerializeField] private float cornerRadius = 15f;
    [SerializeField] private int cornerSegments = 8;

    [Header("Glow Settings")]
    [SerializeField] private float glowSize = 10f;
    [Range(0f, 1f)] [SerializeField] private float glowIntensity = 0.5f;

    [Header("Neon Animation")]
    [SerializeField] private Color color1 = new Color(0.12f, 0.45f, 1.0f, 1.0f); // Neon Blue
    [SerializeField] private Color color2 = new Color(0.92f, 0.12f, 0.52f, 1.0f); // Neon Pink
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float waveScale = 1.0f;

    private void Update()
    {
        if (Application.isPlaying)
        {
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;
        float halfThickness = thickness * 0.5f;

        // Clamp radius
        float maxRadius = Mathf.Min(rect.width, rect.height) * 0.5f - halfThickness - glowSize;
        float r = Mathf.Clamp(cornerRadius, 0f, maxRadius);

        // Precalculate corner center points
        float outL = rect.xMin + glowSize;
        float outR = rect.xMax - glowSize;
        float outB = rect.yMin + glowSize;
        float outT = rect.yMax - glowSize;

        Vector2 centerBL = new Vector2(outL + r, outB + r);
        Vector2 centerBR = new Vector2(outR - r, outB + r);
        Vector2 centerTR = new Vector2(outR - r, outT - r);
        Vector2 centerTL = new Vector2(outL + r, outT - r);

        int pointsPerCorner = cornerSegments + 1;
        int totalPoints = pointsPerCorner * 4;

        // We use 6 rings of vertices to decouple glow opacity from the solid border:
        // 0: Outer Glow Edge (Alpha = 0)
        // 1: Outer Glow Start (Alpha = glowIntensity)
        // 2: Outer Solid Border (Alpha = 1.0)
        // 3: Inner Solid Border (Alpha = 1.0)
        // 4: Inner Glow Start (Alpha = glowIntensity)
        // 5: Inner Glow Edge (Alpha = 0)
        UIVertex[] ring0 = new UIVertex[totalPoints];
        UIVertex[] ring1 = new UIVertex[totalPoints];
        UIVertex[] ring2 = new UIVertex[totalPoints];
        UIVertex[] ring3 = new UIVertex[totalPoints];
        UIVertex[] ring4 = new UIVertex[totalPoints];
        UIVertex[] ring5 = new UIVertex[totalPoints];

        int idx = 0;
        float timeOffset = Time.time * speed;

        System.Action<Vector2, float, float> generateCornerPoints = (center, startAngle, endAngle) =>
        {
            for (int i = 0; i <= cornerSegments; i++)
            {
                float t = (float)i / cornerSegments;
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Base animated color
                float posFraction = angle / (2f * Mathf.PI);
                float wave = Mathf.Sin(posFraction * waveScale * Mathf.PI * 2f - timeOffset) * 0.5f + 0.5f;
                Color baseColor = Color.Lerp(color1, color2, wave);

                Color glowColor = baseColor;
                glowColor.a *= glowIntensity;

                Color fadeColor = baseColor;
                fadeColor.a = 0f;

                // Positions
                Vector2 posGlowOut = center + dir * (r + glowSize);
                Vector2 posBorderOut = center + dir * r;
                Vector2 posBorderIn = center + dir * Mathf.Max(0f, r - thickness);
                Vector2 posGlowIn = center + dir * Mathf.Max(0f, r - thickness - glowSize);

                ring0[idx] = new UIVertex { position = posGlowOut, color = fadeColor };
                ring1[idx] = new UIVertex { position = posBorderOut, color = glowColor };
                ring2[idx] = new UIVertex { position = posBorderOut, color = baseColor };
                ring3[idx] = new UIVertex { position = posBorderIn, color = baseColor };
                ring4[idx] = new UIVertex { position = posBorderIn, color = glowColor };
                ring5[idx] = new UIVertex { position = posGlowIn, color = fadeColor };
                idx++;
            }
        };

        generateCornerPoints(centerTL, Mathf.PI, Mathf.PI * 0.5f);
        generateCornerPoints(centerTR, Mathf.PI * 0.5f, 0f);
        generateCornerPoints(centerBR, 0f, -Mathf.PI * 0.5f);
        generateCornerPoints(centerBL, -Mathf.PI * 0.5f, -Mathf.PI);

        // Add all vertices
        for (int i = 0; i < totalPoints; i++) vh.AddVert(ring0[i]);
        for (int i = 0; i < totalPoints; i++) vh.AddVert(ring1[i]);
        for (int i = 0; i < totalPoints; i++) vh.AddVert(ring2[i]);
        for (int i = 0; i < totalPoints; i++) vh.AddVert(ring3[i]);
        for (int i = 0; i < totalPoints; i++) vh.AddVert(ring4[i]);
        for (int i = 0; i < totalPoints; i++) vh.AddVert(ring5[i]);

        int offset0 = 0;
        int offset1 = totalPoints;
        int offset2 = totalPoints * 2;
        int offset3 = totalPoints * 3;
        int offset4 = totalPoints * 4;
        int offset5 = totalPoints * 5;

        // Bridge quads
        for (int i = 0; i < totalPoints; i++)
        {
            int next = (i + 1) % totalPoints;

            // Outer Glow (Ring 0 to Ring 1)
            vh.AddTriangle(offset0 + i, offset1 + next, offset0 + next);
            vh.AddTriangle(offset0 + i, offset1 + i, offset1 + next);

            // Solid Border (Ring 2 to Ring 3)
            vh.AddTriangle(offset2 + i, offset3 + next, offset2 + next);
            vh.AddTriangle(offset2 + i, offset3 + i, offset3 + next);

            // Inner Glow (Ring 4 to Ring 5)
            vh.AddTriangle(offset4 + i, offset5 + next, offset4 + next);
            vh.AddTriangle(offset4 + i, offset5 + i, offset5 + next);
        }
    }
}
