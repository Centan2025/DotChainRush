using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public int ActiveLevel { get; private set; } = 1;
    public int ActiveGoal { get; private set; } = 1000;
    public float TimeElapsed { get; private set; }

    // Dynamic gameplay variables scaled based on level
    public float SpawnInterval { get; private set; } = 0.8f;
    public float GravityScale { get; private set; } = 0.15f;
    public float SpecialDotChance { get; private set; } = 0f;
    public float FastDotChance { get; private set; } = 0f;
    public float ObstacleChance { get; private set; } = 0f;

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

    private void Start()
    {
        ResetDifficulty();
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        TimeElapsed += Time.deltaTime;
    }

    public void ResetDifficulty()
    {
        TimeElapsed = 0f;
        SetLevel(1);
    }

    public int GetGoalForLevel(int lvl)
    {
        if (lvl <= 1) return 1000;
        if (lvl == 2) return 2500;
        if (lvl == 3) return 5000;
        if (lvl == 4) return 10000;
        return 10000 + (lvl - 4) * 5000;
    }

    public void SetLevel(int lvl)
    {
        ActiveLevel = lvl;
        ActiveGoal = GetGoalForLevel(lvl);

        // Configure level specific difficulty attributes
        if (lvl == 1)
        {
            SpawnInterval = 0.8f;
            GravityScale = 0.15f;
            SpecialDotChance = 0f;
            FastDotChance = 0f;
            ObstacleChance = 0f;
        }
        else if (lvl == 2)
        {
            SpawnInterval = 0.65f;
            GravityScale = 0.23f;
            SpecialDotChance = 0.05f;
            FastDotChance = 0.18f; // Introduce fast dots
            ObstacleChance = 0f;
        }
        else if (lvl == 3)
        {
            SpawnInterval = 0.52f;
            GravityScale = 0.3f;
            SpecialDotChance = 0.08f;
            FastDotChance = 0.22f;
            ObstacleChance = 0.12f; // Introduce obstacle dots
        }
        else if (lvl == 4)
        {
            SpawnInterval = 0.42f;
            GravityScale = 0.42f;
            SpecialDotChance = 0.12f;
            FastDotChance = 0.28f;
            ObstacleChance = 0.16f;
        }
        else
        {
            // Level 5+ progressive infinite scaling
            int extra = lvl - 4;
            SpawnInterval = Mathf.Max(0.24f, 0.42f - extra * 0.02f);
            GravityScale = Mathf.Min(0.65f, 0.42f + extra * 0.02f);
            SpecialDotChance = Mathf.Min(0.24f, 0.12f + extra * 0.015f);
            FastDotChance = Mathf.Min(0.35f, 0.28f + extra * 0.01f);
            ObstacleChance = Mathf.Min(0.22f, 0.16f + extra * 0.01f);
        }
    }

    public string GetLevelPreviewText(int nextLvl)
    {
        if (nextLvl == 2)
        {
            return "Alev saçan HIZLI TOPLAR (Hızlı Dönen & Turuncu İzi Olan) oyuna dahil oluyor! Hızlanmaya hazır ol!";
        }
        if (nextLvl == 3)
        {
            return "Zincirlere bağlanamayan katı metal ENGEL TOPLARI düşmeye başlıyor! Onları patlatmak için komşu topları yok etmelisin!";
        }
        if (nextLvl == 4)
        {
            return "Fırtına başlıyor! Topların yerçekimi ve düşme sıklığı büyük oranda artıyor. Hızlı ol!";
        }
        return $"Seviye {nextLvl} başlıyor! Topların düşme hızı ve engellerin sıklığı daha da artacak. Rekorunu zorla!";
    }
}
