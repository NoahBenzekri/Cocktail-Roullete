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

    [SerializeField] private AudioClip charactersAudioClip;

    private Coroutine typeCoroutine;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (!string.IsNullOrEmpty(dialogueLine))
        {
            StartDialogue(dialogueLine);
        }
    }

    public void StartDialogue(string dialogue)
    {
        if(typeCoroutine != null)
            StopCoroutine(typeCoroutine);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvasGroup(1f, fadeDelay));
        typeCoroutine = StartCoroutine(StartDialogueRoutine(dialogue));
    }

    public void ClearDialogue()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

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

        foreach (char c in dialogueLine)
        {

            dialogueUI.text += c;
            if(charactersAudioClip != null)
                AudioSource.PlayClipAtPoint(charactersAudioClip, Camera.main.transform.position);

            yield return new WaitForSeconds(characterDelay);
        }
    }

  

   
}
