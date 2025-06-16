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
        
        [SerializeField] private float _detectionRadius;
        [SerializeField] private float _attackRange;
        [SerializeField] private int _hitsToSpecial;
        [SerializeField] private float _attackCooldown;
        private CountdownTimer _attackCooldownTimer;
        private bool _cooldown;
        private float _hitCount;
        private bool _inAttackRange;
        private bool _playerDetected;
        private Transform _playerTransform;
        private bool _isAttacking;
        private bool _special;

        private void OnEnable()
        {
            MulticharacterController.Instance.OnSwitchCharacter += OnSwitchCharacter;
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out attack);
            _dependencies.TryGetFeature(out combo);
            _dependencies.TryGetFeature(out resource);
            attack.OnEndAttack += OnAttackEnd;
            attack.OnStartAttack += OnAttackStart;
            _attackCooldownTimer = new(_attackCooldown);
            _attackCooldownTimer.OnTimerStop += () => { _cooldown = false; };
        }

        public override void UpdateFeature()
        {
            _attackCooldownTimer.Tick(Time.deltaTime);
            CheckIfPlayerDetected();
            CheckIfPlayerInRange();
            Attack();
            _special = _hitCount >= _hitsToSpecial;
            IfAttacking();
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

        public void Attack()
        {
            if(!_inAttackRange || _playerTransform == null || _isAttacking || _cooldown) return; 
            
            _isAttacking = true;
        }

        public void IfAttacking()
        {
            if (!_isAttacking || _cooldown) return;
            
            string signal = _special ? "special-neutral-ground" : "attack-neutral-ground";
            combo.ProcessInput(signal);
        }

        public void OnAttackEnd()
        {
            if(!resource.isStunned) movement.ResumeMovement();
            _attackCooldownTimer.Start();
            _cooldown = true;
        }

        public void OnAttackStart()
        {
            _hitCount++;
            _hitCount %= _hitsToSpecial;
            movement.StopMovement();
            _isAttacking = false;
        }
    }
}