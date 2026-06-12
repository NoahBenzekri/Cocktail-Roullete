using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class MenuTextEffect : MonoBehaviour
{
    public TMP_Text text;

    [Header("Wave")]
    public float waveAmp = 4f;
    public float waveFreq = 2f;
    public float waveSpeed = 3f;

    [Header("Flicker")]
    public float flickerMin = 0.4f;     // dimmest alpha
    public float flickerChance = 0.04f; // per-frame chance to dip
    public float recoverSpeed = 6f;     // how fast it brightens back

    float _alpha = 1f;
    TMP_TextInfo _info;

    void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        text.ForceMeshUpdate();
        _info = text.textInfo;

        // flicker
        if (Random.value < flickerChance)
            _alpha = Random.Range(flickerMin, flickerMin + 0.3f);
        _alpha = Mathf.MoveTowards(_alpha, 1f, recoverSpeed * Time.deltaTime);

        for (int i = 0; i < _info.characterCount; i++)
        {
            var ch = _info.characterInfo[i];
            if (!ch.isVisible) continue;

            int vi = ch.vertexIndex;
            int mi = ch.materialReferenceIndex;
            var verts = _info.meshInfo[mi].vertices;
            var cols = _info.meshInfo[mi].colors32;

            // wave offset per character
            float offset = Mathf.Sin(Time.time * waveSpeed + i / waveFreq) * waveAmp;
            for (int v = 0; v < 4; v++)
            {
                verts[vi + v].y += offset;

                Color32 c = cols[vi + v];
                c.a = (byte)(c.a * _alpha);
                cols[vi + v] = c;
            }
        }

        for (int m = 0; m < _info.meshInfo.Length; m++)
        {
            _info.meshInfo[m].mesh.vertices = _info.meshInfo[m].vertices;
            _info.meshInfo[m].mesh.colors32 = _info.meshInfo[m].colors32;
            text.UpdateGeometry(_info.meshInfo[m].mesh, m);
        }
    }
}