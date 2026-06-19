using UnityEngine;
using UnityEngine.UI;

public class UIStarField : MonoBehaviour
{
    [Header("Mode Settings")]
    [SerializeField] private bool warpMode = true; // Toggle for Warp Speed effect

    [Header("Star Settings")]
    [SerializeField] private Sprite starSprite;
    [SerializeField] private int maxStars = 20;
    [SerializeField] private Vector2 starSizeRange = new Vector2(6f, 12f);

    [Header("Neon Glow Colors")]
    [SerializeField] private Color color1 = new Color(1.0f, 0.65f, 0.0f, 1.0f); // Bright Gold/Orange
    [SerializeField] private Color color2 = new Color(1.0f, 0.0f, 0.61f, 1.0f); // Neon Pink/Magenta

    [Header("Trail Settings")]
    [Range(1, 8)] [SerializeField] private int trailLength = 4; // Number of trail segments
    [Range(1f, 5f)] [SerializeField] private float trailSpacing = 2f; // Gap/delay between segments

    [Header("Float Mode Animation")]
    [SerializeField] private float minBlinkSpeed = 0.5f;
    [SerializeField] private float maxBlinkSpeed = 2.0f;
    [SerializeField] private float floatSpeed = 4f;

    [Header("Warp Mode Animation")]
    [SerializeField] private float minWarpSpeed = 80f;
    [SerializeField] private float maxWarpSpeed = 220f;
    [SerializeField] private float acceleration = 3f;

    private struct StarData
    {
        public RectTransform[] segments; // 0 is main star, others are trails
        public Image[] images;
        public Vector2[] positionHistory;
        public float blinkSpeed;
        public float blinkPhase;
        public float randomAngle;
        public Vector2 velocity;
        public float currentSpeed;
        public Vector2 spawnDirection;
        public Color starColor;
    }

    private StarData[] stars;
    private RectTransform parentRect;

    private void Start()
    {
        parentRect = GetComponent<RectTransform>();
        if (parentRect == null)
        {
            Debug.LogError("[UIStarField] Bu script sadece Canvas altındaki UI objelerine (RectTransform olan) eklenmelidir!");
            return;
        }

        if (starSprite == null)
        {
            Debug.LogWarning("[UIStarField] Lütfen Inspector üzerinden 'Star Sprite' alanına yıldız/çizgi görselinizi sürükleyin!");
        }

        stars = new StarData[maxStars];
        for (int i = 0; i < maxStars; i++)
        {
            CreateStar(i);
        }
    }

    private void CreateStar(int index)
    {
        GameObject starParent = new GameObject("StarGroup_" + index, typeof(RectTransform));
        starParent.transform.SetParent(transform, false);

        RectTransform groupRt = starParent.GetComponent<RectTransform>();
        groupRt.anchorMin = new Vector2(0.5f, 0.5f);
        groupRt.anchorMax = new Vector2(0.5f, 0.5f);
        groupRt.pivot = new Vector2(0.5f, 0.5f);
        groupRt.anchoredPosition = Vector2.zero;

        // Instantiate main star and its trail segments based on initial trailLength
        int allocatedLength = Mathf.Max(1, trailLength);
        RectTransform[] segments = new RectTransform[allocatedLength];
        Image[] images = new Image[allocatedLength];
        Vector2[] posHistory = new Vector2[allocatedLength * 5];

        for (int j = 0; j < allocatedLength; j++)
        {
            int segmentIndex = allocatedLength - 1 - j;
            GameObject segObj = new GameObject("Seg_" + segmentIndex, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            segObj.transform.SetParent(starParent.transform, false);

            segments[segmentIndex] = segObj.GetComponent<RectTransform>();
            images[segmentIndex] = segObj.GetComponent<Image>();
            images[segmentIndex].sprite = starSprite;
            images[segmentIndex].raycastTarget = false;
        }

        stars[index] = new StarData
        {
            segments = segments,
            images = images,
            positionHistory = posHistory
        };

        ResetStar(index, true);
    }

    private void ResetStar(int index, bool randomProgress = false)
    {
        StarData star = stars[index];
        float size = Random.Range(starSizeRange.x, starSizeRange.y);
        
        star.blinkSpeed = Random.Range(minBlinkSpeed, maxBlinkSpeed);
        star.blinkPhase = Random.Range(0f, Mathf.PI * 2f);
        star.randomAngle = Random.Range(0f, 360f);
        star.starColor = Color.Lerp(color1, color2, Random.value);

        int len = star.segments.Length;

        // Configure segment transforms safely using the allocated array length
        for (int j = 0; j < len; j++)
        {
            float scaleFactor = 1f - ((float)j / len) * 0.5f; // shrink towards tail
            star.segments[j].sizeDelta = new Vector2(size * scaleFactor, size * scaleFactor);
            star.segments[j].anchorMin = new Vector2(0.5f, 0.5f);
            star.segments[j].anchorMax = new Vector2(0.5f, 0.5f);
            star.segments[j].pivot = new Vector2(0.5f, 0.5f);
            star.segments[j].localScale = Vector3.one;
        }

        if (warpMode)
        {
            float angleRad = Random.Range(0f, Mathf.PI * 2f);
            star.spawnDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            star.currentSpeed = Random.Range(minWarpSpeed, maxWarpSpeed);

            float startDist = randomProgress ? Random.Range(5f, Mathf.Max(parentRect.rect.width, parentRect.rect.height) * 0.5f) : Random.Range(2f, 15f);
            Vector2 initialPos = star.spawnDirection * startDist;

            for (int k = 0; k < star.positionHistory.Length; k++)
            {
                star.positionHistory[k] = initialPos;
            }

            float angleDeg = Mathf.Atan2(star.spawnDirection.y, star.spawnDirection.x) * Mathf.Rad2Deg - 90f;
            for (int j = 0; j < len; j++)
            {
                star.segments[j].localRotation = Quaternion.Euler(0f, 0f, angleDeg);
            }
        }
        else
        {
            Vector2 randPos = GetRandomPosition();
            for (int k = 0; k < star.positionHistory.Length; k++)
            {
                star.positionHistory[k] = randPos;
            }
            star.velocity = Random.insideUnitCircle.normalized * floatSpeed;
        }

        stars[index] = star;
        UpdateSegmentPositions(index);
    }

    private void UpdateSegmentPositions(int index)
    {
        StarData star = stars[index];
        int len = star.segments.Length;
        
        star.segments[0].anchoredPosition = star.positionHistory[0];

        // Safely loop up to the actual segment array length
        for (int j = 1; j < len; j++)
        {
            int historyIndex = Mathf.Clamp(Mathf.RoundToInt(j * trailSpacing), 0, star.positionHistory.Length - 1);
            star.segments[j].anchoredPosition = star.positionHistory[historyIndex];
        }
    }

    private Vector2 GetRandomPosition()
    {
        if (parentRect == null) return Vector2.zero;
        float w = parentRect.rect.width * 0.9f;
        float h = parentRect.rect.height * 0.9f;
        return new Vector2(Random.Range(-w / 2f, w / 2f), Random.Range(-h / 2f, h / 2f));
    }

    private void Update()
    {
        if (stars == null || parentRect == null) return;

        float widthLimit = parentRect.rect.width * 0.5f;
        float heightLimit = parentRect.rect.height * 0.5f;
        float maxDistance = Mathf.Sqrt(widthLimit * widthLimit + heightLimit * heightLimit);

        for (int i = 0; i < stars.Length; i++)
        {
            StarData star = stars[i];
            if (star.segments[0] == null) continue;

            int len = star.segments.Length;

            // Shift position history array
            for (int k = star.positionHistory.Length - 1; k > 0; k--)
            {
                star.positionHistory[k] = star.positionHistory[k - 1];
            }

            if (warpMode)
            {
                star.currentSpeed += acceleration * Time.deltaTime * 60f;
                Vector2 nextPos = star.positionHistory[0] + star.spawnDirection * star.currentSpeed * Time.deltaTime;
                star.positionHistory[0] = nextPos;

                float distance = nextPos.magnitude;

                if (Mathf.Abs(nextPos.x) > widthLimit || Mathf.Abs(nextPos.y) > heightLimit)
                {
                    ResetStar(i, false);
                    continue;
                }

                UpdateSegmentPositions(i);

                for (int j = 0; j < len; j++)
                {
                    float segmentDist = star.segments[j].anchoredPosition.magnitude;
                    float baseAlpha = 1f;

                    if (segmentDist < 30f)
                    {
                        baseAlpha = segmentDist / 30f;
                    }
                    else if (segmentDist > maxDistance * 0.7f)
                    {
                        baseAlpha = Mathf.Clamp01(1f - (segmentDist - maxDistance * 0.7f) / (maxDistance * 0.3f));
                    }

                    float segmentFalloff = 1f - ((float)j / len) * 0.85f;
                    Color color = star.starColor;
                    color.a = baseAlpha * segmentFalloff;
                    star.images[j].color = color;
                }
            }
            else
            {
                Vector2 nextPos = star.positionHistory[0] + star.velocity * Time.deltaTime;
                if (Mathf.Abs(nextPos.x) > widthLimit) nextPos.x = -Mathf.Sign(nextPos.x) * widthLimit;
                if (Mathf.Abs(nextPos.y) > heightLimit) nextPos.y = -Mathf.Sign(nextPos.y) * heightLimit;

                star.positionHistory[0] = nextPos;
                UpdateSegmentPositions(i);

                float alpha = Mathf.PingPong(Time.time * star.blinkSpeed + star.blinkPhase, 1f);
                for (int j = 0; j < len; j++)
                {
                    star.segments[j].localRotation = Quaternion.Euler(0f, 0f, star.randomAngle + Time.time * 5f);
                    float segmentFalloff = 1f - ((float)j / len) * 0.85f;
                    Color color = star.starColor;
                    color.a = alpha * segmentFalloff;
                    star.images[j].color = color;
                }
            }

            stars[i] = star;
        }
    }
}
