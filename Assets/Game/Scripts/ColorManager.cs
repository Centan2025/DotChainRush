using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance { get; private set; }

    [SerializeField] private Color[] dotColors = new Color[]
    {
        new Color(0.75f, 0.2f, 1f),    // Glowing Purple
        new Color(0f, 1f, 0.35f),      // Electric Lime/Green
        new Color(1f, 0.95f, 0f),      // High-vis Yellow
        new Color(1f, 0.15f, 0.25f),   // Neon Hot Red
        new Color(1f, 0.45f, 0f)       // Electric Orange
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Color GetColor(int index)
    {
        if (dotColors == null || dotColors.Length == 0) return Color.white;
        return dotColors[Mathf.Clamp(index, 0, dotColors.Length - 1)];
    }

    public int GetColorCount()
    {
        return dotColors != null ? dotColors.Length : 0;
    }
}
