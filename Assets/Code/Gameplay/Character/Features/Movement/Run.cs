using System.Collections;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Run : Feature
    {
        public enum State
        {
            Walking,
            Crouching,
            Running,
            Blocked,
            Dashing,
            Charging
        }
        
        private Crouch crouch;
        private PhysicsCheck physics;
        private Movement movement;
        private PlayerAnimator animator;
        private Resource resource;
        private Attack attack;

        [Header("Runtime")]
        [SerializeField] private float _currentMaxSpeed;
        public float MaxSpeed => _currentMaxSpeed;
        [SerializeField] private float _currentAcceleration;
        public float Acceleration => _currentAcceleration;
        [SerializeField] private float _desiredMaxSpeed;
        [SerializeField] private float _lastDesiredMaxSpeed;
        [SerializeField] private State _state;
        public State MoveState => _state;
        private Coroutine _speedControl;
        [SerializeField] private bool _isRunning;
        public bool IsRunning => _isRunning && _invoker.Velocity.Get().With(y: 0).magnitude > 1;

        [Header("Walking")]
        [SerializeField] private float _walkMaxSpeed;
        [SerializeField] private float _walkAcceleration;

        [Header("Running")]
        [SerializeField] private float _runMaxSpeed;
        [SerializeField] private float _runAcceleration;

        [Header("Crouching")]
        [SerializeField] private float _crouchMaxSpeed;
        [SerializeField] private float _crouchAcceleration;

        [Header("Additional Configurations")]
        [SerializeField] private float _timeSmoothing;
        [SerializeField] private float _smoothingThreshold;
        [SerializeField] private float _coyoteRunTime;
        
        [Header("Dashing")]
        [SerializeField] private float _dashMaxSpeed;
        
        [Header("Charging")]
        [SerializeField] private float _chargeMaxSpeed;
        [SerializeField] private float _chargeAcceleration;

        public override void ResetFeature(ref SharedProperties shared)
        {
            if(_speedControl != null) StopCoroutine(_speedControl);
            _isRunning = false;
        }

        public override void ReInitializeFeature(Controller controller, SharedProperties shared)
        {
            StateManager(true);
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            
            _dependencies.TryGetFeature(out crouch);
            _dependencies.TryGetFeature(out physics);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out resource);
            _dependencies.TryGetFeature(out attack);
            if (controller is PlayerController player) animator = player.Animator;
            
            StateManager(true);
        }

        public override void Apply(ref InputPayload @event)
        {
            if(@event.Context != UpdateContext.Update) return;
            
            TryRun(@event.Run);
            AnimationMove(@event.MoveDirection);
        }

        private void TryRun(bool runInput)
        {
            if(runInput && !crouch.IsCrouching && Time.time - physics.LastGroundTime <= _coyoteRunTime && resource.AbleToRun) _isRunning = true;
            else if(!runInput || !resource.AbleToRun) _isRunning = false;
        }

        public override void UpdateFeature()
        {
            StateManager();
        }

        public void AnimationMove(Vector2 moveIntent)
        {
            float speed = _invoker.Velocity.Get().With(y: 0).magnitude;

            float blend = 0;
            if (moveIntent != Vector2.zero && speed > .1)
            {
                if (_isRunning ) blend = 1;
                else if (!_isRunning && moveIntent != Vector2.zero) blend = .6f;
            }
            
            animator.SetBlend(blend);
        }

        private void StateManager(bool disableTransition = false)
        {
            float acceleration = _currentAcceleration;
            bool enableTransition = false;

            if (movement.IsMovementBlocked)
            {
                _state = State.Blocked;
            }
            
            else if (attack.IsDashing)
            {
                _state = State.Dashing;
                _desiredMaxSpeed = _dashMaxSpeed;
            }
            
            else if (attack.Charging)
            {
                _state = State.Charging;
                _desiredMaxSpeed = _chargeMaxSpeed;
                acceleration = _chargeAcceleration;
                enableTransition = true;
            }

            else if(crouch.IsCrouching)
            {
                _state = State.Crouching;
                
                _desiredMaxSpeed = _crouchMaxSpeed;
                acceleration = _crouchAcceleration;
                enableTransition = true;
            }

            else if(_isRunning)
            {
                _state = State.Running;
                
                _desiredMaxSpeed = _runMaxSpeed;
                acceleration = _runAcceleration;
                enableTransition = true;
            }

            else
            {
                _state = State.Walking;
                
                _desiredMaxSpeed = _walkMaxSpeed;
                acceleration = _walkAcceleration;
                enableTransition = true;
            }
            
            enableTransition &= !disableTransition;
            
            _currentAcceleration = acceleration;
            
            if(_lastDesiredMaxSpeed <= 0) _lastDesiredMaxSpeed = _desiredMaxSpeed;

            if(Mathf.Abs(_desiredMaxSpeed - _lastDesiredMaxSpeed) > _smoothingThreshold && enableTransition)
            {
                if(_speedControl != null) StopCoroutine(_speedControl);
                _speedControl = StartCoroutine(SmoothlyLerpMaxSpeed());
            } else _currentMaxSpeed = _desiredMaxSpeed;

            _lastDesiredMaxSpeed = _desiredMaxSpeed;
        }

        private IEnumerator SmoothlyLerpMaxSpeed()
        {
            float time = 0f;
            float difference = Mathf.Abs(_desiredMaxSpeed - _currentMaxSpeed) * _timeSmoothing;
            float startValue = _currentMaxSpeed;

            while(time < difference)
            {
                _currentMaxSpeed = Mathf.Lerp(startValue, _desiredMaxSpeed, time/difference);
                time += Time.deltaTime;

                yield return null;
            }

            _currentMaxSpeed = _desiredMaxSpeed;
        }
    }
}