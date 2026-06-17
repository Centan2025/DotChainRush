using System.Collections.Generic;
using UnityEngine;

public class DotSpawner : MonoBehaviour
{
    public static DotSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private float spawnY = 5f;
    [SerializeField] private float minX = -2.2f;
    [SerializeField] private float maxX = 2.2f;

    private float spawnTimer = 0f;
    private float dangerTimer = 0f;
    private readonly List<Dot> activeDots = new List<Dot>();

    public List<Dot> ActiveDots => activeDots;

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
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // Fetch dynamic interval from DifficultyManager (Fever mode scales it in GameManager/Time.timeScale)
        float interval = DifficultyManager.Instance != null 
            ? DifficultyManager.Instance.SpawnInterval 
            : 0.7f;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= interval)
        {
            spawnTimer = 0f;
            SpawnDot();
        }

        // Find the highest settled dot Y position (ignoring newly spawned dots at the very top Y >= 4.2)
        float highestSettledY = -99f;
        for (int i = 0; i < activeDots.Count; i++)
        {
            Dot dot = activeDots[i];
            if (dot != null && dot.transform.position.y < 4.2f)
            {
                Rigidbody2D dotRb = dot.GetComponent<Rigidbody2D>();
                if (dotRb != null && dotRb.linearVelocity.magnitude < 0.15f)
                {
                    if (dot.transform.position.y > highestSettledY)
                    {
                        highestSettledY = dot.transform.position.y;
                    }
                }
            }
        }

        // Interpolate camera background color based on stack height
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            float startY = 1.0f;
            float endY = 3.8f;
            float dangerT = Mathf.Clamp01((highestSettledY - startY) / (endY - startY));
            Color normalBg = new Color(0.12f, 0.12f, 0.16f);
            Color warningBg = new Color(0.32f, 0.06f, 0.08f); // Deep crimson red alert color
            mainCam.backgroundColor = Color.Lerp(normalBg, warningBg, dangerT);
        }

        // Determine danger phase
        if (highestSettledY >= 3.8f) // RED (Critical)
        {
            dangerTimer += Time.deltaTime;
            float remaining = Mathf.Max(0f, 3.0f - dangerTimer);
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetDangerActive(true, $"DANGER! {remaining:F1}s");
            }
            
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.TriggerCameraShake(0.08f, 0.05f);
            }
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDangerWarningSound(true); // Fast tick
            }

            if (dangerTimer >= 3.0f)
            {
                GameManager.Instance.GameOver();
                dangerTimer = 0f;
            }
        }
        else if (highestSettledY >= 2.8f) // YELLOW (Warning)
        {
            dangerTimer = 0f; // Reset critical timer, but keep warning visual
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetDangerActive(true, "WARNING!");
            }
            
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.TriggerCameraShake(0.02f, 0.05f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDangerWarningSound(false); // Slow tick
            }
        }
        else // GREEN (Normal)
        {
            dangerTimer = 0f;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetDangerActive(false);
            }
        }
    }

    public void InitializeSpawner()
    {
        ClearAll();
        spawnTimer = 0f;
        dangerTimer = 0f;
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResetDifficulty();
        }
    }

    public void ClearAll()
    {
        for (int i = activeDots.Count - 1; i >= 0; i--)
        {
            Dot dot = activeDots[i];
            if (dot != null)
            {
                dot.StopLifeCycle();
                if (ObjectPool.Instance != null)
                {
                    ObjectPool.Instance.ReturnToPool(dot.gameObject);
                }
                else
                {
                    Destroy(dot.gameObject);
                }
            }
        }
        activeDots.Clear();
    }

    public GameObject SpawnDot()
    {
        if (ObjectPool.Instance == null) return null;

        // Discrete columns to prevent overlapping physics at spawn time
        int columnCount = 5;
        int randomCol = Random.Range(0, columnCount);
        float step = (columnCount > 1) ? (maxX - minX) / (columnCount - 1) : 0f;
        float spawnX = minX + randomCol * step;
        
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        GameObject dotGO = ObjectPool.Instance.Get();
        dotGO.transform.position = spawnPos;
        dotGO.transform.rotation = Quaternion.identity;

        Dot dot = dotGO.GetComponent<Dot>();
        if (dot == null)
        {
            dot = dotGO.AddComponent<Dot>();
        }

        // Check rates from DifficultyManager
        float obstacleChance = DifficultyManager.Instance != null ? DifficultyManager.Instance.ObstacleChance : 0f;
        float fastDotChance = DifficultyManager.Instance != null ? DifficultyManager.Instance.FastDotChance : 0f;
        float specialChance = DifficultyManager.Instance != null ? DifficultyManager.Instance.SpecialDotChance : 0f;

        bool isObstacle = false;
        bool isFastDot = false;
        bool isSpecial = false;

        float rand = Random.value;
        if (rand < obstacleChance)
        {
            isObstacle = true;
        }
        else if (rand < obstacleChance + fastDotChance)
        {
            isFastDot = true;
        }
        else if (rand < obstacleChance + fastDotChance + specialChance)
        {
            isSpecial = true;
        }

        DotType type = DotType.Normal;
        if (isObstacle)
        {
            // 50% chance of Frozen or Metal dot
            type = (Random.value < 0.5f) ? DotType.Frozen : DotType.Metal;
        }
        else if (isSpecial)
        {
            type = DotType.Rainbow;
        }
        else if (isFastDot)
        {
            type = DotType.Speed;
        }
        else
        {
            // Occasional Bomb or Time dots in normal spawns
            float r = Random.value;
            if (r < 0.04f) type = DotType.Bomb;
            else if (r < 0.08f) type = DotType.Time;
        }

        // Obstacles don't have standard colors
        int colorId = -1;
        if (type != DotType.Metal && type != DotType.Frozen)
        {
            int colorCount = ColorManager.Instance != null ? ColorManager.Instance.GetColorCount() : 5;
            colorId = Random.Range(0, colorCount);
        }

        dot.Init(colorId, type, OnDotLifeEnded);
        activeDots.Add(dot);

        return dotGO;
    }

    public void DespawnDot(Dot dot)
    {
        if (dot == null) return;

        dot.StopLifeCycle();
        activeDots.Remove(dot);

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(dot.gameObject);
        }
        else
        {
            Destroy(dot.gameObject);
        }
    }

    public void SetSpawnRange(float min, float max)
    {
        minX = min;
        maxX = max;
    }

    private void OnDotLifeEnded(Dot dot)
    {
        DespawnDot(dot);
    }
}
