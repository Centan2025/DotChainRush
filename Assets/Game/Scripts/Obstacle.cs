using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public ObstacleConfig config;
    
    public string id;
    public ObstacleType type;
    public int health;
    public int armor;
    public ObstacleElement element;
    public ObstacleMovement movement;
    public ObstacleInteraction interaction;
    public ObstacleRewardType rewardType;
    public int rewardAmount;

    [Header("Chain Links")]
    public List<Obstacle> linkedObstacles = new List<Obstacle>();

    [Header("Shield Configuration")]
    public Vector2 shieldDirection = Vector2.up; // blocks damage coming from this direction

    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;
    private float virusSpreadTimer = 0f;
    private float virusSpreadInterval = 8.0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (config != null)
        {
            InitializeFromConfig(config);
        }
    }

    public void InitializeFromConfig(ObstacleConfig cfg)
    {
        config = cfg;
        id = cfg.obstacleId;
        type = cfg.type;
        health = cfg.maxHealth;
        armor = cfg.armor;
        element = cfg.element;
        movement = cfg.movement;
        interaction = cfg.interaction;
        rewardType = cfg.rewardType;
        rewardAmount = cfg.rewardAmount;

        if (sr != null && cfg.visualSprite != null)
        {
            sr.sprite = cfg.visualSprite;
        }

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (sr == null) return;

        // Custom visuals based on type
        switch (type)
        {
            case ObstacleType.NormalBlock:
                sr.color = Color.white;
                break;
            case ObstacleType.MetalBlock:
                sr.color = Color.gray;
                break;
            case ObstacleType.IceBlock:
                sr.color = health > 1 ? new Color(0.5f, 0.8f, 1f, 1f) : new Color(0.7f, 0.9f, 1f, 0.7f); // cracked is more transparent
                break;
            case ObstacleType.ShieldBlock:
                sr.color = new Color(0.8f, 0.4f, 0.9f);
                break;
            case ObstacleType.ChainBlock:
                sr.color = new Color(0.9f, 0.7f, 0.1f);
                break;
            case ObstacleType.MirrorBlock:
                sr.color = new Color(0.2f, 0.9f, 0.8f);
                break;
            case ObstacleType.VoidBlock:
                sr.color = Color.black;
                break;
            case ObstacleType.TimeBlock:
                sr.color = Color.green;
                break;
            case ObstacleType.VirusBlock:
                sr.color = new Color(0.1f, 0.7f, 0.2f);
                break;
            case ObstacleType.CorruptedBlock:
                sr.color = new Color(0.5f, 0f, 0.5f);
                break;
        }
    }

    private void Update()
    {
        // Handle Virus Spreading behavior
        if (type == ObstacleType.VirusBlock && GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            virusSpreadTimer += Time.deltaTime;
            if (virusSpreadTimer >= virusSpreadInterval)
            {
                virusSpreadTimer = 0f;
                SpreadVirus();
            }
        }
    }

    public void OnAdjacentDotPopped(Dot poppedDot)
    {
        Vector2 directionToPop = (poppedDot.transform.position - transform.position).normalized;
        TakeDamage(1, poppedDot.Type, directionToPop);
    }

    public void TakeDamage(int amount, DotType damageSourceType, Vector2 direction)
    {
        if (health <= 0) return;

        // 1) Shield check
        if (type == ObstacleType.ShieldBlock)
        {
            float dotProduct = Vector2.Dot(direction, shieldDirection);
            if (dotProduct > 0.5f) // popped from shielded direction
            {
                Debug.Log("[Obstacle] Damage blocked by Shield!");
                if (ComboManager.Instance != null)
                {
                    ComboManager.Instance.TriggerCameraShake(0.05f, 0.1f);
                }
                return;
            }
        }

        // 2) Metal block heavy check
        if (type == ObstacleType.MetalBlock)
        {
            bool isHeavy = (damageSourceType == DotType.AgirTop || damageSourceType == DotType.ElitAgir);
            if (!isHeavy)
            {
                Debug.Log("[Obstacle] Metal Block resists normal damage!");
                return;
            }
        }

        // 3) Ice block check
        if (type == ObstacleType.IceBlock)
        {
            bool isIceMelter = (damageSourceType == DotType.Buz || damageSourceType == DotType.BuzElement || damageSourceType == DotType.Ates);
            if (isIceMelter)
            {
                health = 0; // instantly melts
            }
        }

        // Calculate actual damage with armor
        int actualDamage = Mathf.Max(0, amount - armor);
        if (damageSourceType == DotType.Kritik) actualDamage *= 2; // Critical damage

        health -= actualDamage;
        UpdateVisuals();

        Debug.Log($"[Obstacle] {type} took {actualDamage} damage from {damageSourceType}. Health remaining: {health}");

        // Chain propagation
        if (type == ObstacleType.ChainBlock && actualDamage > 0)
        {
            foreach (var linked in linkedObstacles)
            {
                if (linked != null && linked != this)
                {
                    linked.TakeDamageDirect(actualDamage);
                }
            }
        }

        if (health <= 0)
        {
            DestroyObstacle();
        }
    }

    public void TakeDamageDirect(int amount)
    {
        if (health <= 0) return;
        health -= amount;
        UpdateVisuals();
        if (health <= 0)
        {
            DestroyObstacle();
        }
    }

    private void SpreadVirus()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        foreach (var col in colliders)
        {
            Dot dot = col.GetComponent<Dot>();
            if (dot != null && !dot.IsObstacle && dot.gameObject.activeInHierarchy)
            {
                // Convert normal dot to virus
                Debug.Log($"[Obstacle] Virus spread: converting dot {dot.name} to Virus");
                dot.Init(dot.ColorId, DotType.Virus, d => { if (DotSpawner.Instance != null) DotSpawner.Instance.DespawnDot(d); });
                break; // only spread to one dot at a time
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Dot dot = collision.gameObject.GetComponent<Dot>();
        if (dot == null) return;

        // 1) Void Block consumes balls
        if (type == ObstacleType.VoidBlock)
        {
            if (dot.Type == DotType.Bosluk || dot.Type == DotType.VoidRainbow)
            {
                TakeDamageDirect(1); // destroyed by void balls
            }
            else
            {
                Debug.Log("[Obstacle] Void Block consumed ball: " + dot.gameObject.name);
                dot.RecycleImmediate();
            }
        }

        // 2) Mirror Block reflects trajectory
        if (type == ObstacleType.MirrorBlock)
        {
            Rigidbody2D dotRb = dot.GetComponent<Rigidbody2D>();
            if (dotRb != null)
            {
                Vector2 normal = collision.contacts[0].normal;
                Vector2 reflected = Vector2.Reflect(dotRb.linearVelocity, normal);
                dotRb.linearVelocity = reflected * 1.2f; // speed up slightly on reflection
                Debug.Log("[Obstacle] Mirror reflected ball trajectory");

                if (dot.Type == DotType.Ayna)
                {
                    TakeDamageDirect(1);
                }
            }
        }
    }

    private void DestroyObstacle()
    {
        Debug.Log($"[Obstacle] Destroyed: {type}");
        
        // Spawn particles or explosions
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.SpawnExplosion(transform.position, sr != null ? sr.color : Color.cyan, 25);
            ComboManager.Instance.TriggerCameraShake(0.15f, 0.2f);
        }

        // Apply rewards
        ApplyRewards();

        // Corrupted block rule changes
        if (type == ObstacleType.CorruptedBlock)
        {
            TriggerCorruptionRule();
        }

        // Clean up from spawner list if it is registered as a dot
        Dot dot = GetComponent<Dot>();
        if (dot != null && DotSpawner.Instance != null)
        {
            DotSpawner.Instance.DespawnDot(dot);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ApplyRewards()
    {
        if (rewardType == ObstacleRewardType.None) return;

        if (rewardType == ObstacleRewardType.Points && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(rewardAmount);
        }
        else if (rewardType == ObstacleRewardType.ExtraTime)
        {
            // Add time to active timer
            RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
            if (rtc != null) rtc.CurrentTime += rewardAmount;
            
            CircularTimer ct = FindAnyObjectByType<CircularTimer>();
            if (ct != null) ct.CurrentTime += rewardAmount;

            Debug.Log($"[Obstacle] Awarded +{rewardAmount} seconds!");
        }
        else if (rewardType == ObstacleRewardType.Crystal)
        {
            int crystals = SaveSystem.LoadInt("Crystals", 0);
            SaveSystem.SaveInt("Crystals", crystals + rewardAmount);
            Debug.Log($"[Obstacle] Awarded +{rewardAmount} permanent Crystals!");
        }
    }

    private void TriggerCorruptionRule()
    {
        Debug.Log("[Obstacle] Corrupted Block destroyed! Changing local rules...");
        if (BossDimensionManager.Instance != null && BossDimensionManager.Instance.IsBossLevelActive)
        {
            BossDimensionManager.Instance.InitializeBossLevel(BossDimensionManager.Instance.ActiveLevelNumber, TopTipi.Glitch);
        }
    }
}
