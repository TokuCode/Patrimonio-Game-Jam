using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Movement : PlayerFeature
    {
        private PhysicsCheck physics;
        private Jump jump;
        private Run run;
        private Resource resource;
    
        [Header("Movement Parameters")]
        [SerializeField] private float _airMultiplier;
        [SerializeField] private float _airFriction;
        [SerializeField] private float _groundFriction;
    
        public bool isTurningForward(Vector3 velocity, Vector3 forward, Vector3 moveDirection) => Vector3.Dot(velocity, forward) * moveDirection.y < 0f;
        public bool isTurningRight(Vector3 velocity, Vector3 right, Vector3 moveDirection) => Vector3.Dot(velocity, right) * moveDirection.x < 0f;

        private bool _isMovementBlocked;
        public bool IsMovementBlocked => _isMovementBlocked;

        public override void ResetFeature(ref SharedProperties shared)
        {
            Free();
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out physics);
            _dependencies.TryGetFeature(out jump);
            _dependencies.TryGetFeature(out run);
            _dependencies.TryGetFeature(out resource);
        }
    
        public override void FixedUpdateFeature()
        {
            TransitionSlopeToGround();
            LimitSpeed();
        }

        public override void Apply(ref InputPayload @event)
        {
            if (@event.Context != UpdateContext.FixedUpdate) return;
            
            var moveDirection = @event.MoveDirection;
            Move(moveDirection);
            Friction(moveDirection);
        }

        private void Move(Vector2 moveDirection)
        {
            if (_isMovementBlocked || resource.isStunned) return;
            
            Vector3 forward = _invoker.Forward.Get();
            Vector3 right = _invoker.Right.Get();
            Vector3 velocity = _invoker.Velocity.Get();
            
            Vector3 direction = forward * moveDirection.y + right * moveDirection.x;
    
            if(physics.OnSlope && !jump.OnDeparture)
            {
                direction = physics.ProjectOnSlope(direction);
                _invoker.AddForce.Execute(new (direction, 20f * run.Acceleration, ForceMode.Acceleration));
    
                float speedOnNormalDirection = Vector3.Dot(velocity, physics.SlopeNormal);
                if(speedOnNormalDirection > 0)
                {
                    _invoker.AddForce.Execute(new AddForceParams(-physics.SlopeNormal, 40f, ForceMode.Acceleration));
                }
    
                return;
            }
    
            _invoker.AddForce.Execute(new AddForceParams(direction.normalized, 10f * (physics.OnGround ? 1f : _airMultiplier) * run.Acceleration, ForceMode.Acceleration));
        }
    
        private void LimitSpeed()
        {
            Vector3 velocity = _invoker.Velocity.Get();
            
            if(physics.OnSlope && !jump.OnDeparture)
            {
                if(velocity.magnitude > run.MaxSpeed)
                    _invoker.Velocity.Execute(velocity.normalized * run.MaxSpeed);
    
                return;
            }
    
            Vector3 flatVelocity = new (velocity.x, 0f, velocity.z);
    
            if(flatVelocity.magnitude > run.MaxSpeed)
            {
                flatVelocity = flatVelocity.normalized * run.MaxSpeed;
                _invoker.Velocity.Execute(new (flatVelocity.x, velocity.y, flatVelocity.z));
            }
        }
    
        private void Friction(Vector2 moveDirection)
        {
            if (_isMovementBlocked) moveDirection = Vector2.zero;
            
            Vector3 drag = Vector3.zero;
            float dragFactor = physics.OnGround ? _groundFriction : _airFriction;
            Vector3 forward = _invoker.Forward.Get();
            Vector3 right = _invoker.Right.Get();
            Vector3 velocity = _invoker.Velocity.Get();
    
            if(isTurningRight(velocity, right, moveDirection) || moveDirection.x == 0) drag += Vector3.right * dragFactor;
            if(isTurningForward(velocity, right, moveDirection) || moveDirection.y == 0) drag += Vector3.forward * dragFactor;
    
            ApplyDrag(drag);
        }
    
        private void ApplyDrag(Vector3 directionalDrag)
        {
            Vector3 forward = _invoker.Forward.Get();
            Vector3 right = _invoker.Right.Get();
            var up = Vector3.Cross(forward, right).normalized;
            Vector3 velocity = _invoker.Velocity.Get();
            
            Vector3 velocityForward = Vector3.Project(velocity, forward);
            Vector3 velocityRight = Vector3.Project(velocity, right);
            Vector3 velocityUp = Vector3.Project(velocity, up);
    
            Vector3 dragForward = -velocityForward * (directionalDrag.z * Time.fixedDeltaTime);
            Vector3 dragRight = -velocityRight * (directionalDrag.x * Time.fixedDeltaTime);
            Vector3 dragUp = -velocityUp * (directionalDrag.y * Time.fixedDeltaTime);
    
            _invoker.AddForce.Execute(new (dragForward + dragRight + dragUp, ForceMode.VelocityChange));
        }
    
        private void TransitionSlopeToGround()
        {
            if(physics.PreviousOnSlope && !physics.OnSlope && !jump.OnDeparture)
            {
                _invoker.AddForce.Execute(new (-Vector3.up, 40f, ForceMode.VelocityChange));
            }
        }
        
        public void Block() => _isMovementBlocked = true;
        public void Free() => _isMovementBlocked = false;
    }
}