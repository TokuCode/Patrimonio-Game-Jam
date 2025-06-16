using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace Movement3D.Gameplay
{
    public class EnemyMovement : EnemyFeature
    {
        public enum MovementType
        {
            Wander,
            Circulate,
            Chase,
            Retreat
        }

        public enum MovementSpeed
        {
            Run,
            Walk,
            Sneak
        }
        
        private EnemyAttributes attributes;
        private EnemyAnimator animator;
        [SerializeField] private MovementType _moveType;
        public MovementType MoveType => _moveType;
        private IMoveState _moveState;
        [SerializeField] private float _delta;
        [SerializeField] private MovementSpeed _speed;
        
        [Header("Sample Rates")]
        [SerializeField] private float _sampleDistance;
        [SerializeField] private float _sampleRateWander;
        private WanderMoveState _wander;
        [SerializeField] private float _sampleRateCirculate;
        private CirculateMoveState _circulate;
        [SerializeField] private float _sampleRateChase;
        private ChaseMoveState _chase;
        [SerializeField] private float _sampleRateRetreat;
        private RetreatMoveState _retreat;
        private Metronome updateTimer;
        private Transform _target;
        public Transform Target => _target;
        private bool _stopped;
        
        [Header("Speeds")]
        [SerializeField] private float _speedWalk;
        [SerializeField] private float _accelerationWalk;
        [SerializeField] private float _speedRun;
        [SerializeField] private float _accelerationRun;
        [SerializeField] private float _speedSneak;
        [SerializeField] private float _accelerationSneak;
        
        public override void ResetFeature(ref SharedProperties shared)
        {
            _target = null;
            SetMoveType(_moveType);
            SetMoveSpeed(_speed);
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out attributes);
            _wander = new(_delta);
            _circulate = new(_delta);
            _chase = new();
            _retreat = new(_delta);
            updateTimer = new(1);
            if (controller is EnemyController enemy)
            {
                animator = enemy.Animator;
            }
            SetMoveType(_moveType);
            SetMoveSpeed(_speed);
        }

        public void SetTarget(Transform target = null)
        {
            _target = target;
        }

        public override void UpdateFeature()
        {
            updateTimer.Update(Time.deltaTime);

            while (updateTimer.ShouldTick() && !_stopped)
            {
                var position = _invoker.Position.Get();
                bool hit = NavMesh.SamplePosition(_moveState.GetNextPosition(position, _target), out var navHit, _sampleDistance, NavMesh.AllAreas);
                if (hit)
                {
                    var direction = (navHit.position - position).With(y: 0).normalized;
                    _invoker.Destination.Execute(navHit.position);
                    _invoker.Forward.Execute(direction);
                }
            }
        }

        public void ResumeMovement()
        {
            if(!_stopped) return;
            
            _invoker.Stop.Execute(false);
            animator.SetBlend(_moveType == MovementType.Chase ? 1 : .6f);
            _stopped = false;
        }

        public void StopMovement()
        {
            if(_stopped) return;
            
            _invoker.Stop.Execute(true);
            animator.SetBlend(0);
            _stopped = true;
        }

        public void SetMoveType(MovementType movementType)
        {
            _moveType = movementType;
            
            switch (movementType)
            {
                case MovementType.Wander:
                    _moveState = _wander;
                    updateTimer.Reset(_sampleRateWander);
                    animator.SetBlend(.6f);
                    break;
                case MovementType.Circulate:
                    _moveState = _circulate;
                    updateTimer.Reset(_sampleRateCirculate);
                    break;
                case MovementType.Chase:
                    _moveState = _chase;
                    updateTimer.Reset(_sampleRateChase);
                    animator.SetBlend(1f);
                    break;
                case MovementType.Retreat:
                    _moveState = _retreat;
                    updateTimer.Reset(_sampleRateRetreat);
                    break;
            }
        }

        public void SetMoveSpeed(MovementSpeed speed)
        {
            _speed = speed;

            float newSpeed = 0;
            float newAcceleration = 0;
            switch (speed)
            {
                case MovementSpeed.Run:
                    newSpeed = _speedRun * attributes.RunSpeed;
                    newAcceleration = _accelerationRun * attributes.RunAcceleration;
                    break;
                case MovementSpeed.Walk:
                    newSpeed = _speedWalk * attributes.Speed;
                    newAcceleration = _accelerationWalk * attributes.Acceleration;
                    break;
                case MovementSpeed.Sneak:
                    newSpeed = _speedSneak;
                    newAcceleration = _accelerationSneak;
                    break;
            }
            
            _invoker.AgentSpeed.Execute(newSpeed);
            _invoker.AgentAcceleration.Execute(newAcceleration);
        }
    }

    public interface IMoveState
    {
        Vector3 GetNextPosition(Vector3 position, Transform target);
    }

    public class ChaseMoveState : IMoveState
    {
        public Vector3 GetNextPosition(Vector3 position, Transform target)
        {
            if(target == null) return position;
            return target.position;
        }
    }

    public class RetreatMoveState : IMoveState
    {
        private float _delta;
        
        public RetreatMoveState(float delta) => _delta = delta;
        
        public Vector3 GetNextPosition(Vector3 position, Transform target)
        {
            if(target == null) return position;
            
            return position - (target.position - position).With(y: 0).normalized * _delta;
        }
    }

    public class WanderMoveState : IMoveState
    { 
        private float _delta;
        
        public WanderMoveState(float delta) => _delta = delta;

        public Vector3 GetNextPosition(Vector3 position, Transform target)
        {
            var random = Random.insideUnitCircle;
            return position + new Vector3(random.x, 0, random.y).normalized * _delta;
        }
    }

    public class CirculateMoveState : IMoveState
    {
        public float _delta;
        
        public CirculateMoveState(float delta) => _delta = delta;

        public Vector3 GetNextPosition(Vector3 position, Transform target)
        {
            if(target == null) return position;
            
            var direction = (position - target.position).With(y: 0).normalized;
            var distance = (position - target.position).With(y: 0).magnitude;
            var angle = _delta / distance * Mathf.Rad2Deg;
            var newDirection = Quaternion.AngleAxis(angle, Vector3.up) * direction;
            
            return newDirection * distance;
        }
    }
}