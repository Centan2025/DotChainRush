using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class FeverSegmentedBar : MonoBehaviour
{
    [Header("Segment Images (Assign 5 images, or leave empty to auto-generate)")]
    [SerializeField] private Image[] segments = new Image[5];

    [Header("Color Settings")]
    [SerializeField] private Color litColor = new Color(1.0f, 0.0f, 0.81f, 1.0f); // Bright neon pink/magenta
    [SerializeField] private Color unlitColor = new Color(0.24f, 0.08f, 0.44f, 0.6f); // Dark purple slot
    [SerializeField] private Color glowColor = new Color(1.0f, 1.0f, 1.0f, 1.0f); // White hot core highlight

    [Header("Progress Settings")]
    [Range(0f, 1f)] [SerializeField] private float currentFill = 0.2f; // Preview fill in editor (0..1)

    private void Awake()
    {
        FindAndAssignSegments();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            FindAndAssignSegments();
            UpdateSegmentsVisuals(currentFill);
        }
    }

    private void OnValidate()
    {
        FindAndAssignSegments();
        UpdateSegmentsVisuals(currentFill);
    }

    private void FindAndAssignSegments()
    {
        if (segments == null || segments.Length != 5)
        {
            segments = new Image[5];
        }

        // Reset to prevent caching duplicate references
        for (int i = 0; i < 5; i++)
        {
            segments[i] = null;
        }

        // Find all child images (excluding the parent itself and Border)
        Image[] childImages = GetComponentsInChildren<Image>(true);
        System.Collections.Generic.List<Image> validImages = new System.Collections.Generic.List<Image>();
        foreach (Image img in childImages)
        {
            if (img.gameObject != gameObject && !img.gameObject.name.Contains("Border"))
            {
                validImages.Add(img);
            }
        }

        // Sort by sibling index so they are in visual/layout order
        validImages.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        // Assign and rename to ensure names correspond to their actual order
        for (int i = 0; i < 5 && i < validImages.Count; i++)
        {
            segments[i] = validImages[i];
            if (segments[i] != null && segments[i].gameObject.name != "Segment_" + i)
            {
                segments[i].gameObject.name = "Segment_" + i;
            }
        }
    }

    public void SetProgress(float fillAmount)
    {
        currentFill = Mathf.Clamp01(fillAmount);
        UpdateSegmentsVisuals(currentFill);
    }

    private void UpdateSegmentsVisuals(float fill)
    {
        int totalSegments = segments.Length;
        float segmentValue = 1f / totalSegments;

        Debug.Log($"[FeverSegmentedBar] SetProgress: fill={fill:F3}, totalSegments={totalSegments}, segmentValue={segmentValue:F3}");

        for (int i = 0; i < totalSegments; i++)
        {
            if (segments[i] == null)
            {
                Debug.LogWarning($"[FeverSegmentedBar] Segment_{i} is NULL!");
                continue;
            }

            float segmentThreshold = (i + 1) * segmentValue;
            bool isLit = fill >= segmentThreshold - 0.01f;
            
            Debug.Log($"[FeverSegmentedBar] Segment_{i} ({segments[i].gameObject.name}): threshold={segmentThreshold:F3}, isLit={isLit}");

            if (isLit)
            {
                segments[i].color = litColor;
                
                // Add a glowing core effect to the last active cell if it's lit
                if (fill < segmentThreshold + segmentValue - 0.01f || i == totalSegments - 1)
                {
                    // Add bright hot center glow
                    segments[i].color = Color.Lerp(litColor, glowColor, 0.4f);
                }
            }
            else
            {
                segments[i].color = unlitColor;
            }
        }
    }
}
