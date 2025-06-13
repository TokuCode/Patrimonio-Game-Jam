using Movement3D.Helpers;
using Unity.Cinemachine;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class FirstPersonCamera : Feature
    {
        private Movement movement;
        private Resource resource;
        private CinemachineInputAxisController _input;
        private Transform _lookAtFollow;
        private Transform _lookAt;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out resource);
            if (controller is PlayerController playerController)
            {
                _lookAtFollow = playerController.LookAtFollow;
                _lookAt = playerController.LookAt;
            }
        }
        
        public void SetInput(CinemachineInputAxisController input) => _input = input;

        public override void UpdateFeature()
        {
            if(!enabled) return;
            
            _input.enabled = !resource.isStunned && !movement.IsMovementBlocked;
        }

        public void SetOrientation()
        {
            var direction = (_lookAtFollow.position - _lookAt.position).With(y: 0).normalized;
            
            _invoker.Forward.Execute(direction);
            _invoker.PlayerForward.Execute(direction);
        }
    }
}