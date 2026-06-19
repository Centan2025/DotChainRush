using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class SetupFeverSegmentedBar : Editor
{
    [MenuItem("Tools/Setup Fever Segmented Bar")]
    public static void SetupUI()
    {
        // 1. Generate a rounded-box sprite texture for the segments
        string spritePath = "Assets/Game/UI/RoundedBlock.png";
        GenerateRoundedBlockTexture(spritePath);

        // 2. Find Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Setup] Canvas bulunamadı! Lütfen önce sahnenizi açın.");
            return;
        }

        // 3. Find UIManager
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[Setup] UIManager bulunamadı!");
            return;
        }

        // 4. Find or Create Fever Panel Container
        Transform feverPanelTransform = null;
        SerializedObject soUIManager = new SerializedObject(uiManager);
        SerializedProperty spFeverPanel = soUIManager.FindProperty("feverPanel");

        if (spFeverPanel != null && spFeverPanel.objectReferenceValue != null)
        {
            GameObject fpGO = (GameObject)spFeverPanel.objectReferenceValue;
            feverPanelTransform = fpGO.transform;
        }

        // Fallback: search hierarchy
        if (feverPanelTransform == null)
        {
            feverPanelTransform = FindRecursive(canvas.transform, "FeverPanelContainer");
            if (feverPanelTransform == null)
            {
                feverPanelTransform = FindRecursive(canvas.transform, "FeverPanel");
            }
        }

        if (feverPanelTransform != null)
        {
            Object.DestroyImmediate(feverPanelTransform.gameObject);
        }

        Debug.Log("[Setup] FeverPanelContainer temizlendi ve yeniden oluşturuluyor...");
        GameObject newFeverPanel = new GameObject("FeverPanelContainer", typeof(RectTransform));
        newFeverPanel.transform.SetParent(canvas.transform, false);

        // Position FeverPanelContainer directly as the segmented bar container size
        RectTransform feverPanelRect = newFeverPanel.GetComponent<RectTransform>();
        feverPanelRect.anchorMin = new Vector2(0f, 1f);
        feverPanelRect.anchorMax = new Vector2(0f, 1f);
        feverPanelRect.pivot = new Vector2(0f, 1f);
        feverPanelRect.anchoredPosition = new Vector2(50f, -195f); // Positioned on left side under Pause
        feverPanelRect.sizeDelta = new Vector2(170f, 44f); // Exact size of segmented bar

        feverPanelTransform = newFeverPanel.transform;

        // Link to UIManager
        if (spFeverPanel != null)
        {
            spFeverPanel.objectReferenceValue = newFeverPanel;
            soUIManager.ApplyModifiedProperties();
        }

        // Deactivate old linear progress bar if it exists
        SerializedProperty spOldBar = soUIManager.FindProperty("feverProgressBar");
        if (spOldBar != null && spOldBar.objectReferenceValue != null)
        {
            Image oldBar = (Image)spOldBar.objectReferenceValue;
            oldBar.gameObject.SetActive(false);
        }

        // 5. Create Segmented Bar Container (Background Image only) directly inside FeverPanelContainer
        GameObject barContainer = new GameObject("FeverSegmentedBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barContainer.transform.SetParent(feverPanelTransform, false);

        RectTransform containerRect = barContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero; // stretch to fill parent FeverPanelContainer
        containerRect.localPosition = Vector3.zero;

        Image containerBg = barContainer.GetComponent<Image>();
        containerBg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        containerBg.type = Image.Type.Sliced;
        containerBg.color = new Color(0.08f, 0.02f, 0.14f, 0.95f);

        // Create a separate child GameObject for the Neon Border
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(CanvasRenderer));
        borderObj.transform.SetParent(barContainer.transform, false);

        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderRect.localPosition = Vector3.zero;

        NeonUIBorder border = borderObj.AddComponent<NeonUIBorder>();
        SerializedObject soBorder = new SerializedObject(border);
        soBorder.FindProperty("thickness").floatValue = 2.5f;
        soBorder.FindProperty("cornerRadius").floatValue = 14f;
        soBorder.FindProperty("glowSize").floatValue = 6f;
        soBorder.FindProperty("glowIntensity").floatValue = 0.5f;
        soBorder.FindProperty("color1").colorValue = new Color(0.44f, 0.16f, 0.69f);
        soBorder.FindProperty("color2").colorValue = new Color(0.44f, 0.16f, 0.69f);
        soBorder.FindProperty("speed").floatValue = 0f;
        soBorder.ApplyModifiedProperties();

        // 6. Create Child Container for segments
        GameObject gridObj = new GameObject("Grid", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        gridObj.transform.SetParent(barContainer.transform, false);

        RectTransform gridRect = gridObj.GetComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.sizeDelta = new Vector2(-12f, -10f);
        gridRect.localPosition = Vector3.zero;

        HorizontalLayoutGroup layout = gridObj.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 5f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        // 7. Spawn 5 Segment Rounded children
        Sprite blockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        Image[] segmentsArray = new Image[5];

        for (int i = 0; i < 5; i++)
        {
            GameObject segObj = new GameObject("Segment_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            segObj.transform.SetParent(gridObj.transform, false);

            RectTransform segRect = segObj.GetComponent<RectTransform>();
            segRect.localPosition = Vector3.zero;

            Image segImg = segObj.GetComponent<Image>();
            segImg.sprite = blockSprite;
            segImg.type = Image.Type.Sliced;
            segImg.color = new Color(0.24f, 0.08f, 0.44f, 0.6f);
            segImg.raycastTarget = false;

            segmentsArray[i] = segImg;
        }

        // 8. Add FeverSegmentedBar script and link references
        FeverSegmentedBar segmentedBarScript = barContainer.AddComponent<FeverSegmentedBar>();
        SerializedObject soBarScript = new SerializedObject(segmentedBarScript);
        SerializedProperty spSegments = soBarScript.FindProperty("segments");
        for (int i = 0; i < 5; i++)
        {
            spSegments.GetArrayElementAtIndex(i).objectReferenceValue = segmentsArray[i];
        }
        soBarScript.ApplyModifiedProperties();

        // 9. Assign to UIManager
        SerializedProperty spBarRef = soUIManager.FindProperty("feverSegmentedBar");
        if (spBarRef != null)
        {
            spBarRef.objectReferenceValue = segmentedBarScript;
            soUIManager.ApplyModifiedProperties();
            Debug.Log("[Setup] UIManager feverSegmentedBar referansı bağlandı!");
        }

        // Save
        EditorUtility.SetDirty(uiManager);
        EditorUtility.SetDirty(barContainer);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Setup] Fever Segmented Bar (Sadece Bar) dikey hizalamasız ve şık şekilde oluşturuldu!");
    }

    private static Transform FindRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindRecursive(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    private static void GenerateRoundedBlockTexture(string path)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = 16f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = (x < radius) ? radius : (x >= size - radius) ? size - radius - 1 : x;
                float cy = (y < radius) ? radius : (y >= size - radius) ? size - radius - 1 : y;

                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = 1.0f;
                if (x < radius || x >= size - radius || y < radius || y >= size - radius)
                {
                    if (dist > radius)
                    {
                        alpha = Mathf.Clamp01(1.0f - (dist - radius));
                    }
                }

                float ny = (float)y / size;
                float baseIntensity = Mathf.Lerp(0.35f, 0.95f, ny);

                float nx = (float)(x - size / 2) / (size / 2);
                float highlightX = Mathf.Clamp01(1f - nx * nx * 1.5f);
                float highlightY = Mathf.Exp(-Mathf.Pow(y - size * 0.72f, 2f) / 18f);
                float spec = highlightX * highlightY * 0.85f;

                Color col = new Color(baseIntensity, baseIntensity, baseIntensity, alpha);
                if (spec > 0.01f)
                {
                    col.r = Mathf.Clamp01(col.r + spec);
                    col.g = Mathf.Clamp01(col.g + spec);
                    col.b = Mathf.Clamp01(col.b + spec);
                }

                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64f;
            importer.spriteBorder = new Vector4(18, 18, 18, 18);
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }
}
