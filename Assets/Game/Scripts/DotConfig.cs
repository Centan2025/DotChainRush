using UnityEngine;

[CreateAssetMenu(fileName = "DotConfig", menuName = "Config/DotConfig")]
public class DotConfig : ScriptableObject
{
    public string DotName;
    public Color ThemeColor = Color.white;
    public bool IsSpecial;
    public bool IsObstacle;
    public bool IsFastDot;
    public Sprite CustomSprite;
}
