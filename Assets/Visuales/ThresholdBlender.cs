using UnityEngine;

public class ThresholdBlender : MonoBehaviour
{
    [Header("Material con el shader 'ThresholdMerge'")]
    public Material blendMaterial;

    [Header("Texturas Render")]
    public RenderTexture renderPre;
    public RenderTexture renderPost;

    [Header("Parámetros")]
    [Range(0, 1)]
    public float threshold = 0.5f;
    public float scrollSpeed = 0.1f;

    void Start()
    {
        if (blendMaterial != null)
        {
            blendMaterial.SetTexture("_PreTex", renderPre);
            blendMaterial.SetTexture("_PostTex", renderPost);
        }
    }

    void Update()
    {
        // Cambia el threshold con scroll del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            threshold = Mathf.Clamp01(threshold + scroll * scrollSpeed);
            blendMaterial.SetFloat("_Threshold", threshold);
        }
    }
}
