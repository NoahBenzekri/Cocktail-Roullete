using System.Collections;
using UnityEngine;

public class OpponentAnimator : MonoBehaviour
{
    public Animator animator;

    [Header("State Names")]
    public string drinkState = "Opponent_character_rig|Drink";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("[OpponentAnimator] No Animator found!", this);
    }
  public void ReturnToIdle()
    {
        if (animator == null) return;
        animator.Play("Opponent_character_rig|Idle", 0, 0f);
    }
    public void ResetTriggers()
    {
        if (animator == null) return;
        animator.ResetTrigger("Die");
        animator.ResetTrigger("Drink");
    }

    public void PlayDrink()
    {
        if (animator == null) return;
        animator.SetTrigger("Drink");
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.SetTrigger("Die");
    }

    public IEnumerator WaitForDrinkThenDie()
    {
        if (animator == null) yield break;

        // wait until we're actually in the drink state
        float t = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(drinkState) && t < 1f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // wait until the drink clip has fully played
        t = 0f;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(drinkState) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f &&
               t < 6f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        animator.SetTrigger("Die");
    }
}