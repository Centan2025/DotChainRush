using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class ScreenBoundsAdaptor : MonoBehaviour
{
    private void Start()
    {
        EnsurePlayAreaFrame();
        AdaptBounds();
    }

    private void EnsurePlayAreaFrame()
    {
        if (FindAnyObjectByType<PlayAreaFrame>() == null)
        {
            GameObject frameGO = new GameObject("PlayAreaFrame");
            frameGO.AddComponent<PlayAreaFrame>();
            Debug.Log("[DynamicSetup] Created missing PlayAreaFrame object automatically.");
        }
    }

    public void AdaptBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null) return;

        // Reset transform to ensure local space matches world space
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        float aspect = cam.aspect;

        float orthoHeight = cam.orthographicSize * 2f;
        float orthoWidth = orthoHeight * aspect;

        float halfWidth = orthoWidth / 2f;
        float halfHeight = orthoHeight / 2f;

        float headerHeight = 1.2f;
        float footerHeight = 0.6f;
        float sidePadding = 0.08f;

        float leftX = -halfWidth + sidePadding;
        float rightX = halfWidth - sidePadding;
        float bottomY = -halfHeight + footerHeight;
        // Walls extend only slightly above the visual frame top — enough to contain
        // dots being spawned but not so far that dots pile up into the header.
        float visualTopY = halfHeight - headerHeight;
        float wallTopY = halfHeight + 4.0f; // Make walls extend much higher above the screen to prevent dots from spilling over the sides

        // U-shape: left wall up, across bottom, right wall up — top is OPEN for spawning
        // Dots are contained by the three walls; danger system handles game-over before overflow
        Vector2[] points = new Vector2[]
        {
            new Vector2(leftX, wallTopY),
            new Vector2(leftX, bottomY),
            new Vector2(rightX, bottomY),
            new Vector2(rightX, wallTopY)
        };
        edgeCollider.points = points;

        // Thick edge radius prevents dots from tunneling through thin walls under pressure
        edgeCollider.edgeRadius = 0.05f;

        // Zero-friction material so dots slide down walls smoothly
        if (edgeCollider.sharedMaterial == null || edgeCollider.sharedMaterial.friction != 0f)
        {
            PhysicsMaterial2D mat = new PhysicsMaterial2D("ScreenBoundsPhysicsMaterial");
            mat.friction = 0f;
            mat.bounciness = 0.1f;
            edgeCollider.sharedMaterial = mat;
        }

        // Dynamically adjust DotSpawner spawn limits and spawn height
        if (DotSpawner.Instance != null)
        {
            float dotRadius = 0.26f;
            float spawnMargin = dotRadius + 0.15f;
            DotSpawner.Instance.SetSpawnRange(leftX + spawnMargin, rightX - spawnMargin);

            // Spawn at the visual frame top — dots appear just inside the visible area
            DotSpawner.Instance.SpawnY = visualTopY - dotRadius;
        }
    }

    private void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float aspect = cam.aspect;
        float orthoHeight = cam.orthographicSize * 2f;
        float orthoWidth = orthoHeight * aspect;

        float halfWidth = orthoWidth / 2f;
        float halfHeight = orthoHeight / 2f;

        float headerHeight = 1.2f;
        float footerHeight = 0.6f;
        float sidePadding = 0.08f;

        float leftX = -halfWidth + sidePadding;
        float rightX = halfWidth - sidePadding;
        float bottomY = -halfHeight + footerHeight;
        float topY = halfHeight - headerHeight;

        // Draw Play Area Visual Bounds
        Gizmos.color = Color.cyan;
        Vector3 topLeft = new Vector3(leftX, topY, 0);
        Vector3 topRight = new Vector3(rightX, topY, 0);
        Vector3 bottomLeft = new Vector3(leftX, bottomY, 0);
        Vector3 bottomRight = new Vector3(rightX, bottomY, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // Draw Spawn Line
        Gizmos.color = Color.yellow;
        float dotRadius = 0.26f;
        float spawnMargin = dotRadius + 0.15f;
        float spawnY = topY - dotRadius;
        Gizmos.DrawLine(new Vector3(leftX + spawnMargin, spawnY, 0), new Vector3(rightX - spawnMargin, spawnY, 0));
    }
}
