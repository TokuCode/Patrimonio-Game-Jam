using UnityEngine;

public class ThresholdMergeBlitter : MonoBehaviour
{
    public Material thresholdMaterial;
    public RenderTexture renderPre;   // lo nuevo (frame actual)
    public RenderTexture renderPost;  // lo acumulado (donde se escribe)

    private RenderTexture tempBuffer;

    void Update()
    {
        // Asegurar que el buffer temporal tenga el mismo tamaño
        if (tempBuffer == null || tempBuffer.width != renderPost.width || tempBuffer.height != renderPost.height)
        {
            if (tempBuffer != null) tempBuffer.Release();
            tempBuffer = new RenderTexture(renderPost.width, renderPost.height, 0, renderPost.format);
            tempBuffer.Create();
        }

        // 1. Copiar renderPost a buffer temporal
        Graphics.Blit(renderPost, tempBuffer);

        // 2. Setear texturas para el shader
        thresholdMaterial.SetTexture("_PreTex", renderPre);
        thresholdMaterial.SetTexture("_PostTex", tempBuffer); // usamos la copia, no el destino

        // 3. Blittear desde shader hacia renderPost
        Graphics.Blit(null, renderPost, thresholdMaterial);
    }

    void OnDestroy()
    {
        if (tempBuffer != null) tempBuffer.Release();
    }
}
