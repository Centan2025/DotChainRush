using UnityEngine;
using UnityEngine.UI;

public class NeonBackgroundAnimator : MonoBehaviour
{
    [Header("Base Settings")]
    [Range(0f, 1f)] [SerializeField] private float baseAlpha = 0.5f;

    private struct SmokeLayer
    {
        public Transform transform;
        public SpriteRenderer spriteRenderer;
        public Image uiImage;
        
        public float rotationSpeed;
        public float scaleSpeed;
        public float scaleAmount;
        public float driftSpeed;
        public Vector2 driftAmount;
        public float alphaSpeed;
        public float minAlpha;
        public float maxAlpha;
        
        public Vector3 initialScale;
        public Vector3 initialPosition;
        public float angleOffset;
    }

    private SmokeLayer[] layers;
    private bool isInitialized = false;
    private bool isWorldSpace = false;

    private void Start()
    {
        InitializeLayers();
    }

    private void InitializeLayers()
    {
        SpriteRenderer worldSR = GetComponent<SpriteRenderer>();
        Image uiImage = GetComponent<Image>();

        if (worldSR == null && uiImage == null)
        {
            // If neither is present, try to find/add SpriteRenderer (default to World Space background)
            worldSR = gameObject.AddComponent<SpriteRenderer>();
        }

        isWorldSpace = (worldSR != null);
        layers = new SmokeLayer[3];

        if (isWorldSpace)
        {
            // Force SpriteRenderer settings
            worldSR.color = new Color(1f, 1f, 1f, baseAlpha);
            worldSR.sortingOrder = -10; // Render far behind dots (dots are usually 0)

            Vector3 initScale = transform.localScale;
            if (initScale.x > 3f || initScale.y > 3f || initScale.sqrMagnitude < 0.001f)
            {
                initScale = Vector3.one;
            }

            layers[0] = new SmokeLayer
            {
                transform = transform,
                spriteRenderer = worldSR,
                rotationSpeed = 0.3f,
                scaleSpeed = 0.2f,
                scaleAmount = 0.03f,
                driftSpeed = 0.15f,
                driftAmount = new Vector2(0.2f, 0.2f), // Smaller drift units in World Space
                alphaSpeed = 0.1f,
                minAlpha = baseAlpha * 0.9f,
                maxAlpha = baseAlpha * 1.1f,
                initialScale = initScale,
                initialPosition = transform.localPosition,
                angleOffset = 0f
            };

            // Set default scale in world space to cover typical camera viewport
            transform.localScale = layers[0].initialScale;

            // Create Layer 2 & 3 as World Space children
            layers[1] = CreateWorldSubLayer("SmokeLayer_Mid", transform, worldSR.sprite, new Color(0.9f, 0.8f, 1f, baseAlpha * 0.6f), -9);
            layers[1].rotationSpeed = -0.6f;
            layers[1].scaleSpeed = 0.35f;
            layers[1].scaleAmount = 0.05f;
            layers[1].driftSpeed = 0.25f;
            layers[1].driftAmount = new Vector2(0.4f, 0.3f);
            layers[1].alphaSpeed = 0.18f;
            layers[1].minAlpha = baseAlpha * 0.4f;
            layers[1].maxAlpha = baseAlpha * 0.7f;
            layers[1].initialScale = Vector3.one * 1.1f;
            layers[1].angleOffset = 45f;

            layers[2] = CreateWorldSubLayer("SmokeLayer_Fore", transform, worldSR.sprite, new Color(0.8f, 0.9f, 1f, baseAlpha * 0.4f), -8);
            layers[2].rotationSpeed = 0.9f;
            layers[2].scaleSpeed = 0.5f;
            layers[2].scaleAmount = 0.07f;
            layers[2].driftSpeed = 0.4f;
            layers[2].driftAmount = new Vector2(0.5f, 0.4f);
            layers[2].alphaSpeed = 0.28f;
            layers[2].minAlpha = baseAlpha * 0.25f;
            layers[2].maxAlpha = baseAlpha * 0.5f;
            layers[2].initialScale = Vector3.one * 1.2f;
            layers[2].angleOffset = 90f;
        }
        else
        {
            // Canvas UI Space Background
            RectTransform mainRect = GetComponent<RectTransform>();
            uiImage.color = new Color(1f, 1f, 1f, baseAlpha);
            uiImage.raycastTarget = false;

            layers[0] = new SmokeLayer
            {
                transform = transform,
                uiImage = uiImage,
                rotationSpeed = 0.3f,
                scaleSpeed = 0.2f,
                scaleAmount = 0.03f,
                driftSpeed = 0.15f,
                driftAmount = new Vector2(10f, 10f),
                alphaSpeed = 0.1f,
                minAlpha = baseAlpha * 0.9f,
                maxAlpha = baseAlpha * 1.1f,
                initialScale = mainRect.localScale.sqrMagnitude > 0.01f ? mainRect.localScale : Vector3.one,
                initialPosition = mainRect.anchoredPosition,
                angleOffset = 0f
            };

            layers[1] = CreateUISubLayer("SmokeLayer_Mid", mainRect, uiImage.sprite, new Color(0.9f, 0.8f, 1f, baseAlpha * 0.6f));
            layers[1].rotationSpeed = -0.6f;
            layers[1].scaleSpeed = 0.35f;
            layers[1].scaleAmount = 0.05f;
            layers[1].driftSpeed = 0.25f;
            layers[1].driftAmount = new Vector2(25f, 20f);
            layers[1].alphaSpeed = 0.18f;
            layers[1].minAlpha = baseAlpha * 0.4f;
            layers[1].maxAlpha = baseAlpha * 0.7f;
            layers[1].initialScale = Vector3.one * 1.15f;
            layers[1].angleOffset = 45f;

            layers[2] = CreateUISubLayer("SmokeLayer_Fore", mainRect, uiImage.sprite, new Color(0.8f, 0.9f, 1f, baseAlpha * 0.4f));
            layers[2].rotationSpeed = 0.9f;
            layers[2].scaleSpeed = 0.5f;
            layers[2].scaleAmount = 0.07f;
            layers[2].driftSpeed = 0.4f;
            layers[2].driftAmount = new Vector2(35f, 30f);
            layers[2].alphaSpeed = 0.28f;
            layers[2].minAlpha = baseAlpha * 0.25f;
            layers[2].maxAlpha = baseAlpha * 0.5f;
            layers[2].initialScale = Vector3.one * 1.25f;
            layers[2].angleOffset = 90f;
        }

        // Apply initial configurations
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].transform != null)
            {
                layers[i].transform.localScale = Vector3.Scale(layers[i].transform.localScale, layers[i].initialScale);
                layers[i].transform.localRotation = Quaternion.Euler(0f, 0f, layers[i].angleOffset);
            }
        }

        isInitialized = true;
    }

    private SmokeLayer CreateWorldSubLayer(string name, Transform parent, Sprite sprite, Color color, int sortingOrder)
    {
        Transform existing = parent.Find(name);
        GameObject go;
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
        }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        if (parent.GetComponent<SpriteRenderer>() != null)
        {
            sr.sortingLayerName = parent.GetComponent<SpriteRenderer>().sortingLayerName;
        }

        return new SmokeLayer
        {
            transform = go.transform,
            spriteRenderer = sr,
            initialPosition = Vector3.zero
        };
    }

    private SmokeLayer CreateUISubLayer(string name, RectTransform parent, Sprite sprite, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject go;
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
        }

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;

        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = Vector2.zero;

        return new SmokeLayer
        {
            transform = r,
            uiImage = img,
            initialPosition = Vector3.zero
        };
    }

    private void Update()
    {
        if (!isInitialized) return;

        float time = Time.time;

        for (int i = 0; i < layers.Length; i++)
        {
            SmokeLayer layer = layers[i];
            if (layer.transform == null) continue;

            // 1. Slow Rotation
            layer.transform.Rotate(Vector3.forward, layer.rotationSpeed * Time.deltaTime);

            // 2. Pulsing Scale
            float scaleFactor = 1f + Mathf.Sin(time * layer.scaleSpeed + layer.angleOffset) * layer.scaleAmount;
            layer.transform.localScale = layer.initialScale * scaleFactor;

            // 3. Floating/Drifting Position
            float driftX = Mathf.Sin(time * layer.driftSpeed + layer.angleOffset) * layer.driftAmount.x;
            float driftY = Mathf.Cos(time * layer.driftSpeed * 0.8f + layer.angleOffset) * layer.driftAmount.y;
            
            if (isWorldSpace)
            {
                layer.transform.localPosition = layer.initialPosition + new Vector3(driftX, driftY, 0f);
            }
            else
            {
                ((RectTransform)layer.transform).anchoredPosition = (Vector2)layer.initialPosition + new Vector2(driftX, driftY);
            }

            // 4. Alpha breathing
            if (isWorldSpace && layer.spriteRenderer != null)
            {
                float alphaT = (Mathf.Sin(time * layer.alphaSpeed + layer.angleOffset) + 1f) / 2f;
                float targetAlpha = Mathf.Lerp(layer.minAlpha, layer.maxAlpha, alphaT);
                Color c = layer.spriteRenderer.color;
                layer.spriteRenderer.color = new Color(c.r, c.g, c.b, targetAlpha);
            }
            else if (!isWorldSpace && layer.uiImage != null)
            {
                float alphaT = (Mathf.Sin(time * layer.alphaSpeed + layer.angleOffset) + 1f) / 2f;
                float targetAlpha = Mathf.Lerp(layer.minAlpha, layer.maxAlpha, alphaT);
                Color c = layer.uiImage.color;
                layer.uiImage.color = new Color(c.r, c.g, c.b, targetAlpha);
            }
        }
    }
}
