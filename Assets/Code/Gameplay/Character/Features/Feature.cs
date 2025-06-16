using UnityEngine;
using Movement3D.Helpers;

namespace Movement3D.Gameplay
{
    public abstract class Feature : MonoBehaviour, IFeature, IProcess<InputPayload>
    {
        protected IDependencyManager _dependencies; 
        
        public virtual void InitializeFeature(Controller controller)
        {
            _dependencies = controller.Dependencies;
        }

        public virtual void UpdateFeature(){ }
        public virtual void FixedUpdateFeature(){ }
        public virtual void ResetFeature(ref SharedProperties shared) { }
        public virtual void ReInitializeFeature(Controller controller, SharedProperties shared) { }
        public virtual void Apply(ref InputPayload @event) { }
    }
}