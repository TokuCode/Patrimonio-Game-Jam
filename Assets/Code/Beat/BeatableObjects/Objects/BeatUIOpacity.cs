using UnityEngine;
using UnityEngine.UI;

namespace Movement3D.Beat
{
    public class BeatUIOpacity : BeatReaction<float>
    {
        private Image _image;

        protected override void Start()
        {
            _image = GetComponent<Image>();
            base.Start();
        }

        protected override float GetInitialValue() => _image.color.a;

        protected override void ApplyValue(float value) => _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, value);

        protected override void ApplyLerp(float t) => _image.color = new Color(_image.color.r, _image.color.g, _image.color.b,
            Mathf.Lerp(_image.color.a, initialValue, t));
    }
}