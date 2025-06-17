using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DynamicNoiseTexture : MonoBehaviour
{
    public int resolution = 256;
    public float scale = 10f;
    public float speed = 1f;

    private Texture2D noiseTexture;
    private Material material;
    private float offsetX = 0f;
    private float offsetY = 0f;

    void Start()
    {
        // Crear textura
        noiseTexture = new Texture2D(resolution, resolution, TextureFormat.RGFloat, false);
        noiseTexture.wrapMode = TextureWrapMode.Repeat;
        noiseTexture.filterMode = FilterMode.Bilinear;

        // Obtener material del objeto
        material = GetComponent<Renderer>().material;

        // Asignar al shader
        material.SetTexture("_NoiseTex", noiseTexture);
    }

    void Update()
    {
        offsetX += Time.deltaTime * speed;
        offsetY += Time.deltaTime * speed;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / resolution * scale + offsetX;
                float v = (float)y / resolution * scale + offsetY;

                float noise = Mathf.PerlinNoise(u, v);
                Color c = new Color(noise, noise, noise, 1.0f); // RG mismo valor

                noiseTexture.SetPixel(x, y, c);
            }
        }

        noiseTexture.Apply();
    }
}
