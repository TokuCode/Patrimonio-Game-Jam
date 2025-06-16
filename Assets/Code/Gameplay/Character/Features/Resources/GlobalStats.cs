using System;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    [DefaultExecutionOrder(-50)]
    public class GlobalStats : Singleton<GlobalStats>
    {
        [Header("Constitution Stats")]
        [SerializeField] private Stat _maxHealth;
        public float MaxHealth(int attr) => _maxHealth.GetValue(attr);
        [SerializeField] private Stat _stunResistance;
        public float StunResistance(int attr) => _stunResistance.GetValue(attr);
        [SerializeField] private Stat _stunDurationReduction;
        public float StunDurationReduction(int attr) => _stunDurationReduction.GetValue(attr);
        [SerializeField] private Stat _knockbackResistance;
        public float KnockbackResistance(int attr) => _knockbackResistance.GetValue(attr);
        [SerializeField] private Stat _damageReduction;
        public float DamageReduction(int attr) => _damageReduction.GetValue(attr);
        
        [Header("Agility Stats")]
        [SerializeField] private Stat _staminaCostReductionMultiplier;
        public float StaminaCostReduction(int attr) => _staminaCostReductionMultiplier.GetValue(attr);
        [SerializeField] private Stat _staminaRegenMultiplier;
        public float StaminaRegen(int attr) => _staminaRegenMultiplier.GetValue(attr);
        [SerializeField] private Stat _speedMultiplier;
        public float Speed(int attr) => _speedMultiplier.GetValue(attr);
        [SerializeField] private Stat _runSpeedMultiplier;
        public float RunSpeed(int attr) => _runSpeedMultiplier.GetValue(attr);
        [SerializeField] private Stat _accelerationMultiplier;
        public float Acceleration(int attr) => _accelerationMultiplier.GetValue(attr);
        [SerializeField] private Stat _runAccelerationMultiplier;
        public float RunAcceleration(int attr) => _runAccelerationMultiplier.GetValue(attr);
        [SerializeField] private Stat jumpHeight;
        public float JumpForce(int attr) => JumpHeight.ToForce(jumpHeight.GetValue(attr));

        [Header("Force Stats")] 
        [SerializeField] private Stat _attackPower;
        public float AttackPower(int attr) => _attackPower.GetValue(attr);
        [SerializeField] private Stat _stunPower;
        public float StunPower(int attr) => _stunPower.GetValue(attr);
        [SerializeField] private Stat _stunDurationBoost;
        public float StunDurationBoost(int attr) => _stunDurationBoost.GetValue(attr);
        [SerializeField] private Stat _knockbackPower;
        public float KnockbackPower(int attr) => _knockbackPower.GetValue(attr);
    }
   
    [Serializable]
    public struct Stat
    {
        public float startValue;
        public float endValue;
        public float negativeEndValue;
        public AnimationCurve progression;
        public AnimationCurve negativeProgression;

        public float GetValue(int attribute)
        {
            attribute = Mathf.Clamp(attribute, 0, 10);
            if (attribute == 5) return startValue;
            if (attribute < 5) return Mathf.Lerp(startValue, negativeEndValue, negativeProgression.Evaluate(1 - (float)attribute/5));
            return Mathf.Lerp(startValue, endValue, progression.Evaluate((float)attribute/5 - 1));
        }
    } 
}