using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossDimensionUI : MonoBehaviour
{
    public static BossDimensionUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject bossHUDPanel;
    [SerializeField] private TextMeshProUGUI stabilityText;
    [SerializeField] private Image stabilityFill;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI integrityText;
    [SerializeField] private Image integrityFill;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowBossHUD(bool show)
    {
        if (bossHUDPanel != null)
            bossHUDPanel.SetActive(show);
    }

    public void UpdateStabilityBar(float stabilityPercentage)
    {
        if (stabilityText != null)
        {
            stabilityText.text = $"CHAOS STABILITY: {Mathf.RoundToInt(stabilityPercentage)}%";
        }

        if (stabilityFill != null)
        {
            stabilityFill.fillAmount = stabilityPercentage / 100f;
        }
    }

    public void UpdateIntegrityBar(float integrityPercentage)
    {
        if (integrityText != null)
        {
            integrityText.text = $"ARENA INTEGRITY: {Mathf.RoundToInt(integrityPercentage)}%";
        }

        if (integrityFill != null)
        {
            integrityFill.fillAmount = integrityPercentage / 100f;
        }
    }

    public void UpdatePhaseIndicator(ChaosBossPhase phase)
    {
        if (phaseText != null)
        {
            switch (phase)
            {
                case ChaosBossPhase.Observation:
                    phaseText.text = "PHASE: OBSERVATION";
                    break;
                case ChaosBossPhase.ChaosExpansion:
                    phaseText.text = "PHASE: CHAOS EXPANSION";
                    break;
                case ChaosBossPhase.RealityCollapse:
                    phaseText.text = "PHASE: REALITY COLLAPSE";
                    break;
            }
        }
    }

    public void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideWarning));
            Invoke(nameof(HideWarning), 3f);
        }
    }

    private void HideWarning()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }
}
