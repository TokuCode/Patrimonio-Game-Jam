using UnityEngine;

namespace Movement3D.Gameplay
{
    public class PlayerFeature : Feature
    {
        protected PlayerInvoker _invoker;
        
        public virtual void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            if (controller is PlayerController player)
            {
                _invoker = player.Invoker;
                player.InputPipeline.Register(this);
            }
        } 
    }
}