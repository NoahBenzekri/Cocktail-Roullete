using System.Collections;
using UnityEngine;

public enum TurnPhase
{
    AddLiquid,
    CoinFlip,
    PlayerChoice, 
    PlayerForced,
    AiChoice,
    AiForced, 
    Resolving, 
    GameOver 
}

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    public CocktailGlass glass;
    public DrinkingSystem playerDrinking;
    public DrinkingSystem aiDrinking;
    public HumanBrain humanBrain;
    public CoinFlip coinFlip;
    public AiBrain aiBrain;

    [Header("AI Ingredients")]
    public IngredientsOBJ[] aiPool;

    [Header("Lives")]
    public int lives = 3;

    public TurnPhase Phase { get; private set; }
    int _playerLives;
    int _aiLives;

    public int PlayerLives => _playerLives > 0 ? _playerLives : lives;
    public int AiLives => _aiLives > 0 ? _aiLives : lives;
    public System.Action<TurnPhase> OnPhaseChanged;
    public System.Action<int, int> OnLivesChanged;  // playerLives, aiLives
    public System.Action<bool> OnGameOver;       // true = player wins


    void Start()
    {
        Debug.Log("PLAYER SYSTEM = " + playerDrinking.GetInstanceID());
        Debug.Log("AI SYSTEM = " + aiDrinking.GetInstanceID());

        _playerLives = lives;
        _aiLives = lives;

        playerDrinking.isFrozen = false;
        aiDrinking.isFrozen = false;

        playerDrinking.OnClockExpired += () => OnLifeLost(isPlayer: true);
        aiDrinking.OnClockExpired += () => OnLifeLost(isPlayer: false);

        StartRound();
    }

   

    void StartRound()
    {
        glass.ClearGlass();
        humanBrain.enabled = true;

        // AI picks a random ingredient
        if (aiPool.Length > 0)
            glass.AddIngredient(aiPool[Random.Range(0, aiPool.Length)]);

        SetPhase(TurnPhase.AddLiquid);
        
    }

    public void PlayerConfirmed()
    {
        if (Phase != TurnPhase.AddLiquid) return;

        humanBrain.enabled = false;
        StartCoroutine(FlipCoin());
    }


    IEnumerator FlipCoin()
    {
        SetPhase(TurnPhase.CoinFlip);

        if (coinFlip != null)
        {
            coinFlip.OnCoinSettled = AfterFlip;
            coinFlip.Launch();
            yield break; 
        }

        yield return new WaitForSeconds(1f);
        AfterFlip(Random.value < 0.5f);
    }

    void AfterFlip(bool playerFirst)
    {
        if (playerFirst)
        {
            SetPhase(TurnPhase.PlayerChoice);
            playerDrinking.isFrozen = false; 
        }
        else
        {
            SetPhase(TurnPhase.AiChoice);
            StartCoroutine(AiTurn());
        }
    }
    public void PlayerDrinks()
    {
        if (Phase != TurnPhase.PlayerChoice && Phase != TurnPhase.PlayerForced) return;
        SetPhase(TurnPhase.Resolving);
        playerDrinking.Drink();
        StartCoroutine(NextRoundDelay());
        Debug.Log("=== PLAYER DRINKING ===");
        Debug.Log("Current phase: " + Phase);
        Debug.Log("Player clock before drink: " + playerDrinking.clockTime);
        Debug.Log("AI clock before drink: " + aiDrinking.clockTime);
    }

    public void PlayerPasses()
    {
        if (Phase != TurnPhase.PlayerChoice) return;
        SetPhase(TurnPhase.AiForced);
        StartCoroutine(AiForcedDrink());
    }


    IEnumerator AiTurn()
    {
        yield return null; 
        aiBrain.MakeDecision(_playerLives, _aiLives);
    }

    // Called by AiBrain after decision
    public void AiDrinks()
    {
        SetPhase(TurnPhase.Resolving);
        aiDrinking.Drink();
        StartCoroutine(NextRoundDelay());
        Debug.Log("=== AI DRINKING ===");
        Debug.Log("Current phase: " + Phase);
        Debug.Log("Player clock before drink: " + playerDrinking.clockTime);
        Debug.Log("AI clock before drink: " + aiDrinking.clockTime);
    }

    public void AiPasses()
    {
        SetPhase(TurnPhase.PlayerForced);
    }

    IEnumerator AiForcedDrink()
    {
        yield return new WaitForSeconds(1f);
        SetPhase(TurnPhase.Resolving);
        aiDrinking.Drink();
        StartCoroutine(NextRoundDelay());
    }


   void OnLifeLost(bool isPlayer)
{
    if (isPlayer)
    {
        _playerLives--;
        playerDrinking.clockTime = 90f;
    }
    else
    {
        _aiLives--;
        aiDrinking.clockTime = 90f;
    }

    OnLivesChanged?.Invoke(_playerLives, _aiLives);

    if (_playerLives <= 0)
    {
        SetPhase(TurnPhase.GameOver);
        OnGameOver?.Invoke(false);
        return;
    }

    if (_aiLives <= 0)
    {
        SetPhase(TurnPhase.GameOver);
        OnGameOver?.Invoke(true);
        return;
    }

    StartCoroutine(NextRoundDelay());
}


    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (Phase != TurnPhase.GameOver)
            StartRound();
    }

    void SetPhase(TurnPhase p)
    {
        Phase = p;
        OnPhaseChanged?.Invoke(p);
        Debug.Log("[TurnManager] " + p);
    }
}