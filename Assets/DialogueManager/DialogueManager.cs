using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private string dialogueLine;
    [SerializeField] private TextMeshProUGUI dialogueUI;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private float characterDelay = 0.3f;
    [SerializeField] private float fadeDelay = 2f;
    [SerializeField] private Button[] choiceButtons = new Button[2];

    [Header("Typing Sound")]
    [SerializeField] private AudioSource typeSource;        // dedicated 2D source (Spatial Blend = 0)
    [SerializeField] private AudioClip charactersAudioClip;
    [SerializeField] private int playEveryNChars = 2;       // blip every Nth visible char
    [SerializeField] private float typeVolume = 0.35f;      // keep low
    [SerializeField] private float minBlipInterval = 0.04f; // hard floor between blips
    [Range(0f, 0.2f)] [SerializeField] private float pitchJitter = 0.08f;

    private float _lastBlipTime;
    private int _charsSinceBlip;

    private Coroutine typeCoroutine;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(dialogueLine))
            StartDialogue(dialogueLine);
    }

    public void StartDialogue(string dialogue)
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, fadeDelay));
        typeCoroutine = StartCoroutine(StartDialogueRoutine(dialogue));
    }

    public void ClearDialogue()
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        dialogueUI.text = "";
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, fadeDelay));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha, float duration)
    {
        float startAlpha = dialogueCanvasGroup.alpha;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = targetAlpha;
    }

    private IEnumerator StartDialogueRoutine(string dialogueLine)
    {
        dialogueUI.text = "";
        _charsSinceBlip = 0;

        foreach (char c in dialogueLine)
        {
            dialogueUI.text += c;
            PlayTypeBlip(c);
            yield return new WaitForSeconds(characterDelay);
        }
    }

    private void PlayTypeBlip(char c)
    {
        if (typeSource == null || charactersAudioClip == null) return;
        if (char.IsWhiteSpace(c)) return;                 // no sound on spaces/newlines

        _charsSinceBlip++;
        if (_charsSinceBlip < playEveryNChars) return;    // throttle by char count
        _charsSinceBlip = 0;

        if (Time.unscaledTime - _lastBlipTime < minBlipInterval) return; // throttle by time
        _lastBlipTime = Time.unscaledTime;

        typeSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter); // vary so it doesn't drone
        typeSource.PlayOneShot(charactersAudioClip, typeVolume);
    }
}