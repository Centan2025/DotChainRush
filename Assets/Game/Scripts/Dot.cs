using System;
using System.Collections;
using UnityEngine;

public enum DotType
{
    Normal,
    Rainbow,
    Speed,
    Bomb,
    Frozen,
    Metal,
    Time
}

public class Dot : MonoBehaviour
{
    public enum AnimState { Idle, Shrinking }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite specialSprite;
    [SerializeField] private Sprite obstacleSprite;

    public int ColorId { get; private set; }
    public bool IsSelected { get; private set; }
    public DotType Type { get; private set; } = DotType.Normal;

    public bool IsSpecial => Type == DotType.Rainbow;
    public bool IsObstacle => Type == DotType.Metal || Type == DotType.Frozen;
    public bool IsFastDot => Type == DotType.Speed;
    public bool IsBomb => Type == DotType.Bomb;
    public bool IsFrozen => Type == DotType.Frozen;
    public bool IsMetal => Type == DotType.Metal;
    public bool IsTimeDot => Type == DotType.Time;

    public AnimState CurrentAnimState { get; private set; } = AnimState.Idle;

    private Coroutine animCoroutine;
    private Rigidbody2D rb;
    private Transform specialRing;
    private Transform smokeTransform;
    private Transform visualCore;
    private Transform highlightTransform;
    private float currentVisualCoreScale = 1.0f;

    private void Awake()
    {
        visualCore = transform.Find("VisualCore");
        if (spriteRenderer == null)
        {
            if (visualCore != null) spriteRenderer = visualCore.GetComponent<SpriteRenderer>();
            else spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (circleCollider == null) circleCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        specialRing = transform.Find("SpecialRing");
        smokeTransform = transform.Find("Smoke");
        highlightTransform = transform.Find("Highlight");
    }

    private void Update()
    {
        // Offscreen check to prevent leaks (adapts dynamically to camera orthographic size)
        float offscreenThreshold = -6f;
        if (Camera.main != null)
        {
            offscreenThreshold = -Camera.main.orthographicSize - 2.0f;
        }

        if (transform.position.y < offscreenThreshold)
        {
            RecycleImmediate();
            return;
        }

        // Juiciness: smooth shrunken visual core + concentric neon halo border + cloud-like halo radiating outwards
        if (IsSelected && CurrentAnimState == AnimState.Idle)
        {
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
                // Breathe the outer border scale slightly
                float ringPulse = 1.05f + 0.08f * Mathf.Sin(Time.time * 10f);
                specialRing.localScale = Vector3.one * ringPulse;
                
                SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
                if (ringSR != null)
                {
                    // Ring glows bright in bubble's own color (slightly intensified)
                    ringSR.color = new Color(baseColor.r * 1.3f, baseColor.g * 1.3f, baseColor.b * 1.3f, 1.0f);
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
        else if (CurrentAnimState == AnimState.Idle)
        {
            // Soft breathing for normal unselected dots' cloud halos
            if (smokeTransform != null)
            {
                float breathe = 1.45f + 0.1f * Mathf.Sin(Time.time * 3f + GetHashCode() % 5);
                smokeTransform.localScale = Vector3.one * breathe;
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

        // Start scale at zero for a smooth pop-in reveal effect when spawning
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity; // Reset rotation from recycled Speed/Rainbow dots
        currentVisualCoreScale = 1.0f;

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
        animCoroutine = StartCoroutine(ScaleInCoroutine(type == DotType.Bomb ? 0.62f : 0.52f));

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
            specialRing.gameObject.SetActive(IsSpecial || IsBomb);
            specialRing.localScale = Vector3.one;
            SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
            if (ringSR != null) ringSR.color = Color.white;
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

        if (IsObstacle)
        {
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            Color obstacleColor = IsMetal ? new Color(0.4f, 0.4f, 0.45f) : new Color(0.7f, 0.9f, 1f);
            if (spriteRenderer != null) { spriteRenderer.sprite = obstacleSprite; spriteRenderer.color = obstacleColor; }
            if (vcSR != null) { vcSR.sprite = obstacleSprite; vcSR.color = obstacleColor; }
        }
        else if (IsSpecial)
        {
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            if (spriteRenderer != null) { spriteRenderer.sprite = specialSprite; spriteRenderer.color = Color.white; }
            if (vcSR != null) { vcSR.sprite = specialSprite; vcSR.color = Color.white; }
        }
        else if (IsBomb)
        {
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            Color bombColor = new Color(0.6f, 0.0f, 0.0f);
            if (spriteRenderer != null) { spriteRenderer.sprite = normalSprite; spriteRenderer.color = bombColor; }
            if (vcSR != null) { vcSR.sprite = normalSprite; vcSR.color = bombColor; }
            
            if (specialRing != null)
            {
                specialRing.gameObject.SetActive(true);
                specialRing.localScale = Vector3.one * 1.15f;
                SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
                if (ringSR != null) ringSR.color = new Color(1f, 0.05f, 0.0f, 0.95f);
            }
            if (smokeTransform != null)
            {
                SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
                if (smokeSR != null) smokeSR.color = new Color(1f, 0.1f, 0.0f, 0.45f);
            }
            transform.localScale = Vector3.one * 0.62f;
        }
        else if (IsTimeDot)
        {
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            Color timeColor = new Color(0.1f, 0.85f, 0.3f, 1f);
            if (spriteRenderer != null) { spriteRenderer.sprite = normalSprite; spriteRenderer.color = timeColor; }
            if (vcSR != null) { vcSR.sprite = normalSprite; vcSR.color = timeColor; }
        }
        else
        {
            SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;
            if (spriteRenderer != null && normalSprite != null) spriteRenderer.sprite = normalSprite;
            if (vcSR != null && normalSprite != null) vcSR.sprite = normalSprite;
            SetupColor();
        }
    }

    public void SetupColor()
    {
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

        SpriteRenderer vcSR = visualCore != null ? visualCore.GetComponent<SpriteRenderer>() : null;

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
        transform.localScale = Vector3.one * (IsBomb ? 0.62f : 0.52f); // Bombs are larger, base regular scale is 0.52f
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
            specialRing.gameObject.SetActive(IsSpecial || IsBomb);
            specialRing.localScale = Vector3.one;
            SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
            if (ringSR != null) ringSR.color = IsBomb ? new Color(1f, 0.05f, 0.0f, 0.95f) : Color.white;
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

    private void RecycleImmediate()
    {
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
}
