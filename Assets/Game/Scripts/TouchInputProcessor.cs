using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchInputProcessor : MonoBehaviour
{
    public static TouchInputProcessor Instance { get; private set; }

    public static event Action<Vector3, Dot> OnInputDown;
    public static event Action<Vector3, Dot> OnInputDragged;
    public static event Action OnInputUp;

    [SerializeField] private LayerMask dotLayerMask;
    private Camera mainCamera;
    private bool isPressing = false;
    private bool wasTouch = false;

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
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            if (isPressing)
            {
                isPressing = false;
                OnInputUp?.Invoke();
            }
            return;
        }

        Vector2 screenPos = Vector2.zero;
        bool inputDown = false;
        bool inputHeld = false;
        bool inputUp = false;

        // Support for Touchscreen (Input System)
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];
            screenPos = touch.position.ReadValue();
            wasTouch = true;
            
            if (touch.press.wasPressedThisFrame)
            {
                inputDown = true;
                isPressing = true;
            }
            else if (touch.press.wasReleasedThisFrame)
            {
                inputUp = true;
                isPressing = false;
            }
            else if (touch.press.isPressed)
            {
                inputHeld = true;
            }
        }
        // Support for Mouse (Input System fallback)
        else if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
            wasTouch = false;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                inputDown = true;
                isPressing = true;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                inputUp = true;
                isPressing = false;
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                inputHeld = true;
            }
        }

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;

        if (inputDown)
        {
            Dot dot = GetDotAtPosition(worldPos);
            OnInputDown?.Invoke(worldPos, dot);
        }
        else if (inputHeld && isPressing)
        {
            Dot dot = GetDotAtPosition(worldPos);
            OnInputDragged?.Invoke(worldPos, dot);
        }
        else if (inputUp)
        {
            OnInputUp?.Invoke();
        }
    }

    private Dot GetDotAtPosition(Vector2 position)
    {
        // Use OverlapCircle instead of OverlapPoint to add touch tolerance.
        // Dots are scaled to ~0.52 with collider radius 0.5 → effective radius ~0.26.
        // A touch radius of 0.15 gives a forgiving hit area matching visual size.
        float touchRadius = 0.15f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, touchRadius, dotLayerMask);

        // Pick the closest dot to the touch point
        Dot closestDot = null;
        float closestDist = float.MaxValue;
        foreach (var hit in hits)
        {
            Dot dot = hit.GetComponent<Dot>();
            if (dot != null && !dot.IsObstacle)
            {
                float dist = Vector2.Distance(position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestDot = dot;
                }
            }
        }
        return closestDot;
    }
}
