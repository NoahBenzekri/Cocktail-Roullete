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
    public Ingredient[] aiBottles;

    [Header("Lives")]
    public int lives = 3;

    [Header("Animators")]
    public BartenderAnimator bartenderAnimator;
    public OpponentAnimator opponentAnimator;
    public PlayerAnimator playerAnimator;

    [Header("Scoreboard Camera")]
    public Vector3 scoreboardCameraPos;
    public Vector3 scoreboardCameraRot;
    public float scoreboardMoveTime = 1f;
    public float scoreboardStayTime = 12f;

    [Header("Resolve Timing")]
    public float drinkDelay = 2.2f;   // wait before the drink resolves
    public float postDrinkDelay = 3f; // wait after drink/death before scoreboard
    public float drinkAnimTime = 4.3f;   // length of the Drink clip — Die fires after this
    public float afterFlipDelay = 1.5f;
    public TurnPhase Phase { get; private set; }


    int _playerLives;
    int _aiLives;

    Vector3 _originCameraPos;
    Quaternion _originCameraRot;

    bool _playerPendingDeath;
    bool _aiPendingDeath;
    bool _resolvingDeath;

    public int PlayerLives => _playerLives;
    public int AiLives => _aiLives;

    public System.Action<TurnPhase> OnPhaseChanged;
    public System.Action<int, int, bool> OnLivesChanged;
    public System.Action<bool> OnGameOver;

    // ── INIT ─────────────────────────────────────────────
    void Awake()
    {
        _playerLives = lives;
        _aiLives = lives;
    }

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
        if (humanBrain?.playerInteraction != null)
            humanBrain.playerInteraction.isPouring = false;

        foreach (Ingredient i in FindObjectsOfType<Ingredient>())
            i.ResetPose();

        if (opponentAnimator != null) opponentAnimator.ResetTriggers();
        if (playerAnimator != null) playerAnimator.ResetTriggers();

        if (glass != null)
            glass.ClearGlass();

        playerDrinking.isFrozen = false;
        aiDrinking.isFrozen = false;

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

        if (aiBottles.Length > 0 && glass != null)
        {
            Ingredient chosen = aiBottles[Random.Range(0, aiBottles.Length)];

            bool poured = false;
            chosen.Pour(glass, () =>
            {
                glass.AddIngredient(chosen.ingredientData);
                poured = true;
            });

            float timeout = 6f, t = 0f;
            while (!poured && t < timeout) { t += Time.deltaTime; yield return null; }
            if (!poured) glass.AddIngredient(chosen.ingredientData); // fallback
        }

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
            coinFlip.OnCoinSettled = AfterFlip;
            coinFlip.Launch();

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

        StartCoroutine(AfterFlipRoutine(playerFirst));
    }

    IEnumerator AfterFlipRoutine(bool playerFirst)
    {
        yield return new WaitForSeconds(afterFlipDelay); // hold on the coin result

        if (playerFirst)
        {
            SetPhase(TurnPhase.PlayerChoice);
        }
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
        StartCoroutine(ResolveRoutine(() =>
        {
            if (playerAnimator != null) playerAnimator.PlayDrink();
            playerDrinking.Drink();
        }));
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
        playerDrinking.isFrozen = true;
        aiDrinking.isFrozen = true;

        yield return new WaitForSeconds(drinkDelay);
        drinkAction?.Invoke();              // fires Drink trigger + resolves effect

        // wait for the drink animation to actually finish before death
        if (_aiPendingDeath && opponentAnimator != null)
            yield return StartCoroutine(opponentAnimator.WaitForDrinkThenDie());
        else if (_playerPendingDeath && playerAnimator != null)
            yield return StartCoroutine(playerAnimator.WaitForDrinkThenDie());
        else
            yield return new WaitForSeconds(drinkAnimTime); // non-lethal, just pace it

        yield return new WaitForSeconds(postDrinkDelay);
        yield return StartCoroutine(ShowScoreboardRoutine(processDeaths: true));

        if (Phase != TurnPhase.GameOver)
            StartRound();
    }
    // ── LIVES ────────────────────────────────────────────
    void OnLifeLost(bool isPlayer)
    {
        if (Phase == TurnPhase.GameOver) return;

        if (isPlayer) _playerPendingDeath = true;
        else _aiPendingDeath = true;

        // stop the drain immediately
        playerDrinking.isFrozen = true;
        aiDrinking.isFrozen = true;

        // pure timeout (no resolve running) → drive the scoreboard ourselves
        if (Phase != TurnPhase.Resolving && !_resolvingDeath)
        {
            _resolvingDeath = true;
            SetPhase(TurnPhase.Resolving);
            StartCoroutine(TimeoutRoutine());
        }
        // otherwise ResolveRoutine owns the scoreboard and will pick this up
    }

    IEnumerator TimeoutRoutine()
    {
        if (_aiPendingDeath && opponentAnimator != null)
            opponentAnimator.PlayDeath();
        if (_playerPendingDeath && playerAnimator != null)
            playerAnimator.PlayDeath();

        yield return new WaitForSeconds(2f); // death-anim breathing room

        yield return StartCoroutine(ShowScoreboardRoutine(processDeaths: true));

        if (Phase != TurnPhase.GameOver)
            StartRound();
    }

    bool ProcessPendingDeaths()
    {
        if (_playerPendingDeath) _playerLives--;
        if (_aiPendingDeath) _aiLives--;

        OnLivesChanged?.Invoke(_playerLives, _aiLives, true);

        if (_playerLives <= 0) { SetPhase(TurnPhase.GameOver); OnGameOver?.Invoke(false); return true; }
        if (_aiLives <= 0) { SetPhase(TurnPhase.GameOver); OnGameOver?.Invoke(true); return true; }
        return false;
    }

    void ResetPendingClocks()
    {
        if (_playerPendingDeath)
        {
            playerDrinking.clockTime = 90f;
            playerDrinking.hasExpired = false;
            _playerPendingDeath = false;
        }
        if (_aiPendingDeath)
        {
            aiDrinking.clockTime = 90f;
            aiDrinking.hasExpired = false;
            _aiPendingDeath = false;
        }

        _resolvingDeath = false;
    }

    // ── CAMERA ───────────────────────────────────────────
    public float cameraMoveTime = 1f;

    public void ReturnCamera(bool restoreLook = true)
    {
        if (mainCamera == null) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.DOKill();

        mainCamera.DOMove(_originCameraPos, cameraMoveTime).SetEase(Ease.InOutSine);
        mainCamera.DORotateQuaternion(_originCameraRot, cameraMoveTime)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                if (restoreLook && playerLook != null)
                    playerLook.enabled = true;
            });
    }

    void MoveCamera(Vector3 pos, Vector3 rot, bool unlockCursor, bool disableLook)
    {
        if (playerLook != null) playerLook.enabled = !disableLook;
        Cursor.lockState = unlockCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = unlockCursor;

        mainCamera.DOKill();

        mainCamera.DOMove(pos, cameraMoveTime).SetEase(Ease.InOutSine);
        mainCamera.DORotateQuaternion(Quaternion.Euler(rot), cameraMoveTime).SetEase(Ease.InOutSine);
    }
    // ── PHASE ────────────────────────────────────────────
    void SetPhase(TurnPhase p)
    {
        Phase = p;
        OnPhaseChanged?.Invoke(p);

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

    IEnumerator ShowScoreboardRoutine(bool processDeaths = false)
    {
        if (mainCamera == null) yield break;

        if (playerLook != null) playerLook.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.DOMove(scoreboardCameraPos, scoreboardMoveTime).SetEase(Ease.InOutSine);
        mainCamera.DORotate(scoreboardCameraRot, scoreboardMoveTime).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(scoreboardMoveTime);

        // drop the life here so the heart vanishes on the board
        if (processDeaths && ProcessPendingDeaths())
            yield break; // game over: stay on board, no reset/return

        yield return new WaitForSeconds(scoreboardStayTime);

        if (_aiPendingDeath && opponentAnimator != null)
            opponentAnimator.ReturnToIdle();
        if (processDeaths)
            ResetPendingClocks();

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