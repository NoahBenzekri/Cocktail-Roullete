using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Turn Manager + Drinking Systems")]
    public TurnManager turnManager;
    public DrinkingSystem playerDrinking;
    public DrinkingSystem aiDrinking;

    [Header("Player Lives (assign 3 heart images)")]
    public Image[] playerHearts;

    [Header("AI Lives (assign 3 heart images)")]
    public Image[] aiHearts;

    [Header("Clocks")]
    public Slider playerClock;
    public Slider aiClock;
    public TextMeshProUGUI playerClockText;
    public TextMeshProUGUI aiClockText;

    [Header("Buttons")]
    public GameObject drinkButton;
    public GameObject passButton;

    void Start()
    {

        turnManager.OnLivesChanged += UpdateLives;
        turnManager.OnPhaseChanged += UpdateButtons;

        if (playerClockText != null && playerDrinking != null)
            playerClockText.text = Mathf.CeilToInt(playerDrinking.clockTime).ToString();

        if (aiClockText != null && aiDrinking != null)
            aiClockText.text = Mathf.CeilToInt(aiDrinking.clockTime).ToString();
        // Init
        UpdateLives(turnManager.PlayerLives, turnManager.AiLives);
        UpdateButtons(turnManager.Phase);
    }

    void Update()
    {
        if (playerClockText != null && playerDrinking != null)
            playerClockText.text = Mathf.CeilToInt(playerDrinking.clockTime).ToString();

        if (aiClockText != null && aiDrinking != null)
            aiClockText.text = Mathf.CeilToInt(aiDrinking.clockTime).ToString();
    }

    void UpdateLives(int playerLives, int aiLives)
    {
        for (int i = 0; i < playerHearts.Length; i++)
        {
            if (playerHearts[i] != null)
                playerHearts[i].gameObject.SetActive(i < playerLives);
        }

        for (int i = 0; i < aiHearts.Length; i++)
        {
            if (aiHearts[i] != null)
                aiHearts[i].gameObject.SetActive(i < aiLives);
        }
    }
    void UpdateButtons(TurnPhase phase)
    {
        if (drinkButton != null)
            drinkButton.SetActive(phase == TurnPhase.PlayerChoice || phase == TurnPhase.PlayerForced);

        if (passButton != null)
            passButton.SetActive(phase == TurnPhase.PlayerChoice);
    }
}