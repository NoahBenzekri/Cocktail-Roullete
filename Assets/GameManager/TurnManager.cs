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

    public TurnPhase Phase { get; private set; }

    int _playerLives;
    int _aiLives;

    private Vector3 _originCameraPos;
    private Quaternion _originCameraRot;

    public int PlayerLives => _playerLives;
    public int AiLives => _aiLives;

    public System.Action<TurnPhase> OnPhaseChanged;
    public System.Action<int, int> OnLivesChanged;
    public System.Action<bool> OnGameOver;

    // ── INIT ─────────────────────────────────────────────
    void Start()
    {
        if (mainCamera != null)
        {
            _originCameraPos = mainCamera.position;
            _originCameraRot = mainCamera.rotation;
        }

        _playerLives = lives;
        _aiLives = lives;

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
        // reset pour lock
        if (humanBrain?.playerInteraction != null)
            humanBrain.playerInteraction.isPouring = false;

        // snap all bottles back
        foreach (Ingredient i in FindObjectsOfType<Ingredient>())
            i.ResetPose();

        if (glass != null)
        {
            glass.ClearGlass();
            if (aiPool.Length > 0)
                glass.AddIngredient(aiPool[Random.Range(0, aiPool.Length)]);
        }

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
        StartCoroutine(ResolveRoutine(() => aiDrinking.Drink()));
    }

    public void AiPasses()
    {
        SetPhase(TurnPhase.PlayerForced);
        ReturnCamera(); // move camera back so player can see the glass/enemy
    }

    IEnumerator AiForcedDrink()
    {
        yield return new WaitForSeconds(1f);
        SetPhase(TurnPhase.Resolving);
        ReturnCamera();
        StartCoroutine(ResolveRoutine(() => aiDrinking.Drink()));
    }

    // ── RESOLVING ────────────────────────────────────────
    IEnumerator ResolveRoutine(System.Action drinkAction)
    {
        yield return new WaitForSeconds(1.2f); // wait for camera
        drinkAction?.Invoke();
        yield return new WaitForSeconds(2f);
        if (Phase != TurnPhase.GameOver)
            StartRound();
    }

    // ── LIVES ────────────────────────────────────────────
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