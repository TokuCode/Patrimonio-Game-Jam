using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Crouch : PlayerFeature
    {
        private PhysicsCheck physics;
        private Movement movement;
        private PlayerAnimator animator;
        private Resource resource;

        [Header("Extra Settings")] 
        [SerializeField] private bool _crounchEnabled;
        [SerializeField] private float _crouchColliderCenter;
        private float _startCrouchColliderCenter;

        [Header("Runtime")]
        [SerializeField] private bool _isCrouching;
        public bool IsCrouching => _isCrouching;
        private float _startYScale;
        private float _startHeight;

        [Header("Parameters")]
        [SerializeField] private float _crouchHeightMultiplier;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out physics);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out resource);
            if (controller is PlayerController player) animator = player.Animator;
            
            _startYScale = _invoker.LocalScale.Get().y;
            _startHeight = _invoker.Height.Get();
            _startCrouchColliderCenter = _invoker.ColliderCenter.Get();
        }

        public override void ResetFeature(ref SharedProperties shared)
        {
            UncrouchAction();
        }

        public override void Apply(ref InputPayload @event)
        {
            if(@event.Context != UpdateContext.Update) return;
            
            CheckCrouching(@event.Crouch);
        }

        private void CheckCrouching(bool crouchInput)
        {
            bool canCrouch = !movement.IsMovementBlocked && !resource.isStunned && _crounchEnabled;
            
            if(crouchInput && canCrouch && !_isCrouching)
                CrouchAction();
            else if((!crouchInput || !canCrouch) && _isCrouching && !physics.BlockedHead)
                UncrouchAction();
        }

        private void CrouchAction()
        {
            Vector3 localScale = _invoker.LocalScale.Get();
            
            _invoker.ReferencesLocalScale.Execute(new (localScale.x, _startYScale * _crouchHeightMultiplier, localScale.z));
            _invoker.Height.Execute(_startHeight * _crouchHeightMultiplier);
            _invoker.ColliderCenter.Execute(_crouchColliderCenter);
            
            animator.SetCrouch(true);

            _isCrouching = true;
        }

        private void UncrouchAction()
        {
            Vector3 localScale = _invoker.LocalScale.Get();
            
            _invoker.ReferencesLocalScale.Execute(new (localScale.x, _startYScale, localScale.z));
            _invoker.Height.Execute(_startHeight);
            _invoker.ColliderCenter.Execute(_startCrouchColliderCenter);
            
            animator.SetCrouch(false);

            _isCrouching = false;
        }
    }
}