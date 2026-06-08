using System.Collections;
using UnityEngine;

public class OpponentAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayDrink()
    {
        animator.SetTrigger("Drink");
    }

    public void PlayDeath()
    {
        StartCoroutine(DeathDelay());
    }

    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetTrigger("Die");
    }
}