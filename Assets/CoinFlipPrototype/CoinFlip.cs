using UnityEngine;

public class CoinFlip : MonoBehaviour
{
    public enum CoinSide { Tails, Heads };
    public CoinSide coinSide;

    public Rigidbody coinRigidbody;

    public GameObject triggers;

    private void Start()
    {
        if (coinRigidbody != null)
        {
            int jumpForce = Random.Range(100, 200);
            coinRigidbody.AddForce(0, jumpForce, 0);

            int torqX = Random.Range(20, 100);
            int torqZ = Random.Range(20, 100);
            coinRigidbody.AddTorque(torqX, 0, torqZ);


        }
    }

    public void ChooseHeads()
    {
        SwitchSide(CoinSide.Heads);
    }

    public void ChooseTails()
    {
        SwitchSide(CoinSide.Tails);
    }

    private void SwitchSide(CoinSide side)
    {
        coinSide = side;

        switch (coinSide)
        {
            case CoinSide.Tails:
                Debug.Log("Tails");
                break;
            case CoinSide.Heads:
                Debug.Log("Heads");
                break;
        }
    }
}
