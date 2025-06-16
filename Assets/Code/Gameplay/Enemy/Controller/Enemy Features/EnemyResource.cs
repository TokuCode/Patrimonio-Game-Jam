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

        [Header("Health")] [SerializeField] private float _currentHealth;
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
            _dependencies.TryGetFeature(out attack);
            _dependencies.TryGetFeature(out combo);
            _dependencies.TryGetFeature(out attributes);
            _dependencies.TryGetFeature(out movement);
            _currentHealth = attributes.MaxHealth;
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

            playerShared.healthRatio = Mathf.Clamp01(_currentHealth / attributes.MaxHealth);
            isInvincible = false;
            isStunned = false;
            _invincibleTimer.Stop();
            _stunTimer.Stop();
        }

        public override void UpdateFeature()
        {
            _invincibleTimer.Tick(Time.deltaTime);
            _stunTimer.Tick(Time.deltaTime);
        }
            
        public void BasicAttack(float damage, float multiplier = 3)
        {
            Damage(damage);
            Stun(multiplier);
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
                isDead = true;
                OnDie?.Invoke();
                OnDeath();
            }
            else
            {
                isInvincible = true;
                _invincibleTimer.Start();
            }

            _currentHealth = Mathf.Clamp(_currentHealth, 0, attributes.MaxHealth);

            var delta = new Delta
            {
                delta = startHealth - _currentHealth,
                newRatio = Mathf.Clamp01(_currentHealth / attributes.MaxHealth),
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

            Vector3 attackPoint = hitInfo.position;
            Vector3 center = _invoker.Position.Get();
            var knockback = hitInfo.hit.knockback;
            var direction = (center - attackPoint).With(y: 0).normalized;
            var force = direction * knockback.x + Vector3.up * knockback.y;

            _invoker.Velocity.Execute(Vector3.zero);
            _invoker.Knockback.Execute(force);
        }

        public void OnDeath()
        {
            combo.ArtificialLock();
            movement.StopMovement();
            animator.Death();
        }

        public void EffectiveDeath()
        {
            _invoker.Kill.Get();
        }
    }
}