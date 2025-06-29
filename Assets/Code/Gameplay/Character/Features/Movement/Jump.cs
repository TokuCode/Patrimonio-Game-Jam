using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Jump : PlayerFeature
    {
        private PhysicsCheck physics;
        private Crouch crouch;
        private Movement movement;
        private PlayerAnimator animator;
        private Resource resource;
        private Attack attack;
        private Attributes attributes;
    
        [Header("Jump Parameters")]
        [SerializeField] private float _jumpHeight;
        [SerializeField] private float _jumpCooldown;
        [SerializeField] private float _coyoteJumpTime;
        [SerializeField] private float _movementBonus;
        [SerializeField] private float _crouchMultiplier;
    
        [Header("Runtime")]
        [SerializeField] private bool _jumpAction;
        [SerializeField] private float _jumpCooldownTimer;
        [SerializeField] private bool _onDeparture; 
        public bool OnDeparture => _onDeparture;
    
        [Header("Gravity")]
        [SerializeField] private float _maxFallSpeed;
        [SerializeField] private float _fallMultiplier;
        [SerializeField] private float _lowJumpMultiplier;

        public override void ResetFeature(ref SharedProperties shared)
        {
            _onDeparture = false;
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out physics);
            _dependencies.TryGetFeature(out crouch);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out resource);
            _dependencies.TryGetFeature(out attack);
            _dependencies.TryGetFeature(out attributes);
            if (controller is PlayerController player) animator = player.Animator;

            if (controller is PlayerController _player)
            { 
                InputReader.Instance.JumpPressed += TryJump;
            }
        }
        
        public override void UpdateFeature()
        {
            if(_jumpCooldownTimer > 0) _jumpCooldownTimer -= Time.deltaTime;
            else if(!physics.OnGround) _onDeparture = false;
            
            AnimationJump();
        }
    
        public override void FixedUpdateFeature()
        {
            SetGravityUse();
            LimitFallSpeed();
        }

        public override void Apply(ref InputPayload @event)
        {
            GravityHandling(@event.Jump);
        }

        public void AnimationJump()
        {
            var vertical = _invoker.Velocity.Get().y;
            bool overGround = (physics.OnGround || physics.OnSlope) && !_onDeparture;
            animator.SetVertical(overGround ? 0 : vertical);
        }

        private void TryJump()
        {
            if (!gameObject.activeSelf) return;
            
            float timeSinceGround = Time.time - physics.LastGroundTime;
            bool canJumpInternal = _jumpCooldownTimer <= 0 && timeSinceGround <= _coyoteJumpTime;
            bool canJumpExternal = !movement.IsMovementBlocked && resource.AbleToJump && !resource.isStunned;

            if (canJumpInternal && canJumpExternal)
            {
                JumpAction();
                resource.OnJumpStamina();
                _jumpCooldownTimer = _jumpCooldown;
                _onDeparture = true;
            }
        }
    
        private void JumpAction()
        {
            Vector3 velocity = _invoker.Velocity.Get();
            float speed = velocity.magnitude;
            float bonusForce = speed * _movementBonus;
            float multiplier = crouch.IsCrouching ? _crouchMultiplier : 1f;
    
            _invoker.Velocity.Execute(new (velocity.x, 0f, velocity.z));
            _invoker.AddForce.Execute(new(Vector3.up, JumpHeight.ToForce(_jumpHeight + bonusForce) * multiplier, ForceMode.VelocityChange));
        }
    
        private void SetGravityUse()
        {
            _invoker.UseGravity.Execute(!physics.OnSlope && !attack.IsSuspended);
        }
    
        private void LimitFallSpeed()
        {
            if(physics.OnSlope) return;
            
            Vector3 velocity = _invoker.Velocity.Get();
    
            if(Mathf.Abs(velocity.y) > _maxFallSpeed)
                _invoker.Velocity.Execute(new(velocity.x, Mathf.Sign(velocity.y) * _maxFallSpeed, velocity.z));
        }
        
        private void GravityHandling(bool jumpInput)
        {
            if (physics.OnGround || physics.OnSlope || attack.IsSuspended) return;
            
            var velocity = _invoker.Velocity.Get();

            if (velocity.y < 0 || movement.IsMovementBlocked)
                _invoker.AddForce.Execute(new(Vector3.up, Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime, ForceMode.VelocityChange));
            else if (velocity.y > 0 && !jumpInput)
                _invoker.AddForce.Execute(new(Vector2.up, Physics2D.gravity.y * (_lowJumpMultiplier - 1) * Time.fixedDeltaTime, ForceMode.VelocityChange));
        } 
    }
}
