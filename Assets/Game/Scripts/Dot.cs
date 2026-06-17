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
        // Offscreen check to prevent leaks
        if (transform.position.y < -6f)
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

            if (spriteRenderer != null && ColorManager.Instance != null)
            {
                // Glow core dynamically in its own color with breathing glow intensity
                float corePulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 24f);
                Color glowColor = Color.Lerp(baseColor, Color.white, corePulse * 0.5f);
                spriteRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, 1f);
            }
        }
        // Dopamine rotation for Special (Rainbow) wild-card dots
        else if (IsSpecial && spriteRenderer != null && CurrentAnimState == AnimState.Idle)
        {
            transform.Rotate(Vector3.forward * -60f * Time.deltaTime);
            spriteRenderer.color = Color.white;
            
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
        else if (CurrentAnimState == AnimState.Idle)
        {
            // Soft breathing for normal unselected dots' cloud halos
            if (smokeTransform != null)
            {
                float breathe = 1.45f + 0.1f * Mathf.Sin(Time.time * 3f + GetHashCode() % 5);
                smokeTransform.localScale = Vector3.one * breathe;
            }
        }
    }

    public void Init(int colorId, DotType type, Action<Dot> onLifeEnd)
    {
        ColorId = colorId;
        IsSelected = false;
        Type = type;
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

        transform.localScale = Vector3.one * 0.62f; // Slightly larger overall base scale as requested
        currentVisualCoreScale = 1.0f;

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }

        if (visualCore == null) visualCore = transform.Find("VisualCore");
        if (visualCore != null)
        {
            visualCore.localScale = Vector3.one;
        }

        if (highlightTransform == null) highlightTransform = transform.Find("Highlight");
        if (highlightTransform != null)
        {
            highlightTransform.localScale = Vector3.one;
        }

        if (specialRing == null) specialRing = transform.Find("SpecialRing");
        if (specialRing != null)
        {
            specialRing.gameObject.SetActive(IsSpecial);
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
                    smokeSR.color = new Color(1f, 0.25f, 0f, 0.85f); // Neon Fire/Orange smoke
                }
                else if (IsObstacle)
                {
                    if (IsMetal)
                    {
                        smokeSR.color = new Color(0.3f, 0.3f, 0.35f, 0.6f); // Heavy metallic smoke
                    }
                    else
                    {
                        smokeSR.color = new Color(0.4f, 0.7f, 1f, 0.6f); // Frozen blue smoke
                    }
                }
                else if (IsBomb)
                {
                    smokeSR.color = new Color(1f, 0.1f, 0.1f, 0.8f); // Bomb red glow
                }
                else if (IsTimeDot)
                {
                    smokeSR.color = new Color(0f, 1f, 0.3f, 0.8f); // Time green glow
                }
                else
                {
                    Color baseColor = IsSpecial ? Color.white : ColorManager.Instance.GetColor(ColorId);
                    smokeSR.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.22f); // Subtler soft cloud-like halo
                }
            }
            smokeTransform.localScale = Vector3.one * 1.55f; // Scaled up to extend beautifully outside the bubble edge
            smokeTransform.gameObject.SetActive(true);
        }

        if (IsObstacle)
        {
            if (spriteRenderer != null && obstacleSprite != null)
            {
                spriteRenderer.sprite = obstacleSprite;
                spriteRenderer.color = IsMetal ? new Color(0.4f, 0.4f, 0.45f) : new Color(0.7f, 0.9f, 1f); // Metal is dark grey, Frozen is light blue
            }
        }
        else if (IsSpecial)
        {
            if (spriteRenderer != null && specialSprite != null)
            {
                spriteRenderer.sprite = specialSprite;
                spriteRenderer.color = Color.white;
            }
        }
        else
        {
            if (spriteRenderer != null && normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
            SetupColor();
        }
    }

    public void SetupColor()
    {
        if (spriteRenderer != null && ColorManager.Instance != null)
        {
            spriteRenderer.color = ColorManager.Instance.GetColor(ColorId);
        }
    }

    public void Select()
    {
        if (CurrentAnimState == AnimState.Shrinking) return;
        IsSelected = true;
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, 1.0f); // Set to full opacity (Update will animate it)
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
        if (IsObstacle)
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
        }
        else if (!IsSpecial)
        {
            SetupColor();
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        transform.localScale = Vector3.one * 0.62f; // Restore base scale
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
            specialRing.gameObject.SetActive(IsSpecial);
            specialRing.localScale = Vector3.one;
            SpriteRenderer ringSR = specialRing.GetComponent<SpriteRenderer>();
            if (ringSR != null) ringSR.color = Color.white;
        }

        if (smokeTransform != null)
        {
            smokeTransform.localScale = Vector3.one * 1.55f;
            SpriteRenderer smokeSR = smokeTransform.GetComponent<SpriteRenderer>();
            if (smokeSR != null)
            {
                if (IsFastDot) smokeSR.color = new Color(1f, 0.25f, 0f, 0.85f);
                else if (IsObstacle) smokeSR.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
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
}
