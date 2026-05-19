using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private string dialogueLine;

    [SerializeField] private TextMeshProUGUI dialogueUI;

    [SerializeField] private CanvasGroup dialogueCanvasGroup;

    [SerializeField] private float characterDelay = 0.3f;

    [SerializeField] private float fadeDelay = 2f;

    [SerializeField] private Button[] choiceButtons = new Button[2];

    [SerializeField] private AudioClip charactersAudioClip;

    private void Start()
    {
        StartCoroutine(FadeCanvasGroup());
        StartCoroutine(StartDialogueRoutine());
    }
    private IEnumerator FadeCanvasGroup()
    {
        float delay = 0;

        while (delay < fadeDelay)
        {
            delay += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(0, 1, delay / fadeDelay);
            yield return null;
        }
    }

    private IEnumerator StartDialogueRoutine()
    {
   

        foreach (char c in dialogueLine)
        {

            dialogueUI.text += c;
            AudioSource.PlayClipAtPoint(charactersAudioClip, Camera.main.transform.position);
            yield return new WaitForSeconds(characterDelay);
        }

        
    }
}
