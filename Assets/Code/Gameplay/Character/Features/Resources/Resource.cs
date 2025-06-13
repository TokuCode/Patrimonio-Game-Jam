using System;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Movement3D.Gameplay
{
    public class Resource : Feature
    {
        private Run run;
        private Attack attack;
        private FloatingText damageNumber;
        
        [Header("Health")] 
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _currentHealth;
        public float CurrentHealth => _currentHealth;
        public bool isDead { get; private set; }
        public bool isInvincible { get; private set; }
        public CountdownTimer _invincibleTimer;
        [SerializeField] public float _invincibilityTime;
        public Pipeline<HitInfo> pipeline { get; private set; } = new();
        [SerializeField] private float baseStunTime;
        public bool isStunned { get; private set; }
        private CountdownTimer _stunTimer;
        
        public event Action<Delta> OnHealthChanged;
        public event Action OnStun;
        public event Action OnDie;
        
        [Header("Stamina")]
        [SerializeField] private float _baseStamina;
        [SerializeField] private float _minStaminaDelta;
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
            _currentHealth = _maxHealth;
            _currentStamina = _baseStamina;
            _staminaTimer = new CountdownTimer(_staminaCooldown);
            _staminaTimer.OnTimerStart = () => { _regen = false; };
            _staminaTimer.OnTimerStop = () => { _regen = true; };
            _invincibleTimer = new CountdownTimer(_invincibilityTime);
            _invincibleTimer.OnTimerStop = () => { isInvincible = false; };
            _stunTimer = new CountdownTimer(baseStunTime);
            _stunTimer.OnTimerStop = () => { isStunned = false; };
            if (controller is PlayerController { IsPlayer: false } player)
            {
                damageNumber = player.DamageNumber;
            }
        }

        public override void UpdateFeature()
        {
            _invincibleTimer.Tick(Time.deltaTime);
            _stunTimer.Tick(Time.deltaTime);
            _staminaTimer.Tick(Time.deltaTime);
            StaminaManagement();
            
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha9)) BasicAttack(20);
#endif
        }

        public void StaminaManagement()
        {
            if (run.IsRunning)
            {
                _staminaDeltaAccumulator -= _staminaDepletionRate * Time.deltaTime;
            } else if (_regen && _currentStamina < _baseStamina)
            {
                _staminaDeltaAccumulator += _staminaRegenRate * Time.deltaTime;
            }
            if (_currentStamina >= _baseStamina) isDepleted = false;
            StaminaDeltaAcumulator();
        }

        private void StaminaDeltaAcumulator()
        { 
            if (Mathf.Abs(_staminaDeltaAccumulator) > _minStaminaDelta)
            {
                float delta = _minStaminaDelta * Mathf.Sign(_staminaDeltaAccumulator);
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
            DeltaStamina(-_attackCost);
        }

        public void OnJumpStamina()
        {
            _staminaTimer.Start();
            DeltaStamina(-_jumpCost);
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
            if (attack.PriorityCheck(hitInfo.priority))
            {
                Stun(hitInfo.hit.stunTime / baseStunTime); 
                Knockback(hitInfo);
            }
        }

        public void Damage(float damage)
        {
            if(isInvincible) return;
            
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
            
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            var delta = new Delta
            {
                delta = startHealth - _currentHealth,
                newRatio = Mathf.Clamp01(_currentHealth / _maxHealth),
            };
            if (delta.delta != 0)
            {
                OnHealthChanged?.Invoke(delta);
                if(damageNumber != null) damageNumber.Init((int)delta.delta);
            }
        }

        public void Stun(float multiplier = 1)
        {
            if (multiplier <= 0) return;
            
            isStunned = true;
            _stunTimer.Reset(baseStunTime * multiplier);
            _stunTimer.Start();
            
            OnStun?.Invoke();
        }

        public void Knockback(HitInfo hitInfo)
        {
            if(!hitInfo.success) return;
            
            Vector3 attackPoint = hitInfo.position;
            Vector3 center = _invoker.CenterPosition.Get();
            var knockback = hitInfo.hit.knockback;
            var direction = (center - attackPoint).With(y: 0).normalized;
            var force = direction * knockback.x + Vector3.up * knockback.y;
            
            _invoker.Velocity.Execute(Vector3.zero);
            _invoker.AddForce.Execute(new(force, ForceMode.VelocityChange));
        }

        public void OnDeath()
        {
            gameObject.SetActive(false);
        }
    }

    public struct Delta
    {
        public float delta;
        public float newRatio;
    }
}