using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }

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

    public void ResetScore()
    {
        CurrentScore = 0;
        UpdateUI();
    }

    public void AddPoints(int points)
    {
        CurrentScore += points;
        UpdateUI();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckLevelCompletion();
        }
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(CurrentScore);
        }
    }
}
