using UnityEngine;

public class BrightnessThresholdController : MonoBehaviour
{
    public Material brightnessMaterial;
    [Range(0f, 1f)]
    public float threshold = 0.5f;
    public float scrollSpeed = 0.1f;

    void Update()
    {
        // Leer scroll del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Modificar threshold
            threshold += scroll * scrollSpeed;
            threshold = Mathf.Clamp01(threshold); // Limita entre 0 y 1

            // Enviar valor al material
            brightnessMaterial.SetFloat("_Threshold", threshold);
        }
    }
}
