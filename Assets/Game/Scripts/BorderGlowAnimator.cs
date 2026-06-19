using UnityEngine;
using UnityEngine.UI;

public class BorderGlowAnimator : MonoBehaviour
{
    [SerializeField] private Image topBorder;
    [SerializeField] private Image bottomBorder;
    [SerializeField] private Image leftBorder;
    [SerializeField] private Image rightBorder;
    [SerializeField] private Image[] cornerImages;

    [Header("Animation Settings")]
    [SerializeField] private Color dimColor = new Color(0.25f, 0.1f, 0.6f, 0.35f);
    [SerializeField] private Color glowColor = new Color(0.55f, 0.25f, 1f, 0.9f);
    [SerializeField] private float speed = 1.5f;

    private Image[] allBorders;

    private void Start()
    {
        // Collect all border images
        int cornerCount = cornerImages != null ? cornerImages.Length : 0;
        allBorders = new Image[4 + cornerCount];
        allBorders[0] = topBorder;
        allBorders[1] = rightBorder;
        allBorders[2] = bottomBorder;
        allBorders[3] = leftBorder;
        for (int i = 0; i < cornerCount; i++)
        {
            allBorders[4 + i] = cornerImages[i];
        }
    }

    private void Update()
    {
        if (allBorders == null) return;

        // Traveling wave glow: each border lights up in sequence
        float cycle = Time.time * speed;

        for (int i = 0; i < allBorders.Length; i++)
        {
            if (allBorders[i] == null) continue;

            // Phase offset for each element creates a traveling wave
            float phase = cycle - (i * 0.35f);
            float wave = (Mathf.Sin(phase * Mathf.PI * 2f) + 1f) * 0.5f;

            allBorders[i].color = Color.Lerp(dimColor, glowColor, wave);
        }
    }
}
