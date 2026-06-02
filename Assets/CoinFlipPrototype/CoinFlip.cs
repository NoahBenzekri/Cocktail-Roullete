using System.Collections;
using UnityEngine;

public class CoinFlip : MonoBehaviour
{
    public enum CoinSide { Tails, Heads }

    public Rigidbody coinRigidbody;

    [Header("Which axis is the 'heads' face pointing up?")]
    public Vector3 headsUpAxis = Vector3.up;

    [HideInInspector] public System.Action<bool> OnCoinSettled;

    private bool _launched = false;
    private bool _settled = false;


    public void Launch()
    {
        _launched = false;
        _settled = false;

        if (coinRigidbody == null) return;

        coinRigidbody.linearVelocity = Vector3.zero;
        coinRigidbody.angularVelocity = Vector3.zero;

        coinRigidbody.AddForce(0, Random.Range(100, 200), 0);
        coinRigidbody.AddTorque(Random.Range(20, 100), 0, Random.Range(20, 100));

        _launched = true;
        StartCoroutine(WaitForSettle());
    }

    private IEnumerator WaitForSettle()
    {
        yield return new WaitForSeconds(0.3f);

        float settleThreshold = 0.5f;
        float requiredSettleTime = 0.2f;
        float maxWaitTime = 6f;
        float settleTime = 0f;
        float elapsed = 0f;

        while (settleTime < requiredSettleTime)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= maxWaitTime) break;

            bool stillMoving = coinRigidbody != null &&
                               (coinRigidbody.linearVelocity.magnitude > settleThreshold ||
                                coinRigidbody.angularVelocity.magnitude > settleThreshold);

            if (!stillMoving)
                settleTime += Time.deltaTime;
            else
                settleTime = 0f;

            yield return null;
        }

        if (_settled) yield break;
        _settled = true;

        bool isHeads = Vector3.Dot(transform.TransformDirection(headsUpAxis), Vector3.up) > 0f;

        Debug.Log($"[CoinFlip] Settled — {(isHeads ? "Heads" : "Tails")}. Player gets choice: {isHeads}");

        OnCoinSettled?.Invoke(isHeads);
    }
}