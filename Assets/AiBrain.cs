using UnityEngine;
using System.Collections;
public class AiBrain : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public DrinkingSystem aiDrinking;
    public DrinkingSystem playerDrinking;

    [Header("Timing")]
    [Tooltip("How long the AI 'thinks' before acting")]
    public float thinkTime = 1.2f;

    public void MakeDecision(int playerLives, int aiLives)
    {
        StartCoroutine(DecideRoutine(aiLives, playerLives));
    }

    private IEnumerator DecideRoutine(int aiLives, int playerLives)
    {
        yield return new WaitForSeconds(thinkTime);

        float riskScore = CalculateRisk(aiLives, playerLives);

        // riskScore 0-1: closer to 1 = more likely to pass
        bool pass = Random.value < riskScore;

        Debug.Log($"[AiBrain] Risk score: {riskScore:F2} → {(pass ? "PASS" : "DRINK")}");

        if (pass)
            turnManager.AiPasses();
        else
            turnManager.AiDrinks();
    }
    private float CalculateRisk(int aiLives, int playerLives)
    {
  
        float aiClock     = aiDrinking.clockTime;
        float playerClock = playerDrinking.clockTime;
        float totalClock  = aiClock + playerClock;

        // 0 = AI has no time, 1 = AI has all the time
        float clockAdvantage = totalClock > 0f ? aiClock / totalClock : 0.5f;

        //Lives advantage 
        float totalLives   = aiLives + playerLives;
        float livesAdvantage = totalLives > 0f ? (float)aiLives / totalLives : 0.5f;

        //Desperation modifier
        float desperationPenalty = aiLives == 1 ? 0.2f : 0f;

        // Aggression modifier
        float aggressionBonus = playerLives == 1 ? 0.2f : 0f;

        // Combine
        float score = (clockAdvantage * 0.5f) + (livesAdvantage * 0.5f)
                      - desperationPenalty
                      + aggressionBonus;

        return Mathf.Clamp01(score);
    }
}