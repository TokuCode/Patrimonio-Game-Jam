using UnityEngine;
using Movement3D.Helpers;

namespace Movement3D.Gameplay
{
    public abstract class Feature : MonoBehaviour, IFeature, IProcess<InputPayload>
    {
        protected Invoker _invoker;
        protected IDependencyManager _dependencies; 
        
        public virtual void InitializeFeature(Controller controller)
        {
            if (controller is PlayerController player)
            {
                _invoker = player.Invoker;
                _dependencies = player.Dependencies;
                player.InputPipeline.Register(this);
            }
        }

        public virtual void UpdateFeature(){ }
        public virtual void FixedUpdateFeature(){ }
        public virtual void Apply(ref InputPayload @event) { }
    }
}