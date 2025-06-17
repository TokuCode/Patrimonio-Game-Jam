using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyBrain : EnemyFeature
    {
        private EnemyMovement movement;
        private EnemyAttack attack;
        private EnemyComboReader combo;
        private EnemyResource resource;
        private EnemyPhysicsCheck check;
        
        [SerializeField] private float _detectionRadius;
        [SerializeField] private float _attackRange;
        [SerializeField] private int _hitsToSpecial;
        [SerializeField] private float _attackCooldown;
        [SerializeField] private float _resumingThreshold;
        private CountdownTimer _attackCooldownTimer;
        [Header("Runtime")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private bool _cooldown;
        [SerializeField] private bool _special;
        [SerializeField] private float _hitCount;
        [SerializeField] private bool _inAttackRange;
        [SerializeField] private bool _playerDetected;
        [SerializeField] private bool _isStunned;

        public bool IsStunned
        {
            get => _isStunned;
            set => _isStunned = value;
        }

        [SerializeField] private bool _isAttacking;
        [SerializeField] private bool _isDead;

        private void OnEnable()
        {
            MulticharacterController.Instance.OnSwitchCharacter += OnSwitchCharacter;
        }

        public override void ResetFeature(ref SharedProperties shared)
        {
            _attackCooldownTimer.Stop();
            _cooldown = false;
            _playerDetected = false;
            _isAttacking = false;
            _isStunned = false;
            _special = false;
            _isStunned = false;
            _isDead = false;
        }

        public override void ReInitializeFeature(Controller controller, SharedProperties shared)
        {
            _playerTransform = MulticharacterController.Instance.CurrentPlayer.transform;
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out attack);
            _dependencies.TryGetFeature(out combo);
            _dependencies.TryGetFeature(out resource);
            _dependencies.TryGetFeature(out check);
            attack.OnEndAttack += OnAttackEnd;
            attack.OnStartAttack += OnAttackStart;
            _attackCooldownTimer = new(_attackCooldown);
            _attackCooldownTimer.OnTimerStop += () => { _cooldown = false; };
            resource.OnStun += () => { _isStunned = true; };
            resource.OnDie += () => { _isDead = true; };
        }

        public override void UpdateFeature()
        {
            _attackCooldownTimer.Tick(Time.deltaTime);
            CheckIfPlayerDetected();
            CheckIfPlayerInRange();
            Attack();
            _special = _hitCount >= _hitsToSpecial - 1;
            IfAttacking();
            CheckForResumeMovement();
        }

        public void OnSwitchCharacter(PlayerController player)
        {
            _playerTransform = player.transform;
            movement.SetTarget(_playerTransform);
        }

        public void CheckIfPlayerDetected()
        {
            if (_playerDetected || _playerTransform == null) return;

            var position = _invoker.Position.Get();
            _playerDetected = Vector3.Distance(position, _playerTransform.position) <= _detectionRadius;

            if (!_playerDetected) return;
            
            movement.SetMoveType(EnemyMovement.MovementType.Chase);
            movement.SetMoveSpeed(EnemyMovement.MovementSpeed.Run);
        }

        public void CheckIfPlayerInRange()
        {
            if (!_playerDetected || _playerTransform == null) return;

            var position = _invoker.Position.Get();
            _inAttackRange = Vector3.Distance(position, _playerTransform.position) <= _attackRange;
        }

        public void CheckForResumeMovement()
        {
            if(!check.OnGround || _isAttacking || _isDead) return;
            
            var velocity = _invoker.Velocity.Get().magnitude;
            
            if(velocity > _resumingThreshold) return;

            movement.ResumeMovement();
        }

        public void Attack()
        {
            if(!_inAttackRange || _playerTransform == null || _isAttacking || _cooldown || _isStunned) return; 
            
            _isAttacking = true;
            movement.StopMovement();
        }

        public void IfAttacking()
        {
            if (!_isAttacking || _cooldown) return;
            
            string signal = _special ? "special-neutral-ground" : "attack-neutral-ground";
            combo.ProcessInput(signal);
        }

        public void OnAttackEnd()
        {
            _attackCooldownTimer.Start();
            _cooldown = true;
        }

        public void OnAttackStart()
        {
            _hitCount++;
            _hitCount %= _hitsToSpecial;
            _isAttacking = false;
        }
    }
}