using System;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyResource : EnemyFeature
    {
        private EnemyAttack attack;
        private FloatingText damageNumber;
        private EnemyAnimator animator; //Change
        private EnemyComboReader combo;
        private EnemyAttributes attributes;
        private EnemyMovement movement;
        private SphereCollider _worldBounds;
        private float _worldLimit;

        [Header("Health")] 
        [SerializeField] private float _maxHealth;
        public float MaxHealth => _maxHealth;
        [SerializeField] private float _currentHealth;
        public float CurrentHealth => _currentHealth;
        public bool isDead { get; private set; }
        public bool isInvincible { get; private set; }
        public CountdownTimer _invincibleTimer;
        [SerializeField] public float _invincibilityTime;
        public Pipeline<HitInfo> pipeline { get; private set; } = new();
        public bool isStunned { get; private set; }
        private CountdownTimer _stunTimer;

        public event Action<Delta> OnHealthChanged;
        public event Action OnStun;
        public event Action OnDie;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _worldBounds = GameObject.FindGameObjectWithTag("WorldBounds").GetComponent<SphereCollider>();
            _worldLimit = GameObject.FindGameObjectWithTag("WorldLimit").transform.position.y;
            _dependencies.TryGetFeature(out attack);
            _dependencies.TryGetFeature(out combo);
            _dependencies.TryGetFeature(out attributes);
            _dependencies.TryGetFeature(out movement);
            _currentHealth = _maxHealth;
            _invincibleTimer = new CountdownTimer(_invincibilityTime);
            _invincibleTimer.OnTimerStop = () => { isInvincible = false; };
            _stunTimer = new CountdownTimer(1f);
            _stunTimer.OnTimerStop = () =>
            {
                isStunned = false;
            };
            if (controller is EnemyController enemy)
            {
                damageNumber = enemy.DamageNumber;
                animator = enemy.Animator;
            }
        }

        public override void ResetFeature(ref SharedProperties shared)
        {
            if (shared is not PlayerSharedProperties playerShared) return;

            playerShared.healthRatio = Mathf.Clamp01(_currentHealth / _maxHealth);
            isInvincible = false;
            isStunned = false;
            _invincibleTimer.Stop();
            _stunTimer.Stop();
        }

        public override void UpdateFeature()
        {
            _invincibleTimer.Tick(Time.deltaTime);
            _stunTimer.Tick(Time.deltaTime);
            
            CheckOutOfBounds();
        }

        private void CheckOutOfBounds()
        {
            var position = _invoker.Position.Get();
            var distance = Vector3.Distance(position, _worldBounds.transform.position);
            if (distance > _worldBounds.radius && !isDead) OnDeath();
            
            if(position.y < _worldLimit && isDead) EffectiveDeath();
        }
        
        public void Attack(HitInfo hitInfo)
        {
            if (isDead || isInvincible) return;

            pipeline.Process(ref hitInfo);

            if (!hitInfo.success) return;

            Damage(hitInfo.hit.damage);

            if (!hitInfo.stunSuccess || !attack.PriorityCheck(hitInfo.priority)) return;

            Stun(hitInfo.hit.stunDuration);
            Knockback(hitInfo);
        }

        public void Damage(float damage)
        {
            if (isInvincible || isDead) return;

            var startHealth = _currentHealth;
            _currentHealth -= damage;

            if (_currentHealth <= 0)
            {
                OnDeath();
            }
            else
            {
                isInvincible = true;
                _invincibleTimer.Start();
            }

            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            var delta = new Delta
            {
                delta = startHealth - _currentHealth,
                newRatio = Mathf.Clamp01(_currentHealth / _maxHealth),
            };
            if (delta.delta != 0)
            {
                OnHealthChanged?.Invoke(delta);
                if (damageNumber != null) damageNumber.Init((int)delta.delta);
            }
        }

        public void Stun(float stunDuration)
        {
            if (stunDuration <= 0) return;

            isStunned = true;
            _stunTimer.Reset(stunDuration);
            _stunTimer.Start();

            OnStun?.Invoke();
        }

        public void Knockback(HitInfo hitInfo)
        {
            if (!hitInfo.success) return;

            var knockback = hitInfo.hit.knockback;
            var force = hitInfo.direction * knockback.x + Vector3.up * knockback.y;

            _invoker.Velocity.Execute(Vector3.zero);
            _invoker.Knockback.Execute(force);
        }

        public void OnDeath()
        {
            isDead = true;
            combo.ArtificialLock();
            animator.Death();
            OnDie?.Invoke();
        }

        public void EffectiveDeath()
        {
            _invoker.Kill.Get();
        }
    }
}