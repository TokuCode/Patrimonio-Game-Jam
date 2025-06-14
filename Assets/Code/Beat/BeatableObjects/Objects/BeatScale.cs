using UnityEngine;

namespace Movement3D.Beat
{
    public class BeatScale : BeatReaction<Vector3>
    {
        protected override Vector3 GetInitialValue() => transform.localScale;
    
        protected override void ApplyValue(Vector3 value) => transform.localScale = value;
    
        protected override void ApplyLerp(float t) => transform.localScale = Vector3.Lerp(transform.localScale, initialValue, t);
    }
}