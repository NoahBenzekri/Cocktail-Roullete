using UnityEngine;

public class Pour : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleEffect;

    public int pourThreshold = 45;

    private bool isPouring = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _particleEffect = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        bool pourCheck = CalculatePourAngle() < pourThreshold;

        if (isPouring != pourCheck)
        {
            isPouring = pourCheck;

            if (isPouring)
            {
                Debug.Log("Pouring");

                _particleEffect.Play();
            }
            else
            {
                Debug.Log("Stopped Pouring");

                _particleEffect.Stop();
            }
        }
    }

    private float CalculatePourAngle()
    {
        return transform.up.y * Mathf.Rad2Deg;
    }

       
}
