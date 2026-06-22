using System;
using System.Collections;
using UnityEngine;

public enum DotType
{
    Kozmik, KirmiziTop, MaviTop, YesilTop, SariTop, MorTop, Gokkusagi, HizliTop, AgirTop, Bomba, Buz, Yercekimi,
    Kilit, Anahtar, Zaman, KaosOrbBoss1, ElementalFuryBoss2, Ates, BuzElement, Doga, Bosluk, Altin2x, Elektrik, Su, Toprak,
    Isik, Ayna, KaraDelik, SahteTop, CiftYonlu, Yapiskan, Teleport, FlowMasterBoss4, Mutasyon, ZincirUstasi, PatlamaCekirdegi, GokkusagiKani, ZamanBukucu,
    Kalkan, Skor2x, Can, Gorunmez, Kuantum, Glitch, ZamanLorduBoss, TersYercekimi, PatlayiciYagmur, ChaosIncarnateBoss7, PrestigeBoss, Virus, GravityCore,
    OneMistake, HizCekirdegi, Kritik, OlumTopu, GerceklikKirici, KorruptBomba, VoidRainbow, ElitAgir, SonsuzZaman, KaosCekirdegi, Omega, TheVoidFinalBoss, ComboTopu,
    Magnet, Jackpot, Ruzgar, DonmaAlani, Lazer, Enerji, Cogalan, SiyahIsik, Yildiz, Nukleer, BoyutKapisi, Hyper, Sinirsiz,
    Plazma, KaranlikMadde, Rezonans, SolucanDeligi, Nebula, YildizTozu, Sonsuzluk, Kutsal
}

public class Dot : MonoBehaviour
{
    public enum AnimState { Idle, Shrinking }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite specialSprite;
    [SerializeField] private Sprite obstacleSprite;
    [SerializeField] private Sprite starSprite;

    public int ColorId { get; private set; }
    public bool IsSelected { get; private set; }
    public float SelectionProgress { get; set; } = 1.0f;
    public DotType Type { get; private set; } = DotType.KirmiziTop;

    public bool IsSpecial => Type == DotType.Gokkusagi || Type == DotType.VoidRainbow || Type == DotType.Sonsuzluk;
    public bool IsObstacle => Type == DotType.AgirTop || Type == DotType.Buz || Type == DotType.ElitAgir || Type == DotType.OlumTopu;
    public bool IsFastDot => Type == DotType.HizliTop;
    public bool IsBomb => Type == DotType.Bomba || Type == DotType.Ates || Type == DotType.Nukleer || Type == DotType.KorruptBomba || Type == DotType.PatlamaCekirdegi;
    public bool IsFrozen => Type == DotType.Buz || Type == DotType.BuzElement || Type == DotType.DonmaAlani;
    public bool IsMetal => Type == DotType.AgirTop || Type == DotType.ElitAgir;
    public bool IsTimeDot => Type == DotType.Zaman || Type == DotType.SonsuzZaman || Type == DotType.ZamanBukucu;
    public bool IsBossDot => BalanceDB.IsBoss((TopTipi)Type);

    public AnimState CurrentAnimState { get; private set; } = AnimState.Idle;

    private Coroutine animCoroutine;
    private Rigidbody2D rb;
    private Transform specialRing;
    private Transform smokeTransform;
    private Transform visualCore;
    private Transform highlightTransform;
    private Transform starSparkle;
    private float currentVisualCoreScale = 1.0f;
    private LineRenderer progressLine;

    private void Awake()
    {
        visualCore = transform.Find("VisualCore");
        if (spriteRenderer == null)
        {
            if (visualCore != null) spriteRenderer = visualCore.GetComponent<SpriteRenderer>();
            else spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (normalSprite == null && spriteRenderer != null)
        {
            normalSprite = spriteRenderer.sprite;
        }
        if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        specialRing = transform.Find("SpecialRing");
        if (specialRing != null)
        {
            starSparkle = specialRing.Find("StarSparkle");
        }
        smokeTransform = transform.Find("Smoke");
        highlightTransform = transform.Find("Highlight");
    }

    private void Update()
    {
        // Offscreen check: uses a fixed safe threshold independent of Cinemachine camera shake.
        // Play area bottom is approximately -3.5f. Dots can't go below ~-5f physically.
        // We use -12f to give generous margin so falling dots are NEVER culled prematurely.
        float orthoSize = (Camera.main != null) ? Camera.main.orthographicSize : 5f;
        float offscreenThreshold = -orthoSize - 7f; // e.g. -5 - 7 = -12  (independent of cam Y)

        if (transform.position.y < offscreenThreshold)
        {
            Debug.Log($"[Dot Recycle] Recycled offscreen: {gameObject.name} | y={transform.position.y:F2} threshold={offscreenThreshold:F2} type={Type}");
            RecycleImmediate();
            return;
        }

        // Juiciness: smooth shrunken visual core + concentric neon halo border + cloud-like halo radiating outwards
        if (IsSelected && CurrentAnimState == AnimState.Idle)
        {
            UpdateProgressCircle();
            Color baseColor = IsSpecial ? Color.white : ColorManager.Instance.GetColor(ColorId);

            // Smooth shrink animation for the visual core and highlight to 0.30f
            currentVisualCoreScale = Mathf.Lerp(currentVisualCoreScale, 0.30f, Time.deltaTime * 18f);
            if (visualCore != null)
            {
                visualCore.localScale = Vector3.one * currentVisualCoreScale;
            }
            if (highlightTransform != null)
            {
                highlightTransform.localScale = Vector3.one * currentVisualCoreScale;
            }

            if (specialRing != null)
            {
                specialRing.gameObject.SetActive(true);
                
                if (SelectionProgress < 1.0f)
                {
                    // Scale from 0 to 1.1 based on progress
                    specialRing.localScale = Vector3.one * (SelectionProgress * 1.1f);
                    
                    // Rotate slower during progress/charging
                    specialRing.Rotate(Vector3.forward * 45f * Time.deltaTime);
                    
                    SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
                    if (ringSR != null)
                    {
                        // Semi-transparent color based on progress
                        ringSR.color = new Color(baseColor.r, baseColor.g, baseColor.b, SelectionProgress * 0.8f);
                    }
                }
                else
                {
                    // Fully charged - breathe the outer border scale slightly
                    float ringPulse = 1.05f + 0.08f * Mathf.Sin(Time.time * 10f);
                    specialRing.localScale = Vector3.one * ringPulse;
                    
                    // Orbit the star by rotating the special ring
                    specialRing.Rotate(Vector3.forward * 90f * Time.deltaTime);
                    
                    SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
                    if (ringSR != null)
                    {
                        // Ring glows bright in bubble's own color (slightly intensified)
                        ringSR.color = new Color(baseColor.r * 1.5f, baseColor.g * 1.5f, baseColor.b * 1.5f, 1.0f);
                    }
                }

                if (starSparkle != null)
                {
                    starSparkle.gameObject.SetActive(true);
                    // Rapid sparkle pulse for the star
                    float sparklePulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 25f);
                    starSparkle.localScale = Vector3.one * (0.25f + sparklePulse * 0.15f);
                    
                    // Local spin for the star itself
                    starSparkle.Rotate(Vector3.forward * -300f * Time.deltaTime);
                    
                    SpriteRenderer starSR = starSparkle.GetComponent<SpriteRenderer>();
                    if (starSR != null)
                    {
                        starSR.color = new Color(1f, 1f, 1f, 0.5f + sparklePulse * 0.5f);
                    }
                }
            }

            if (smokeTransform != null)
            {
                smokeTransform.gameObject.SetActive(true);
                // Radiate/expand smoke halo wave from center outwards (subtler alpha)
                float waveT = (Time.time * 2.0f) % 1.0f;
                float waveScale = Mathf.Lerp(0.6f, 1.8f, waveT);
                float waveAlpha = Mathf.Lerp(0.65f, 0.0f, waveT);
                
                smokeTransform.localScale = Vector3.one * waveScale;
                SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
                if (smokeSR != null)
                {
                    smokeSR.color = new Color(baseColor.r, baseColor.g, baseColor.b, waveAlpha);
                }
            }

            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer activeSR = vcSR != null ? vcSR : spriteRenderer;

            if (activeSR != null && ColorManager.Instance != null)
            {
                // Glow core dynamically in its own color with breathing glow intensity
                float corePulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 24f);
                Color glowColor = Color.Lerp(baseColor, Color.white, corePulse * 0.5f);
                activeSR.color = new Color(glowColor.r, glowColor.g, glowColor.b, 1f);
            }
        }
        // Dopamine rotation for Special (Rainbow) wild-card dots
        else if (IsSpecial && CurrentAnimState == AnimState.Idle)
        {
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer activeSR = vcSR != null ? vcSR : spriteRenderer;

            transform.Rotate(Vector3.forward * -60f * Time.deltaTime);
            if (activeSR != null) activeSR.color = Color.white;
            
            // Soft breathe for special dot smoke
            if (smokeTransform != null)
            {
                float breathe = 1.45f + 0.1f * Mathf.Sin(Time.time * 4f);
                smokeTransform.localScale = Vector3.one * breathe;
            }
        }
        // Rapid rotation for Fast Dots
        else if (IsFastDot && CurrentAnimState == AnimState.Idle)
        {
            transform.Rotate(Vector3.forward * -180f * Time.deltaTime);
            
            if (smokeTransform != null)
            {
                float breathe = 1.45f + 0.1f * Mathf.Sin(Time.time * 8f);
                smokeTransform.localScale = Vector3.one * breathe;
            }
        }
        // Static 3D bomb look — no pulsing, slow rotation for depth feel
        else if (IsBomb && CurrentAnimState == AnimState.Idle)
        {
            // Slow rotation gives a 3D bomb-like feel
            transform.Rotate(Vector3.forward * -15f * Time.deltaTime);

            // Static dark red-black gradient color (no flash)
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer activeSR = vcSR != null ? vcSR : spriteRenderer;
            if (activeSR != null)
            {
                activeSR.color = new Color(0.45f, 0.02f, 0.0f);
            }

            // Static danger ring — no pulse
            if (specialRing != null)
            {
                specialRing.localScale = Vector3.one * 1.15f;
                SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
                if (ringSR != null)
                {
                    ringSR.color = new Color(1f, 0.05f, 0.0f, 0.85f);
                }
            }

            // Static red glow smoke — no pulse
            if (smokeTransform != null)
            {
                smokeTransform.localScale = Vector3.one * 1.5f;
                SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
                if (smokeSR != null)
                {
                    smokeSR.color = new Color(1f, 0.1f, 0.0f, 0.35f);
                }
            }
        }
        // Boss dots: pulsing dark-magenta core + electric ring (only after ScaleIn finished)
        else if (IsBossDot && CurrentAnimState == AnimState.Idle && animCoroutine == null)
        {
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer activeSR = vcSR != null ? vcSR : spriteRenderer;

            // Pulsing colour: deep electric magenta
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
            Color bossCore = Color.Lerp(new Color(0.6f, 0f, 1f), new Color(1f, 0f, 0.6f), pulse);
            if (activeSR != null) activeSR.color = bossCore;

            // Slow heavy rotation
            transform.Rotate(Vector3.forward * -25f * Time.deltaTime);

            if (specialRing != null)
            {
                specialRing.gameObject.SetActive(false);
            }

            if (smokeTransform != null)
            {
                float waveT = (Time.time * 1.5f) % 1f;
                smokeTransform.localScale = Vector3.one * Mathf.Lerp(1.8f, 3.0f, waveT);
                SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
                if (smokeSR != null) smokeSR.color = new Color(0.7f, 0f, 1f, Mathf.Lerp(0.5f, 0f, waveT));
            }
        }
        else if (CurrentAnimState == AnimState.Idle)
        {
            // Soft breathing for normal unselected dots' cloud halos
            if (smokeTransform != null)
            {
                float breathe = 1.45f + 0.1f * Mathf.Sin(Time.time * 3f + GetHashCode() % 5);
                smokeTransform.localScale = Vector3.one * breathe;
            }

            // Roll effect: dynamically rotate the dot based on its horizontal physical movement
            if (rb != null)
            {
                float vx = rb.linearVelocity.x;
                if (Mathf.Abs(vx) > 0.02f)
                {
                    float radius = 0.22f; // approximate radius of normal dot
                    float rotSpeed = -(vx / radius) * Mathf.Rad2Deg;
                    transform.Rotate(Vector3.forward * rotSpeed * Time.deltaTime);
                }
            }
        }

        // Keep Highlight and Smoke at fixed world rotation so specular/glow
        // don't spin with the parent (prevents marbled/hareli appearance)
        if (highlightTransform != null)
        {
            highlightTransform.rotation = Quaternion.identity;
        }
        if (smokeTransform != null && !IsSelected)
        {
            smokeTransform.rotation = Quaternion.identity;
        }
    }

    public void Init(int colorId, DotType type, Action<Dot> onLifeEnd)
    {
        Type = type; // Set Type immediately first so properties (IsSpecial, IsBomb, etc.) function correctly
        ColorId = colorId;
        IsSelected = false;
        CurrentAnimState = AnimState.Idle;

        if (circleCollider != null)
        {
            circleCollider.enabled = true;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            if (GameManager.Instance != null && GameManager.Instance.IsFeverActive)
            {
                rb.gravityScale = 0.08f;
            }
            else
            {
                float baseGravity = DifficultyManager.Instance != null ? DifficultyManager.Instance.GravityScale : 0.5f;
                rb.gravityScale = IsFastDot ? baseGravity * 2.2f : baseGravity;
            }
        }

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }

        // Start scale at zero for a smooth pop-in reveal effect when spawning
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity; // Reset rotation from recycled Speed/Rainbow dots
        currentVisualCoreScale = 1.0f;

        float targetScale = IsBomb ? 0.52f : (BalanceDB.IsBoss((TopTipi)Type) ? 0.75f : 0.44f);
        if (gameObject.activeInHierarchy)
        {
            animCoroutine = StartCoroutine(ScaleInCoroutine(targetScale));
        }
        else
        {
            // If spawned while not active in hierarchy (e.g. pool prep), assign full scale immediately
            transform.localScale = Vector3.one * targetScale;
        }

        // Ensure sprite alpha is fully restored to 1.0f when recycled
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, 1.0f);
        }

        if (visualCore == null) visualCore = transform.Find("VisualCore");
        if (visualCore != null)
        {
            visualCore.localScale = Vector3.one;
            SpriteRenderer vcSR = visualCore.GetComponent<SpriteRenderer>();
            if (vcSR != null)
            {
                Color c = vcSR.color;
                vcSR.color = new Color(c.r, c.g, c.b, 1.0f); // Reset visualCore alpha
            }
        }

        if (highlightTransform == null) highlightTransform = transform.Find("Highlight");
        if (highlightTransform != null)
        {
            highlightTransform.localScale = Vector3.one;
            highlightTransform.localRotation = Quaternion.identity;
            // Reset highlight color to pure white (specular overlay)
            SpriteRenderer highlightSR = highlightTransform.GetComponent<SpriteRenderer>();
            if (highlightSR != null) highlightSR.color = new Color(1f, 1f, 1f, highlightSR.color.a);
        }

        if (specialRing == null) specialRing = transform.Find("SpecialRing");
        if (specialRing != null)
        {
            specialRing.gameObject.SetActive(false);
            specialRing.localScale = Vector3.one;
            specialRing.localRotation = Quaternion.identity;
            SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
            if (ringSR != null) ringSR.color = Color.white;
            
            if (starSparkle == null) starSparkle = specialRing.Find("StarSparkle");
            if (starSparkle != null)
            {
                starSparkle.gameObject.SetActive(false);
                starSparkle.localRotation = Quaternion.identity;
            }
        }

        if (smokeTransform == null) smokeTransform = transform.Find("Smoke");
        if (smokeTransform != null)
        {
            SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
            if (smokeSR != null)
            {
                if (IsFastDot)
                {
                    smokeSR.color = new Color(1f, 0.5f, 0f, 0.25f); // Subtle warm tint
                }
                else if (IsObstacle)
                {
                    if (IsMetal)
                    {
                        smokeSR.color = new Color(0.3f, 0.3f, 0.35f, 0.2f); // Subtle metallic
                    }
                    else
                    {
                        smokeSR.color = new Color(0.4f, 0.7f, 1f, 0.2f); // Subtle frozen blue
                    }
                }
                else if (IsBomb)
                {
                    smokeSR.color = new Color(1f, 0.1f, 0.0f, 0.45f); // Visible red glow for bombs
                }
                else if (IsTimeDot)
                {
                    smokeSR.color = new Color(0f, 1f, 0.3f, 0.28f); // Subtle green hint
                }
                else
                {
                    Color baseColor = IsSpecial ? Color.white : ColorManager.Instance.GetColor(ColorId);
                    smokeSR.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.22f); // Subtler soft cloud-like halo
                }
            }
            smokeTransform.localScale = Vector3.one * 1.35f;
            smokeTransform.localRotation = Quaternion.identity;
            smokeTransform.gameObject.SetActive(true);
        }

        if (IsBossDot)
        {
            // Boss: use library sprite if available, otherwise vivid electric-magenta fallback
            ApplySpriteAndColor(normalSprite, new Color(0.6f, 0f, 1f));

            // No pink ring on boss dots
            if (specialRing != null)
            {
                specialRing.gameObject.SetActive(false);
            }

            // Wide purple glow halo
            if (smokeTransform != null)
            {
                smokeTransform.localScale = Vector3.one * 2.5f;
                SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
                if (smokeSR != null) smokeSR.color = new Color(0.7f, 0f, 1f, 0.45f);
                smokeTransform.gameObject.SetActive(true);
            }
        }
        else if (IsObstacle)
        {
            ApplySpriteAndColor(obstacleSprite, IsMetal ? new Color(0.4f, 0.4f, 0.45f) : new Color(0.7f, 0.9f, 1f));
        }
        else if (IsSpecial)
        {
            ApplySpriteAndColor(specialSprite, Color.white);
        }
        else if (IsBomb)
        {
            ApplySpriteAndColor(normalSprite, new Color(0.6f, 0.0f, 0.0f));
            
            if (specialRing != null)
            {
                specialRing.gameObject.SetActive(false);
                specialRing.localScale = Vector3.one * 1.15f;
                SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
                if (ringSR != null) ringSR.color = new Color(1f, 0.05f, 0.0f, 0.95f);
            }
            if (smokeTransform != null)
            {
                SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
                if (smokeSR != null) smokeSR.color = new Color(1f, 0.1f, 0.0f, 0.45f);
            }
            // NOTE: Do NOT override transform.localScale here — ScaleInCoroutine handles it
        }
        else if (IsTimeDot)
        {
            ApplySpriteAndColor(normalSprite, new Color(0.1f, 0.85f, 0.3f, 1f));
        }
        else
        {
            ApplySpriteAndColor(normalSprite, ColorManager.Instance != null ? ColorManager.Instance.GetColor(ColorId) : Color.white);
            SetupColor();
        }
    }

    public void SetupColor()
    {
        if (GetLibrarySprite() != null)
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (visualCore != null)
            {
                SpriteRenderer vcSR = visualCore.GetComponent<SpriteRenderer>();
                if (vcSR != null) vcSR.color = Color.white;
            }
            return;
        }

        if (ColorManager.Instance != null)
        {
            Color c = ColorManager.Instance.GetColor(ColorId);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = c;
            }
            if (visualCore != null)
            {
                SpriteRenderer vcSR = visualCore.GetComponent<SpriteRenderer>();
                if (vcSR != null)
                {
                    vcSR.color = c;
                }
            }
        }
    }

    private void UpdateProgressCircle()
    {
        if (SelectionProgress >= 1.0f || !IsSelected)
        {
            if (progressLine != null)
            {
                progressLine.gameObject.SetActive(false);
            }
            return;
        }

        if (progressLine == null)
        {
            GameObject progressGO = new GameObject("ProgressCircle");
            progressGO.transform.SetParent(transform, false);
            progressLine = progressGO.AddComponent<LineRenderer>();
            progressLine.useWorldSpace = false;
            progressLine.sortingOrder = 10; // Draw on top
            
            if (spriteRenderer != null)
            {
                progressLine.sharedMaterial = spriteRenderer.sharedMaterial;
            }
            
            progressLine.startWidth = 0.025f;
            progressLine.endWidth = 0.025f;
        }

        progressLine.gameObject.SetActive(true);
        Color baseColor = IsSpecial ? Color.white : ColorManager.Instance.GetColor(ColorId);
        Color hdrGlowColor = new Color(baseColor.r * 3.0f, baseColor.g * 3.0f, baseColor.b * 3.0f, 1.0f);
        progressLine.startColor = hdrGlowColor;
        progressLine.endColor = hdrGlowColor;

        float radius = 0.35f;
        int segments = Mathf.Max(2, Mathf.CeilToInt(40 * SelectionProgress));
        progressLine.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float progressFraction = 0f;
            if (segments > 1)
            {
                progressFraction = ((float)i / (segments - 1)) * SelectionProgress;
            }
            
            float angle = (90f - progressFraction * 360f) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            
            progressLine.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    public void Select()
    {
        if (CurrentAnimState == AnimState.Shrinking) return;
        IsSelected = true;

        SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer activeSR = vcSR != null ? vcSR : spriteRenderer;

        if (activeSR != null)
        {
            Color c = activeSR.color;
            activeSR.color = new Color(c.r, c.g, c.b, 1.0f); // Set to full opacity
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void Deselect()
    {
        if (CurrentAnimState == AnimState.Shrinking) return;
        IsSelected = false;
        SelectionProgress = 1.0f;
        if (progressLine != null)
        {
            progressLine.gameObject.SetActive(false);
        }

        SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;

        if (GetLibrarySprite() != null)
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (vcSR != null) vcSR.color = Color.white;
        }
        else
        {
            if (IsObstacle)
            {
                Color obstacleColor = IsMetal ? new Color(0.4f, 0.4f, 0.45f, 1f) : new Color(0.7f, 0.9f, 1f, 1f);
                if (spriteRenderer != null) spriteRenderer.color = obstacleColor;
                if (vcSR != null) vcSR.color = obstacleColor;
            }
            else if (IsBomb)
            {
                Color bombColor = new Color(0.6f, 0.0f, 0.0f, 1f);
                if (spriteRenderer != null) spriteRenderer.color = bombColor;
                if (vcSR != null) vcSR.color = bombColor;
            }
            else if (IsTimeDot)
            {
                Color timeColor = new Color(0.1f, 0.85f, 0.3f, 1f);
                if (spriteRenderer != null) spriteRenderer.color = timeColor;
                if (vcSR != null) vcSR.color = timeColor;
            }
            else if (IsSpecial)
            {
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
                if (vcSR != null) vcSR.color = Color.white;
            }
            else
            {
                SetupColor();
            }
        }
        transform.localScale = Vector3.one * (IsBomb ? 0.52f : (IsBossDot ? 0.75f : 0.44f)); // Bombs/Bosses are larger, base regular scale is 0.44f
        currentVisualCoreScale = 1.0f;

        if (visualCore != null)
        {
            visualCore.localScale = Vector3.one;
        }

        if (highlightTransform != null)
        {
            highlightTransform.localScale = Vector3.one;
        }

        if (specialRing != null)
        {
            specialRing.gameObject.SetActive(false);
            specialRing.localScale = Vector3.one;
            specialRing.localRotation = Quaternion.identity;
            SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
            if (ringSR != null) ringSR.color = IsBomb ? new Color(1f, 0.05f, 0.0f, 0.95f) : Color.white;
            
            if (starSparkle != null)
            {
                starSparkle.gameObject.SetActive(false);
                starSparkle.localRotation = Quaternion.identity;
            }
        }

        if (smokeTransform != null)
        {
            smokeTransform.localScale = Vector3.one * 1.35f;
            SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
            if (smokeSR != null)
            {
                if (IsFastDot) smokeSR.color = new Color(1f, 0.5f, 0f, 0.25f);
                else if (IsObstacle) smokeSR.color = new Color(0.2f, 0.2f, 0.2f, 0.2f);
                else if (IsBomb) smokeSR.color = new Color(1f, 0.2f, 0.2f, 0.28f);
                else if (IsTimeDot) smokeSR.color = new Color(0f, 1f, 0.3f, 0.28f);
                else
                {
                    Color baseColor = IsSpecial ? Color.white : ColorManager.Instance.GetColor(ColorId);
                    smokeSR.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.22f);
                }
            }
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = DifficultyManager.Instance != null ? DifficultyManager.Instance.GravityScale : 0.5f;
        }
    }

    public void DestroyDot()
    {
        if (CurrentAnimState == AnimState.Shrinking) return;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.RegisterDotPopped(this);
        }
        
        // Disable collider immediately so it can't be selected during animation
        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        if (!gameObject.activeInHierarchy)
        {
            RecycleImmediate();
            return;
        }

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }
        animCoroutine = StartCoroutine(ShrinkAndRecycleCoroutine());
    }

    public void StopLifeCycle()
    {
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
        CurrentAnimState = AnimState.Idle;
    }

    public void MeltAndSinkDown()
    {
        if (CurrentAnimState == AnimState.Shrinking) return;

        if (!gameObject.activeInHierarchy)
        {
            RecycleImmediate();
            return;
        }

        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
        }
        animCoroutine = StartCoroutine(MeltAndSinkCoroutine());
    }

    private IEnumerator MeltAndSinkCoroutine()
    {
        CurrentAnimState = AnimState.Shrinking;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        float duration = 0.7f; // smooth melting transition

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Melt down: sink down 1.5 units and shrink scale to 0
            transform.position = startPos + Vector3.down * (t * 1.6f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Smoothly fade out main renderer and special components
            float alpha = Mathf.Lerp(1f, 0f, t);
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
            }
            if (visualCore != null)
            {
                SpriteRenderer vcSR = visualCore.GetComponent<SpriteRenderer>();
                if (vcSR != null)
                {
                    Color c = vcSR.color;
                    vcSR.color = new Color(c.r, c.g, c.b, alpha);
                }
            }
            if (specialRing != null)
            {
                SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
                if (ringSR != null) ringSR.color = new Color(ringSR.color.r, ringSR.color.g, ringSR.color.b, alpha);
            }
            if (smokeTransform != null)
            {
                SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
                if (smokeSR != null) smokeSR.color = new Color(smokeSR.color.r, smokeSR.color.g, smokeSR.color.b, alpha * 0.4f);
            }

            yield return null;
        }

        RecycleImmediate();
    }

    private void OnEnable()
    {
        Debug.Log($"[Dot Enable] OnEnable called for {gameObject.name} | Type: {Type} | Position: {transform.position}");
    }

    private void OnDisable()
    {
        Debug.Log($"[Dot Disable] OnDisable called for {gameObject.name} | Type: {Type} | Position: {transform.position}\n{System.Environment.StackTrace}");
    }

    public void RecycleImmediate()
    {
        Debug.Log($"[Dot Recycle] RecycleImmediate called for {gameObject.name} | Type: {Type} | Position: {transform.position}\n{System.Environment.StackTrace}");
        SelectionProgress = 1.0f;
        if (progressLine != null)
        {
            progressLine.gameObject.SetActive(false);
        }
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
        CurrentAnimState = AnimState.Idle;

        if (DotSpawner.Instance != null)
        {
            DotSpawner.Instance.DespawnDot(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ShrinkAndRecycleCoroutine()
    {
        CurrentAnimState = AnimState.Shrinking;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.15f; // Fast, juice pop animation

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        RecycleImmediate();
    }

    private IEnumerator ScaleInCoroutine(float targetScale)
    {
        float elapsed = 0f;
        float duration = 0.28f; // Smooth scale-up time
        
        while (elapsed < duration)
        {
            // Guard: if object was deactivated mid-coroutine, bail out immediately
            if (!gameObject.activeInHierarchy)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Smooth custom curve for popping effect: starts fast, eases out
            float scaleMultiplier = Mathf.Sin(t * Mathf.PI * 0.5f); 
            transform.localScale = Vector3.one * (targetScale * scaleMultiplier);
            yield return null;
        }
        
        transform.localScale = Vector3.one * targetScale;
        animCoroutine = null;
    }

    private Sprite GetLibrarySprite()
    {
        if (DotChainRushLibrary.Instance == null) return null;
        return DotChainRushLibrary.Instance.GetTopSprite((TopTipi)Type);
    }

    private void ApplySpriteAndColor(Sprite fallbackSprite, Color fallbackColor)
    {
        Sprite libSprite = GetLibrarySprite();
        SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
        
        if (libSprite != null)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = libSprite;
                spriteRenderer.color = Color.white;
            }
            if (vcSR != null)
            {
                vcSR.sprite = libSprite;
                vcSR.color = Color.white;
            }
        }
        else
        {
            // Use fallback sprite if provided; if not, keep whatever sprite is already set
            // (prevents dot becoming invisible when no sprite is assigned in Inspector)
            if (spriteRenderer != null)
            {
                if (fallbackSprite != null) spriteRenderer.sprite = fallbackSprite;
                spriteRenderer.color = fallbackColor;
            }
            if (vcSR != null)
            {
                if (fallbackSprite != null) vcSR.sprite = fallbackSprite;
                vcSR.color = fallbackColor;
            }
        }
    }
}
