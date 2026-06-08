using System.Collections;
using UnityEngine;
using DG.Tweening;

public enum TurnPhase
{
    Intro,
    AddLiquid,
    AiAddLiquid,
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
    public Enemy enemy;

    [Header("Camera")]
    public Transform mainCamera;
    public PlayerLook playerLook;

    [Header("Camera Positions")]
    public Vector3 selectDrinkCameraPos;
    public Vector3 selectDrinkCameraRot;
    public Vector3 playerChoiceCameraPos;
    public Vector3 playerChoiceCameraRot;
    public Vector3 coinFlipCameraPos;
    public Vector3 coinFlipCameraRot;

    [Header("AI Ingredients")]
    public IngredientsOBJ[] aiPool;

    [Header("Lives")]
    public int lives = 3;

    public BartenderAnimator bartenderAnimator;
    public OpponentAnimator opponentAnimator;


    [Header("Scoreboard Camera")]
    public Vector3 scoreboardCameraPos;
    public Vector3 scoreboardCameraRot;
    public float scoreboardMoveTime = 1f;
    public float scoreboardStayTime = 12f;
    public TurnPhase Phase { get; private set; }

    int _playerLives;
    int _aiLives;

    private Vector3 _originCameraPos;
    private Quaternion _originCameraRot;

    public int PlayerLives => _playerLives;
    public int AiLives => _aiLives;

    public System.Action<TurnPhase> OnPhaseChanged;
    public System.Action<int, int, bool> OnLivesChanged;
    public System.Action<bool> OnGameOver;

    // ── INIT ─────────────────────────────────────────────
    void Start()
    {
        if (mainCamera != null)
        {
            _originCameraPos = mainCamera.position;
            _originCameraRot = mainCamera.rotation;
        }

        playerDrinking.isFrozen = true;
        aiDrinking.isFrozen = true;

        playerDrinking.OnClockExpired += () => OnLifeLost(isPlayer: true);
        aiDrinking.OnClockExpired += () => OnLifeLost(isPlayer: false);

        SetPhase(TurnPhase.Intro);
        StartCoroutine(IntroThenStart());
    }
    void Awake()
    {
        _playerLives = lives;
        _aiLives = lives;
    }

    // ── INTRO ────────────────────────────────────────────
    IEnumerator IntroThenStart()
    {
        humanBrain.enabled = false;

        string[] lines =
        {
            "You've wandered into the wrong bar.",
            "Every drink is a gamble.",
            "Every glass... a risk.",
            "A coin decides who goes first.",
            "You may pass... but they won't forget it.",
            "Lose all your time and you lose everything.",
            "Choose wisely."
        };

        foreach (string line in lines)
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(line);

            float timer = 0f;
            float duration = line.Length * 0.08f + 2f;

            while (timer < duration)
            {
                if (Input.GetKeyDown(KeyCode.Return)) break;
                timer += Time.deltaTime;
                yield return null;
            }
        }

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ClearDialogue();

        yield return new WaitForSeconds(1f);
        StartRound();
    }

    // ── ROUND ────────────────────────────────────────────
    void StartRound()
    {
        Debug.Log($"[StartRound] playerDrinking.isFrozen before: {playerDrinking.isFrozen}");

        // reset pour lock
        if (humanBrain?.playerInteraction != null)
            humanBrain.playerInteraction.isPouring = false;

        foreach (Ingredient i in FindObjectsOfType<Ingredient>())
            i.ResetPose();

        if (glass != null)
        {
            glass.ClearGlass();
            if (aiPool.Length > 0)
                glass.AddIngredient(aiPool[Random.Range(0, aiPool.Length)]);
        }

        playerDrinking.isFrozen = false;
        aiDrinking.isFrozen = false;
        Debug.Log($"[StartRound] playerDrinking.isFrozen after: {playerDrinking.isFrozen}");
        Debug.Log($"[StartRound] playerDrinking object: {playerDrinking.gameObject.name}");

        humanBrain.enabled = true;
        SetPhase(TurnPhase.AddLiquid);
    }
    // ── PLAYER CONFIRMED BOTTLE ───────────────────────────
    public void PlayerConfirmed()
    {
        if (Phase != TurnPhase.AddLiquid) return;
        humanBrain.enabled = false;
        SetPhase(TurnPhase.AiAddLiquid);
        StartCoroutine(AiAddLiquidRoutine());
    }

    IEnumerator AiAddLiquidRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (aiPool.Length > 0 && glass != null)
            glass.AddIngredient(aiPool[Random.Range(0, aiPool.Length)]);

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FlipCoin());
    }

    // ── COIN FLIP ────────────────────────────────────────
    IEnumerator FlipCoin()
    {
        SetPhase(TurnPhase.CoinFlip);

        if (bartenderAnimator != null)
            bartenderAnimator.PlayCoinFlip();

        if (coinFlip != null)
        {
            coinFlip.OnCoinSettled = null;
            coinFlip.OnCoinSettled = AfterFlip;
            coinFlip.Launch();

            // wait for bartender animation to finish before returning camera
            if (bartenderAnimator != null)
            {
                bool animDone = false;
                bartenderAnimator.OnFlipComplete = () => animDone = true;
                yield return new WaitUntil(() => animDone);
            }

            yield break;
        }

        yield return new WaitForSeconds(1f);
        AfterFlip(Random.value < 0.5f);
    }

    void AfterFlip(bool playerFirst)
    {
        string result = playerFirst ? "Heads.\nThe choice is yours." : "Tails.\nThey decide your fate.";

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(result);

        if (playerFirst)
            SetPhase(TurnPhase.PlayerChoice);
        else
        {
            SetPhase(TurnPhase.AiChoice);
            StartCoroutine(AiTurn());
        }
    }

    // ── PLAYER ACTIONS ───────────────────────────────────
    public void PlayerDrinks()
    {
        if (Phase != TurnPhase.PlayerChoice && Phase != TurnPhase.PlayerForced) return;
        SetPhase(TurnPhase.Resolving);
        ReturnCamera();
        StartCoroutine(ResolveRoutine(() => playerDrinking.Drink()));
    }

    public void PlayerPasses()
    {
        if (Phase != TurnPhase.PlayerChoice) return;
        SetPhase(TurnPhase.AiForced);
        StartCoroutine(AiForcedDrink());
    }

    // ── AI ACTIONS ───────────────────────────────────────
    IEnumerator AiTurn()
    {
        yield return null;
        aiBrain.MakeDecision(_playerLives, _aiLives);
    }

    public void AiDrinks()
    {
        SetPhase(TurnPhase.Resolving);
        ReturnCamera();
        StartCoroutine(ResolveRoutine(() =>

        {
            if (opponentAnimator != null) opponentAnimator.PlayDrink();
            aiDrinking.Drink();
        }));
    }

    public void AiPasses()
    {
        SetPhase(TurnPhase.PlayerForced);

    }

    IEnumerator AiForcedDrink()
    {
        yield return new WaitForSeconds(1f);
        SetPhase(TurnPhase.Resolving);
        ReturnCamera();
        StartCoroutine(ResolveRoutine(() =>
        {
            if (opponentAnimator != null) opponentAnimator.PlayDrink();
            aiDrinking.Drink();
        }));
    }

    // ── RESOLVING ────────────────────────────────────────
    IEnumerator ResolveRoutine(System.Action drinkAction)
    {
        // freeze during resolution so time doesn't drain unfairly
        playerDrinking.isFrozen = true;
        aiDrinking.isFrozen = true;

        yield return new WaitForSeconds(2.2f);
        drinkAction?.Invoke();
        yield return new WaitForSeconds(5f);
        yield return StartCoroutine(ShowScoreboardRoutine());

        if (Phase != TurnPhase.GameOver)
            StartRound(); // ← this will unfreeze them again
    }

    // ── LIVES ────────────────────────────────────────────
    void OnLifeLost(bool isPlayer)
    {
        StartCoroutine(OnLifeLostDelayed(isPlayer));
    }
    IEnumerator OnLifeLostDelayed(bool isPlayer)
    {
        // wait for drink/death animation to finish
        yield return new WaitForSeconds(2f); // adjust to match your longest animation

        if (isPlayer)
        {
            _playerLives--;
            playerDrinking.clockTime = 90f;
            playerDrinking.hasExpired = false;
        }
        else
        {
            _aiLives--;
            aiDrinking.clockTime = 90f;
            aiDrinking.hasExpired = false;
            if (opponentAnimator != null) opponentAnimator.PlayDeath();
        }

        OnLivesChanged?.Invoke(_playerLives, _aiLives,true);

        if (_playerLives <= 0)
        {
            SetPhase(TurnPhase.GameOver);
            OnGameOver?.Invoke(false);
            yield break;
        }

        if (_aiLives <= 0)
        {
            SetPhase(TurnPhase.GameOver);
            OnGameOver?.Invoke(true);
            yield break;
        }
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (Phase != TurnPhase.GameOver)
            StartRound();
    }

    // ── CAMERA ───────────────────────────────────────────
    public void ReturnCamera()
    {
        if (mainCamera == null) return;
        if (playerLook != null) playerLook.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mainCamera.DOMove(_originCameraPos, 1f).SetEase(Ease.InOutSine);
        mainCamera.DORotateQuaternion(_originCameraRot, 1f).SetEase(Ease.InOutSine);
    }

    void MoveCamera(Vector3 pos, Vector3 rot, bool unlockCursor, bool disableLook)
    {
        if (playerLook != null) playerLook.enabled = !disableLook;
        Cursor.lockState = unlockCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = unlockCursor;
        mainCamera.DOMove(pos, 1f).SetEase(Ease.InOutSine);
        mainCamera.DORotate(rot, 1f).SetEase(Ease.InOutSine);
    }

    // ── PHASE ────────────────────────────────────────────
    void SetPhase(TurnPhase p)
    {
        Phase = p;
        OnPhaseChanged?.Invoke(p);
        Debug.Log("[TurnManager] " + p);

        if (enemy != null)
        {
            var outline = enemy.GetComponentInChildren<Outline>();
            if (outline != null) outline.enabled = false;
        }

        if (mainCamera == null) return;

        switch (p)
        {
            case TurnPhase.AddLiquid:
                MoveCamera(selectDrinkCameraPos, selectDrinkCameraRot, unlockCursor: true, disableLook: true);
                break;

            case TurnPhase.CoinFlip:
                MoveCamera(coinFlipCameraPos, coinFlipCameraRot, unlockCursor: false, disableLook: true);
                break;

            case TurnPhase.PlayerChoice:
                MoveCamera(playerChoiceCameraPos, playerChoiceCameraRot, unlockCursor: true, disableLook: true);
                StartCoroutine(PlayerChoiceDialogue());
                break;

            case TurnPhase.PlayerForced:
                MoveCamera(playerChoiceCameraPos, playerChoiceCameraRot, unlockCursor: true, disableLook: true);
                StartCoroutine(PlayerForcedDialogue());
                break;
        }
    }

    IEnumerator ShowScoreboardRoutine()
    {
        if (mainCamera == null) yield break;

        if (playerLook != null)
            playerLook.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.DOMove(scoreboardCameraPos, scoreboardMoveTime).SetEase(Ease.InOutSine);
        mainCamera.DORotate(scoreboardCameraRot, scoreboardMoveTime).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(scoreboardMoveTime);

        // UI should already update from OnLivesChanged / timer text here
        yield return new WaitForSeconds(scoreboardStayTime);

        ReturnCamera();

        yield return new WaitForSeconds(scoreboardMoveTime);
    }

    IEnumerator PlayerChoiceDialogue()
    {
        yield return new WaitForSeconds(1.1f);
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue("Click the glass to drink.\nClick your opponent to make them drink.");
    }

    IEnumerator PlayerForcedDialogue()
    {
        yield return new WaitForSeconds(1.1f);
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue("You have no choice.\nClick the glass to drink.");
    }
}