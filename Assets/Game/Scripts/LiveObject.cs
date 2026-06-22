using UnityEngine;
using System.Collections;

public enum LiveObjectType
{
    ChaosCreature,
    Collector,
    Mimic,
    VirusCell
}

public class LiveObject : MonoBehaviour
{
    public LiveObjectType type = LiveObjectType.ChaosCreature;
    public float speed = 1.5f;
    public float wanderRadius = 3.0f;
    public float actionInterval = 5.0f;

    private Vector2 targetPosition;
    private float actionTimer = 0f;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ChooseNewTargetPosition();
        
        if (sr != null)
        {
            // Custom colors for live entities
            switch (type)
            {
                case LiveObjectType.ChaosCreature: sr.color = new Color(0.9f, 0.2f, 0.6f); break;
                case LiveObjectType.Collector: sr.color = new Color(0.1f, 0.5f, 0.9f); break;
                case LiveObjectType.Mimic: sr.color = new Color(0.8f, 0.6f, 0.1f); break;
                case LiveObjectType.VirusCell: sr.color = new Color(0.2f, 0.8f, 0.3f); break;
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // Wander movement
        Vector2 currentPos = transform.position;
        if (Vector2.Distance(currentPos, targetPosition) < 0.2f)
        {
            ChooseNewTargetPosition();
        }

        Vector2 movement = (targetPosition - currentPos).normalized * speed * Time.deltaTime;
        transform.Translate(movement);

        // Perform periodic action (e.g. Virus cell multiplying)
        actionTimer += Time.deltaTime;
        if (actionTimer >= actionInterval)
        {
            actionTimer = 0f;
            ExecuteAction();
        }
    }

    private void ChooseNewTargetPosition()
    {
        // Wander within the gameplay boundary boundaries
        targetPosition = new Vector2(
            Random.Range(-2.2f, 2.2f),
            Random.Range(-3.0f, 3.0f)
        );
    }

    private void ExecuteAction()
    {
        if (type == LiveObjectType.VirusCell)
        {
            // Multiply! Spawn another virus cell nearby
            GameObject child = Instantiate(gameObject, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f), Quaternion.identity);
            child.name = "VirusCell_Child";
            Debug.Log("[LiveObject] Virus cell multiplied!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Dot dot = collision.gameObject.GetComponent<Dot>();
        if (dot == null) return;

        switch (type)
        {
            case LiveObjectType.ChaosCreature:
                // Push ball with an impulse force
                Rigidbody2D dotRb = dot.GetComponent<Rigidbody2D>();
                if (dotRb != null)
                {
                    Vector2 pushDir = (dot.transform.position - transform.position).normalized;
                    dotRb.AddForce(pushDir * 6f, ForceMode2D.Impulse);
                    Debug.Log("[LiveObject] Chaos Creature pushed dot: " + dot.name);
                }
                break;

            case LiveObjectType.Collector:
                // Steal useful special ball
                if (dot.IsSpecial || dot.IsBomb || dot.Type == DotType.Zaman)
                {
                    Debug.Log("[LiveObject] Collector stole special ball: " + dot.name);
                    if (ComboManager.Instance != null)
                    {
                        ComboManager.Instance.SpawnExplosion(dot.transform.position, Color.blue, 10);
                    }
                    dot.RecycleImmediate();
                }
                break;

            case LiveObjectType.Mimic:
                // Act like a TreasureBox but triggers negative effects on collision
                TriggerMimicBite();
                Destroy(gameObject);
                break;
        }
    }

    private void TriggerMimicBite()
    {
        Debug.Log("[LiveObject] Player bit by a Mimic!");
        
        // Deduct time!
        RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
        if (rtc != null) rtc.CurrentTime -= 10f;
        CircularTimer ct = FindAnyObjectByType<CircularTimer>();
        if (ct != null) ct.CurrentTime -= 10f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowComboFeedback("MIMIC ATTACK!", "-10 Seconds!", "", Color.red);
        }
    }
}
