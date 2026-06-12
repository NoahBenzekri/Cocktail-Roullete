using UnityEngine;
using System.Collections;
public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;
    public string drinkState = "Opponent_character_rig|Player_EndDrink";

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }
public void ResetTriggers()
{
    if (animator == null) return;
    animator.ResetTrigger("Die");
    animator.ResetTrigger("Drink");
}
    public void PlayDrink()
    {
        animator.SetFloat("DrinkDir", 1f);
        animator.SetTrigger("Drink");
    }

    public void PlayDeath()
    {
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