using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance { get; private set; }

    [SerializeField] private Color[] dotColors = new Color[]
    {
        new Color(1f, 0.15f, 0.25f),   // Neon Hot Red (Index 0: KirmiziTop)
        new Color(0f, 0.65f, 1.0f),    // Electric Blue (Index 1: MaviTop)
        new Color(0f, 1.0f, 0.35f),    // Electric Lime/Green (Index 2: YesilTop)
        new Color(1f, 0.95f, 0f),      // High-vis Yellow (Index 3: SariTop)
        new Color(0.75f, 0.2f, 1f)     // Glowing Purple (Index 4: MorTop)
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
