using UnityEngine;

namespace Movement3D.Beat
{
    public class BeatEmissionPulse : BeatReaction<float>
    {
        private Renderer targetRenderer;
        private Color baseEmissionColor = Color.white;

        private Material mat;

        protected override void Start()
        {
            targetRenderer = GetComponent<Renderer>();
            mat = targetRenderer.material;
            baseEmissionColor = mat.color;
            mat.EnableKeyword("_EMISSION");
            base.Start();
        }

        protected override float GetInitialValue()
        {
            Color emissionColor = mat.GetColor("_EmissionColor");
            return emissionColor.maxColorComponent / baseEmissionColor.maxColorComponent;
        }

        protected override void ApplyValue(float value)
        {
            Color hdrColor = baseEmissionColor * value;
            mat.SetColor("_EmissionColor", hdrColor);
        }

        protected override void ApplyLerp(float t)
        {
            float current = GetCurrentIntensity();
            float lerped = Mathf.Lerp(current, initialValue, t);
            ApplyValue(lerped);
        }

        private float GetCurrentIntensity()
        {
            Color emissionColor = mat.GetColor("_EmissionColor");
            return emissionColor.maxColorComponent / baseEmissionColor.maxColorComponent;
        }
    }
}