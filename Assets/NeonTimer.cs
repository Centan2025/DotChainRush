using UnityEngine;
using UnityEngine.UI;

public class NeonTimer : MonoBehaviour
{
    private Image uiImage;
    private Material neonMat;
    
    [Range(0, 1)] public float fillAmount = 1.0f; // Müdahale edeceğimiz değer
    public float countdownTime = 10.0f;          // Kaç saniyede bitsin?

    void Start()
    {
        uiImage = GetComponent<Image>();
        if (uiImage != null)
        {
            // Materyalin bir kopyasını alıyoruz ki orijinal dosya bozulmasın
            neonMat = new Material(uiImage.material);
            uiImage.material = neonMat;
        }
    }

    void Update()
    {
        if (neonMat != null && fillAmount > 0)
        {
            // Zamanı her saniye düşür
            fillAmount -= Time.deltaTime / countdownTime;
            
            // Shader Graph'ın içindeki o _FillAmount kutusuna yeni değeri gönderiyoruz!
            neonMat.SetFloat("_FillAmount", fillAmount);
            uiImage.SetMaterialDirty();
        }
    }
}