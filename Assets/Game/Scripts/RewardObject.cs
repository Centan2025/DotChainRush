using UnityEngine;

public enum RewardObjectType
{
    TreasureBox,
    MysteryOrb,
    Crystal
}

public class RewardObject : MonoBehaviour
{
    public RewardObjectType type = RewardObjectType.TreasureBox;
    public RewardConfig config;
    public int baseRewardAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Dot dot = other.GetComponent<Dot>();
        if (dot == null) return;

        // Trigger reward
        TriggerReward();

        // Destroy self
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.SpawnExplosion(transform.position, Color.yellow, 20);
        }
        Destroy(gameObject);
    }

    public void TriggerReward()
    {
        switch (type)
        {
            case RewardObjectType.TreasureBox:
                AwardTreasureBoxDrops();
                break;
            case RewardObjectType.MysteryOrb:
                ApplyMysteryOrbEffect();
                break;
            case RewardObjectType.Crystal:
                int currentCrystals = SaveSystem.LoadInt("Crystals", 0);
                SaveSystem.SaveInt("Crystals", currentCrystals + baseRewardAmount);
                Debug.Log($"[Reward] Awarded +{baseRewardAmount} Crystals! Total: {currentCrystals + baseRewardAmount}");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowComboFeedback("CRYSTAL ACQUIRED!", Color.cyan);
                }
                break;
        }
    }

    private void AwardTreasureBoxDrops()
    {
        int amount = config != null ? config.baseAmount : baseRewardAmount;
        int coins = SaveSystem.LoadInt("Coins", 0);
        SaveSystem.SaveInt("Coins", coins + amount * 10);
        Debug.Log($"[Reward] Treasure Box opened! Awarded {amount * 10} Coins.");

        // Unlock a temporary upgrade in the GameBrain
        if (GameBrain.Instance != null)
        {
            GameBrain.Instance.AddMutation("ScoreMultiplier", 0.1f);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowComboFeedback("TREASURE BOX OPENED!", "+10% Score Multiplier Obtained", "", Color.yellow);
            }
        }
    }

    private void ApplyMysteryOrbEffect()
    {
        float rand = Random.value;
        if (rand < 0.50f)
        {
            // Positive: extra time!
            RadialTimerController rtc = FindAnyObjectByType<RadialTimerController>();
            if (rtc != null) rtc.CurrentTime += 10f;
            CircularTimer ct = FindAnyObjectByType<CircularTimer>();
            if (ct != null) ct.CurrentTime += 10f;

            Debug.Log("[Reward] Mystery Orb Positive Effect: +10 Seconds!");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowComboFeedback("MYSTERY ORB:", "+10 SECONDS!", "", Color.green);
            }
        }
        else
        {
            // Negative: speed up gravity slightly!
            if (GameBrain.Instance != null)
            {
                GameBrain.Instance.AddMutation("GravityMod", 0.05f);
                Debug.Log("[Reward] Mystery Orb Negative Effect: Gravity Speed Increased!");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowComboFeedback("MYSTERY ORB: CURSE!", "Gravity speed increased!", "", Color.red);
                }
            }
        }
    }
}
