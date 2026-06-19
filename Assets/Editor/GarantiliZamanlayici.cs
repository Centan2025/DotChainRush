using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProceduralTimer : MonoBehaviour 
{
    [Header("Görsel Bağlantılar")]
    public Image shaderSlayerImage; // Hiyerarşideki 'Radial_Shader_Layer' objesini buraya sürükle
    public TextMeshProUGUI timerDisplay; // 'TimerText' objesini buraya sürükle
    
    [Header("Zaman Ayarları")]
    public float duration = 88f; // 1:28 = 88 saniye
    private float timeRemaining;
    
    private Material runtimeMaterial;
    private static readonly int FillAmountID = Shader.PropertyToID("_FillAmount");

    void Start() 
    {
        timeRemaining = duration;

        if (shaderSlayerImage != null && shaderSlayerImage.material != null)
        {
            // Materyalin kopyasını oluşturuyoruz ki diğer objeleri etkilemesin
            runtimeMaterial = new Material(shaderSlayerImage.material);
            shaderSlayerImage.material = runtimeMaterial;
        }
    }

    void Update() 
    {
        if (timeRemaining > 0) 
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0) timeRemaining = 0;
            UpdateVisuals();
        }
    }

    void UpdateVisuals() 
    {
        float progress = timeRemaining / duration;
        
        // 1. Shader'daki ilerlemeyi günceller
        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat(FillAmountID, progress);
        }
        
        // 2. Yazıyı günceller
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerDisplay.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
}