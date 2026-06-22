using UnityEngine;
using System.Collections;

public class MovingObstacle : MonoBehaviour
{
    public ObstacleMovement movementType = ObstacleMovement.Static;
    public float speed = 1.0f;
    public float range = 2.0f; // range for sliding
    public float cycleTime = 4.0f; // laser barrier active/inactive timing

    [Header("Teleport Settings")]
    public Transform destinationTarget;
    public Vector3 defaultDestination = Vector3.zero;

    [Header("Gravity Wall Settings")]
    public Vector2 customGravityDirection = Vector2.up;

    private Vector3 startPosition;
    private bool laserActive = true;
    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        startPosition = transform.position;

        if (movementType == ObstacleMovement.LaserBarrier)
        {
            StartCoroutine(LaserCycleCoroutine());
        }
    }

    private void Update()
    {
        switch (movementType)
        {
            case ObstacleMovement.Sliding:
                float pingPong = Mathf.PingPong(Time.time * speed, range * 2) - range;
                transform.position = startPosition + new Vector3(pingPong, 0f, 0f);
                break;
            case ObstacleMovement.Rotating:
                transform.Rotate(Vector3.forward * speed * 50f * Time.deltaTime);
                break;
        }
    }

    private IEnumerator LaserCycleCoroutine()
    {
        while (true)
        {
            // Activate laser
            laserActive = true;
            if (sr != null) sr.color = new Color(1f, 0f, 0f, 1f); // bright red laser
            if (col != null) col.enabled = true;
            yield return new WaitForSeconds(cycleTime);

            // Deactivate laser
            laserActive = false;
            if (sr != null) sr.color = new Color(1f, 0f, 0f, 0.2f); // dimmed red laser
            if (col != null) col.enabled = false;
            yield return new WaitForSeconds(cycleTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Dot dot = other.GetComponent<Dot>();
        if (dot == null) return;

        // 1) Teleport Gate behavior
        if (movementType == ObstacleMovement.TeleportGate)
        {
            Vector3 dest = destinationTarget != null ? destinationTarget.position : defaultDestination;
            dot.transform.position = dest;
            Debug.Log("[MovingObstacle] Teleported dot " + dot.name + " to " + dest);
        }

        // 2) Gravity Wall behavior
        if (movementType == ObstacleMovement.GravityWall)
        {
            Rigidbody2D rb = dot.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Temporarily reverse or change direction of gravity by applying force
                rb.linearVelocity = customGravityDirection * rb.linearVelocity.magnitude;
                rb.gravityScale = -rb.gravityScale; // Flip gravity!
                Debug.Log("[MovingObstacle] Changed gravity scale of dot " + dot.name);
            }
        }
    }
}
