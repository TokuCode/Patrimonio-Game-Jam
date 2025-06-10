using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Ladder : Feature
    { 
        private const float ExtraOffsetRaycastForward = .1f;
        private const float ExtraOffsetRaycastUp = .1f;
        private const float StepJumpExtraDistance = 0f;
        private const float MaxAngleOfIntention = 30f;
        private const float StraightWallMaxAngle = 30f;
        private const float VelocityIntentThreshold = 1f;

        private PhysicsCheck physics;
        private Movement movement;

        [Header("Runtime")]
        [SerializeField] private bool _isPathBlocked;
        [SerializeField] private bool _isPathLadderStep;
        private RaycastHit _ladderHit;
        private RaycastHit _stepHit;
        private Vector2 _lastMoveInput;
        private Vector3 _moveDirection;

        [Header("Parameters")]
        [SerializeField] private float _stepSize;
        [SerializeField] private LayerMask _whatIsGround;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out physics);
            _dependencies.TryGetFeature(out movement);
        }

        public override void Apply(ref InputPayload @event)
        {
            if(@event.Context != UpdateContext.FixedUpdate) return;
            
            CalculateMoveDirection(@event.MoveDirection);
            CheckBlockedPath();
            CheckPathLadderJumpeable();
            JumpStep(@event.MoveDirection);
        }

        private void CalculateMoveDirection(Vector2 moveDirection)
        {
            if(moveDirection != Vector2.zero) _lastMoveInput = moveDirection; 
            
            var velocity = _invoker.Velocity.Get();
            var flatVelocity = new Vector3(velocity.x, 0, velocity.y).normalized;

            if (flatVelocity.magnitude > VelocityIntentThreshold)
            {
                _moveDirection = flatVelocity;
                return;
            }

            var forward = _invoker.Forward.Get();
            var right = _invoker.Right.Get();
            var moveIntent = (forward * moveDirection.y + right * moveDirection.x).normalized;

            if (moveIntent != Vector3.zero)
            {
                _moveDirection = moveIntent;
                return;
            }
            
            var lastMoveIntent = (forward * _lastMoveInput.y + right * _lastMoveInput.x).normalized;
            if (lastMoveIntent != Vector3.zero)
            {
                _moveDirection = lastMoveIntent;
            }
        }

        private void CheckBlockedPath()
        {
            var center = _invoker.CenterPosition.Get();
            var radius = _invoker.Radius.Get();
            var height = _invoker.Height.Get();
            
            Vector3 footPoint = center + Vector3.up * (ExtraOffsetRaycastUp - height/2);

            float footStepDistance = ExtraOffsetRaycastForward + radius;

            if(Physics.Raycast(footPoint, _moveDirection, out _stepHit, footStepDistance, _whatIsGround))
            {
                float angle = Vector3.Angle(-_moveDirection, _stepHit.normal);

                _isPathBlocked = angle <= StraightWallMaxAngle;
            }
            else _isPathBlocked = false;
        }

        private void CheckPathLadderJumpeable()
        {
            var center = _invoker.CenterPosition.Get();
            var height = _invoker.Height.Get();
            var radius = _invoker.Radius.Get();
            
            float forwardDistanceRaycast = radius + ExtraOffsetRaycastForward;

            if(_isPathBlocked)
            {
                forwardDistanceRaycast = (_stepHit.point - (center - Vector3.up * (height/2 - ExtraOffsetRaycastUp))).magnitude;
            }

            Vector3 footPoint = center + Vector3.up * (_stepSize + ExtraOffsetRaycastUp - height/2)
                + _moveDirection * (forwardDistanceRaycast + .05f);

            _isPathLadderStep = Physics.Raycast(footPoint, Vector3.down, out _ladderHit, _stepSize + ExtraOffsetRaycastUp - .05f, _whatIsGround);
        }

        private void JumpStep(Vector2 moveInput)
        {
            if(physics.OnSlope) return;

            if(moveInput.magnitude <= 0f || movement.IsMovementBlocked) return;
            
            var center = _invoker.CenterPosition.Get();
            var forward = _invoker.Forward.Get();
            var right = _invoker.Right.Get();
            var height = _invoker.Height.Get();

            Vector3 inputDirection = right * moveInput.x + forward * moveInput.y;
            float angleOfIntention = Vector3.Angle(_moveDirection, inputDirection);

            if(angleOfIntention > MaxAngleOfIntention) return;

            if(_isPathBlocked && _isPathLadderStep)
            {
                float stepHeight = _ladderHit.point.y - center.y + StepJumpExtraDistance + height/2;
                _invoker.AddRigidbodyPositionAdd.Execute(stepHeight);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            
            var center = _invoker.CenterPosition.Get();
            var height = _invoker.Height.Get();
            var radius = _invoker.Radius.Get();
            
            Gizmos.color = _isPathBlocked ? Color.red : Color.green;
            Vector3 footPoint = center + Vector3.up * (ExtraOffsetRaycastUp - height/2);
            float footStepDistance = ExtraOffsetRaycastForward + radius;
            Gizmos.DrawLine(footPoint, footPoint + _moveDirection * footStepDistance);

            Gizmos.color = _isPathLadderStep ? Color.red : Color.green;

            float forwardDistanceRaycast = radius + ExtraOffsetRaycastForward;

            if(_isPathBlocked)
            {
                forwardDistanceRaycast = (_stepHit.point - (center - Vector3.up * (height/2 - ExtraOffsetRaycastUp))).magnitude;
            }

            footPoint = center + Vector3.up * (_stepSize + ExtraOffsetRaycastUp - height/2)
                                        + _moveDirection * (forwardDistanceRaycast + .05f);

            Gizmos.DrawLine(footPoint, footPoint + Vector3.down * (_stepSize + ExtraOffsetRaycastUp - .05f));
        } 
    }
}