using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BrightnessFilter : MonoBehaviour
{
    public Material brightnessMat;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (brightnessMat != null)
        {
            Graphics.Blit(src, dest, brightnessMat);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
