using UnityEngine;

public class ScreenBoundsAdaptor : MonoBehaviour
{
    private BoxCollider2D leftWall, rightWall, topWall, bottomWall;

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

        // Disable legacy EdgeCollider2D if it exists
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider != null)
        {
            edgeCollider.enabled = false;
        }

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
        float footerHeight = 1.4f;
        float sidePadding = 0.08f;

        float leftX = -halfWidth + sidePadding;
        float rightX = halfWidth - sidePadding;
        float bottomY = -halfHeight + footerHeight;
        
        // Walls stop exactly at the visual frame top so balls don't overflow into the UI header
        float visualTopY = halfHeight - headerHeight;
        float ceilingY = visualTopY; 

        // Create 4 thick BoxColliders to prevent any physics tunneling at corners
        float thickness = 5.0f; // Extremely thick walls

        if (leftWall == null) leftWall = gameObject.AddComponent<BoxCollider2D>();
        if (rightWall == null) rightWall = gameObject.AddComponent<BoxCollider2D>();
        if (bottomWall == null) bottomWall = gameObject.AddComponent<BoxCollider2D>();
        if (topWall == null) topWall = gameObject.AddComponent<BoxCollider2D>();

        PhysicsMaterial2D mat = new PhysicsMaterial2D("ScreenBoundsPhysicsMaterial");
        mat.friction = 0f;
        mat.bounciness = 0.1f;

        leftWall.sharedMaterial = mat;
        rightWall.sharedMaterial = mat;
        bottomWall.sharedMaterial = mat;
        topWall.sharedMaterial = mat;

        float centerX = (leftX + rightX) / 2f;
        float centerY = (bottomY + ceilingY) / 2f;
        float width = rightX - leftX;
        float height = ceilingY - bottomY;

        // Left Wall
        leftWall.size = new Vector2(thickness, height + thickness * 2);
        leftWall.offset = new Vector2(leftX - thickness / 2f, centerY);

        // Right Wall
        rightWall.size = new Vector2(thickness, height + thickness * 2);
        rightWall.offset = new Vector2(rightX + thickness / 2f, centerY);

        // Bottom Wall
        bottomWall.size = new Vector2(width + thickness * 2, thickness);
        bottomWall.offset = new Vector2(centerX, bottomY - thickness / 2f);

        // Top Ceiling Wall
        topWall.size = new Vector2(width + thickness * 2, thickness);
        topWall.offset = new Vector2(centerX, ceilingY + thickness / 2f);

        // Dynamically adjust DotSpawner spawn limits and spawn height
        if (DotSpawner.Instance != null)
        {
            float dotRadius = 0.22f;
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
        float footerHeight = 1.4f;
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
        float dotRadius = 0.22f;
        float spawnMargin = dotRadius + 0.15f;
        float spawnY = topY - dotRadius;
        Gizmos.DrawLine(new Vector3(leftX + spawnMargin, spawnY, 0), new Vector3(rightX - spawnMargin, spawnY, 0));
    }
}

