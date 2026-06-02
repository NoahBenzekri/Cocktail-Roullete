using UnityEngine;

public class TriggerCup : MonoBehaviour
{
    [SerializeField] private MeshRenderer liquidMaterialMesh;
    [SerializeField] private float fillSpeed = 0.1f;
    [SerializeField] private float maxFill = 1.0f;
    [SerializeField] private float minFill = 0.0f;

    private float currentFill = 0.0f;
    private Material runtimeMaterial;

    void Start()
    {
        // .material creates an instance clone to prevent modifying the asset file
        runtimeMaterial = liquidMaterialMesh.material;

        currentFill = minFill;
        runtimeMaterial.SetFloat("_Fill", currentFill);
    }

    void OnParticleCollision(GameObject other)
    {
        if (currentFill < maxFill)
        {
            currentFill += fillSpeed * Time.deltaTime;
            currentFill = Mathf.Clamp(currentFill, minFill, maxFill);
            runtimeMaterial.SetFloat("_Fill", currentFill);
        }
    }

    void OnDestroy()
    {
        // Clean up the instantiated material to prevent memory leaks
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
