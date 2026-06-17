using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class ScreenBoundsAdaptor : MonoBehaviour
{
    private void Start()
    {
        AdaptBounds();
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
        if (aspect > 0.6f)
        {
            aspect = 9f / 16f; // Lock physics bounds to 9:16 portrait ratio even on wide/landscape editor views
        }

        float orthoHeight = cam.orthographicSize * 2f;
        float orthoWidth = orthoHeight * aspect;

        float halfWidth = orthoWidth / 2f;
        float halfHeight = orthoHeight / 2f;

        // Floor and walls are aligned exactly with screen edges to eliminate empty space
        float bottomPadding = 0f;
        float sidePadding = 0f;

        float leftX = -halfWidth + sidePadding;
        float rightX = halfWidth - sidePadding;
        float bottomY = -halfHeight + bottomPadding;
        float topY = halfHeight + 2f; // Extend walls above screen view

        Vector2[] points = new Vector2[]
        {
            new Vector2(leftX, topY),
            new Vector2(leftX, bottomY),
            new Vector2(rightX, bottomY),
            new Vector2(rightX, topY)
        };
        edgeCollider.points = points;

        // Apply dynamic zero-friction material to prevent dots from getting stuck on walls
        if (edgeCollider.sharedMaterial == null || edgeCollider.sharedMaterial.friction != 0f)
        {
            PhysicsMaterial2D mat = new PhysicsMaterial2D("ScreenBoundsPhysicsMaterial");
            mat.friction = 0f;
            mat.bounciness = 0.15f;
            edgeCollider.sharedMaterial = mat;
        }

        // Dynamically adjust DotSpawner spawn limits to fit the screen width
        if (DotSpawner.Instance != null)
        {
            float dotRadius = 0.35f; // Dot scale is 0.7f
            float spawnMargin = dotRadius + 0.1f;
            DotSpawner.Instance.SetSpawnRange(leftX + spawnMargin, rightX - spawnMargin);
        }
    }
}
