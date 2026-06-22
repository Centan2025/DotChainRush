using UnityEngine;

public enum PhysicsObjectType
{
    BouncePad,
    MagnetZone,
    WindZone,
    FireZone,
    SlowZone,
    ChaosZone
}

public class PhysicsObject : MonoBehaviour
{
    public PhysicsObjectType type = PhysicsObjectType.BouncePad;
    public float forceMagnitude = 10f;
    public Vector2 direction = Vector2.up; // used for wind or bounce direction
    public float radius = 2.0f; // used for attraction radius

    private void OnTriggerStay2D(Collider2D other)
    {
        Dot dot = other.GetComponent<Dot>();
        if (dot == null) return;

        Rigidbody2D rb = dot.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        switch (type)
        {
            case PhysicsObjectType.MagnetZone:
                Vector2 forceDir = ((Vector2)transform.position - (Vector2)dot.transform.position).normalized;
                float dist = Vector2.Distance(transform.position, dot.transform.position);
                float pull = Mathf.Clamp01(1f - (dist / radius));
                rb.AddForce(forceDir * forceMagnitude * pull * Time.deltaTime * 50f);
                break;

            case PhysicsObjectType.WindZone:
                rb.AddForce(direction.normalized * forceMagnitude * Time.deltaTime * 50f);
                break;

            case PhysicsObjectType.SlowZone:
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * forceMagnitude);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Dot dot = other.GetComponent<Dot>();
        if (dot == null) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        switch (type)
        {
            case PhysicsObjectType.BouncePad:
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, 0f) + forceMagnitude);
                    Debug.Log("[PhysicsObject] Bounced dot upward: " + dot.name);
                    if (ComboManager.Instance != null)
                    {
                        ComboManager.Instance.TriggerCameraShake(0.04f, 0.1f);
                    }
                }
                break;

            case PhysicsObjectType.FireZone:
                Debug.Log("[PhysicsObject] Fire zone burned dot: " + dot.name);
                if (ComboManager.Instance != null)
                {
                    ComboManager.Instance.SpawnExplosion(dot.transform.position, Color.red, 15);
                }
                dot.RecycleImmediate();
                break;

            case PhysicsObjectType.ChaosZone:
                ApplyChaosEffect(dot);
                break;
        }
    }

    private void ApplyChaosEffect(Dot dot)
    {
        // Randomly modify dot size, color, or type
        int rand = Random.Range(0, 3);
        if (rand == 0)
        {
            // Alter color randomly
            int randomColor = Random.Range(0, 5);
            dot.Init(randomColor, dot.Type, d => { if (DotSpawner.Instance != null) DotSpawner.Instance.DespawnDot(d); });
            Debug.Log("[PhysicsObject] Chaos Zone color changed dot: " + dot.name);
        }
        else if (rand == 1)
        {
            // Alter type randomly
            TopTipi randomSpecial = TopTipi.Gokkusagi;
            if (Random.value < 0.5f) randomSpecial = TopTipi.Bomba;
            dot.Init(dot.ColorId, (DotType)randomSpecial, d => { if (DotSpawner.Instance != null) DotSpawner.Instance.DespawnDot(d); });
            Debug.Log("[PhysicsObject] Chaos Zone type changed dot to special: " + dot.name);
        }
        else
        {
            // Alter size dynamically
            dot.transform.localScale = dot.transform.localScale * Random.Range(0.6f, 1.5f);
            Debug.Log("[PhysicsObject] Chaos Zone size mutated dot: " + dot.name);
        }
    }
}
