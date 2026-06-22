using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardChoiceUI : MonoBehaviour
{
    public static RewardChoiceUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Button optionAButton;
    [SerializeField] private Button optionBButton;
    [SerializeField] private Button optionCButton;
    
    private System.Action onRewardSelected;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (optionAButton != null) optionAButton.onClick.AddListener(() => SelectReward("Combo Master"));
        if (optionBButton != null) optionBButton.onClick.AddListener(() => SelectReward("Time Core"));
        if (optionCButton != null) optionCButton.onClick.AddListener(() => SelectReward("Chaos Core"));
    }

    public void ShowRewardChoices(System.Action callback)
    {
        onRewardSelected = callback;
        if (rewardPanel != null) rewardPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game while choosing
    }

    private void SelectReward(string rewardName)
    {
        Debug.Log($"[RewardChoiceUI] Player selected reward: {rewardName}");

        if (rewardName == "Combo Master")
        {
            GameBrain.Instance?.AddMutation("FeverDurationMod", 0.20f);
            Debug.Log("Applied Combo Master: +20% Fever/Combo Duration");
        }
        else if (rewardName == "Time Core")
        {
            GameBrain.Instance?.AddMutation("TimeBallBoost", 0.50f);
            Debug.Log("Applied Time Core: Time Balls +50% efficacy");
        }
        else if (rewardName == "Chaos Core")
        {
            GameBrain.Instance?.AddMutation("ChaosModifierUnlocked", 1.0f);
            GameBrain.Instance?.AddMutation("UnlockVoidBall", 1f); // Chaos related ball
            Debug.Log("Applied Chaos Core: Unlocked Chaos Modifier & Void Ball");
        }

        if (rewardPanel != null) rewardPanel.SetActive(false);
        Time.timeScale = 1f;

        onRewardSelected?.Invoke();
    }
}
