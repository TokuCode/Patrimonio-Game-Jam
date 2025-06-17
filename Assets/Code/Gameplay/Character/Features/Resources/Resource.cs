using System;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Resource : PlayerFeature
    {
        private Run run;
        private Attack attack;
        private PlayerAnimator animator;
        private Movement movement;
        private ComboReader combo;
        private Attributes attributes;

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
        
        [Header("Stamina")]
        [SerializeField] private float _baseStamina;
        public float BaseStamina => _baseStamina;
        [SerializeField] private float _accumulatorSize;
        [SerializeField] private float _runCost;
        [SerializeField] private float _attackCost;
        [SerializeField] private float _jumpCost;
        [SerializeField] private float _staminaDeltaAccumulator;
        [SerializeField] private float _currentStamina;
        public float CurrentStamina => _currentStamina;
        public bool isDepleted { get; private set; }
        private bool _regen;
        public bool AbleToRun => !isDepleted;
        public bool AbleToAttack => !isDepleted;
        public bool AbleToJump => !isDepleted;
        [SerializeField] private float _staminaRegenRate;
        [SerializeField] private float _staminaRegenRateDepleted;
        [SerializeField] private float _staminaDepletionRate;
        [SerializeField] private float _staminaCooldown;
        private CountdownTimer _staminaTimer;
        
        public event Action<Delta> OnStaminaChanged;
        public event Action OnDepleted;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _regen = true;
            _dependencies.TryGetFeature(out run);
            _dependencies.TryGetFeature(out attack);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out combo);
            _dependencies.TryGetFeature(out attributes);
            _currentHealth = _maxHealth;
            _currentStamina = _baseStamina;
            _staminaTimer = new CountdownTimer(_staminaCooldown);
            _staminaTimer.OnTimerStart = () => { _regen = false; };
            _staminaTimer.OnTimerStop = () => { _regen = true; };
            _invincibleTimer = new CountdownTimer(_invincibilityTime);
            _invincibleTimer.OnTimerStop = () => { isInvincible = false; };
            _stunTimer = new CountdownTimer(1f);
            _stunTimer.OnTimerStop = () =>
            {
                isStunned = false;
            };
            if (controller is PlayerController playerController)
            {
                animator = playerController.Animator;
            }
        }

        public override void ResetFeature(ref SharedProperties shared)
        {
            if (shared is not PlayerSharedProperties playerShared) return;
            
            playerShared.healthRatio = Mathf.Clamp01(_currentHealth/_maxHealth);
            playerShared.staminaCount = _currentStamina;
            playerShared.isDepleted = isDepleted;
            isInvincible = false;
            isDepleted = false;
            isStunned = false;
            _invincibleTimer.Stop();
            _staminaTimer.Stop();
            _stunTimer.Stop();
            _currentStamina = _baseStamina;
            _regen = true;
        }

        public override void ReInitializeFeature(Controller controller, SharedProperties shared)
        {
            if(shared is not PlayerSharedProperties playerShared) return;
            
            _currentHealth = _maxHealth * playerShared.healthRatio;
            _currentStamina = Mathf.Clamp(playerShared.staminaCount, 0, _baseStamina);
            isDepleted = playerShared.isDepleted;
        }

        public override void UpdateFeature()
        {
            _invincibleTimer.Tick(Time.deltaTime);
            _stunTimer.Tick(Time.deltaTime);
            if(!attack.IsAttacking) _staminaTimer.Tick(Time.deltaTime);
            StaminaManagement();
            
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha9)) BasicAttack(20);
#endif
        }

        public void StaminaManagement()
        {
            if (run.IsRunning)
            {
                _staminaDeltaAccumulator -= _staminaDepletionRate * attributes.StaminaCostReduction * Time.deltaTime;
            } else if (_regen && _currentStamina < _baseStamina && !attack.IsAttacking)
            {
                _staminaDeltaAccumulator += (isDepleted ? _staminaRegenRateDepleted : _staminaRegenRate) * attributes.StaminaRegen * Time.deltaTime;
            }
            if (_currentStamina >= _baseStamina) isDepleted = false;
            StaminaDeltaAcumulator();
        }

        private void StaminaDeltaAcumulator()
        { 
            if (Mathf.Abs(_staminaDeltaAccumulator) > _accumulatorSize)
            {
                float delta = _accumulatorSize * Mathf.Sign(_staminaDeltaAccumulator);
                _staminaDeltaAccumulator -= delta;
                DeltaStamina(delta);
            }
        }

        public void DeltaStamina(float stamina)
        {
            var startStamina = _currentStamina;
            _currentStamina += stamina;
            _currentStamina = Mathf.Clamp(_currentStamina, 0, _baseStamina);

            if (_currentStamina <= 0)
            {
                isDepleted = true;
                _staminaTimer.Start();
                OnDepleted?.Invoke();
            }
            
            var delta = new Delta
            {
                delta = _currentStamina - startStamina,
                newRatio = Mathf.Clamp01(_currentStamina / _baseStamina),
            };
            if(delta.delta != 0) OnStaminaChanged?.Invoke(delta);
        }

        public void OnAttackStamina()
        {
            _staminaTimer.Start();
            _regen = false;
            DeltaStamina(-_attackCost * attributes.StaminaCostReduction);
        }

        public void OnJumpStamina()
        {
            _staminaTimer.Start();
            _regen = false;
            DeltaStamina(-_jumpCost * attributes.StaminaCostReduction);
        }

        public void BasicAttack(float damage, float multiplier = 3)
        {
            Damage(damage);
            Stun(multiplier);
        }

        public void Attack(HitInfo hitInfo)
        {
            if(isDead || isInvincible) return;
            
            pipeline.Process(ref hitInfo);

            if (!hitInfo.success) return;
            
            Damage(hitInfo.hit.damage);

            if (!hitInfo.stunSuccess || !attack.PriorityCheck(hitInfo.priority)) return;
            
            Stun(hitInfo.hit.stunDuration); 
            Knockback(hitInfo);
        }

        public void Damage(float damage)
        {
            if(isInvincible || isDead) return;
            
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
                newRatio = Mathf.Clamp01(_currentHealth / _maxHealth)
            };
            if (delta.delta != 0)
            {
                OnHealthChanged?.Invoke(delta);
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
            if(!hitInfo.success) return;
            
            var knockback = hitInfo.hit.knockback;
            var force = hitInfo.direction * knockback.x + Vector3.up * knockback.y;
            
            _invoker.Velocity.Execute(Vector3.zero);
            _invoker.AddForce.Execute(new(force, ForceMode.VelocityChange));
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

    public struct Delta
    {
        public float delta;
        public float newRatio;
    }
}