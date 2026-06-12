using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public DrinkingSystem playerDrinking;
    public DrinkingSystem aiDrinking;

    [Header("Player Lives")]
    public Image[] playerHearts;

    [Header("AI Lives")]
    public Image[] aiHearts;

    [Header("Health / Timer Text")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI aiHealthText;

    [Header("Buttons")]
    public GameObject drinkButton;
    public GameObject passButton;

    [Header("Heart Disappear Delay")]
    public float heartDelayNormal = 1.5f;
    public float heartDelayDeath = 3f; // longer to account for OnLifeLostDelayed's own 2f
    void Start()
    {
        if (turnManager != null)
        {
            turnManager.OnLivesChanged += UpdateLives;
            turnManager.OnPhaseChanged += UpdateButtons;

            UpdateLives(turnManager.PlayerLives, turnManager.AiLives, false);
            UpdateButtons(turnManager.Phase);
        }

        UpdateHealthTexts();
    }

    void Update()
    {
        UpdateHealthTexts();
    }

    void UpdateHealthTexts()
    {
        if (playerHealthText != null && playerDrinking != null)
            playerHealthText.text = FormatTime(playerDrinking.clockTime);

        if (aiHealthText != null && aiDrinking != null)
            aiHealthText.text = FormatTime(aiDrinking.clockTime);
    }

    string FormatTime(float time)
    {
        int seconds = Mathf.CeilToInt(time);
        seconds = Mathf.Max(seconds, 0);

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        return $"{minutes:00}:{remainingSeconds:00}";
    }

   void UpdateLives(int playerLives, int aiLives, bool isDeath)
    {
        for (int i = 0; i < playerHearts.Length; i++)
            if (playerHearts[i] != null)
                playerHearts[i].gameObject.SetActive(i < playerLives);

        for (int i = 0; i < aiHearts.Length; i++)
            if (aiHearts[i] != null)
                aiHearts[i].gameObject.SetActive(i < aiLives);
    }
    IEnumerator UpdateLivesDelayed(int playerLives, int aiLives, bool isDeath)
    {
        // if lives actually dropped, check if we're already mid-delay (death path)
        yield return new WaitForSeconds(isDeath ? heartDelayDeath : heartDelayNormal);

        for (int i = 0; i < playerHearts.Length; i++)
            if (playerHearts[i] != null)
                playerHearts[i].gameObject.SetActive(i < playerLives);

        for (int i = 0; i < aiHearts.Length; i++)
            if (aiHearts[i] != null)
                aiHearts[i].gameObject.SetActive(i < aiLives);
    }

    void UpdateButtons(TurnPhase phase)
    {
        if (drinkButton != null)
            drinkButton.SetActive(phase == TurnPhase.PlayerChoice || phase == TurnPhase.PlayerForced);

        if (passButton != null)
            passButton.SetActive(phase == TurnPhase.PlayerChoice);
    }

    void OnDestroy()
    {
        if (turnManager == null) return;

        turnManager.OnLivesChanged -= UpdateLives;
        turnManager.OnPhaseChanged -= UpdateButtons;
    }
}