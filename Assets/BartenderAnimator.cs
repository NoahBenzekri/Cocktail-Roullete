using System.Collections;
using UnityEngine;

public class BartenderAnimator : MonoBehaviour
{
    private Animator animator;
    public System.Action OnFlipComplete;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCoinFlip()
    {
        Debug.Log("PlayCoinFlip called, animator: " + animator);
        if (animator == null) return;
        animator.SetTrigger("StartFlip");
        StartCoroutine(WaitForFlipComplete());
    }

    private IEnumerator WaitForFlipComplete()
    {
        // wait for transition out of idle
        yield return new WaitForSeconds(0.2f);

        // wait until we're back in idle
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("rig|Animation_Idle"))
            yield return null;

         yield return new WaitForSeconds(4f); 


        OnFlipComplete?.Invoke();
    }
}