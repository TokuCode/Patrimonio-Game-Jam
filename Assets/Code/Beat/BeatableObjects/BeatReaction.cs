using UnityEngine;

namespace Movement3D.Beat
{
    public abstract class BeatReaction<T> : MonoBehaviour, IBeatReactable
    {
        [SerializeField] protected float returnSpeed = 5f;
        [SerializeField] protected T beatValue;
    
        protected T initialValue;

        protected virtual void Start() => initialValue = GetInitialValue();
    
        protected virtual void Update() => ApplyLerp(Time.deltaTime * returnSpeed);

        public void OnBeat() => ApplyValue(beatValue);
    
        protected abstract T GetInitialValue();
        protected abstract void ApplyValue(T value);
        protected abstract void ApplyLerp(float t);
    }
}