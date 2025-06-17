using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Attributes : PlayerFeature, IProcess<HitInfo>
    {
        private Resource resource;
        
        [Header("Attributes")] 
        [SerializeField, Range(0, 10)] private int _constitution;
        public float Constitution => _constitution;
        [SerializeField, Range(0, 10)] private int _agility;
        public int Agility => _agility;
        [SerializeField, Range(0, 10)] private int _force;
        public float Force => _force;

        [Header("Overwrites")] 
        [SerializeField] private float _stunResistance;
        [SerializeField] private float _stunDurationReduction;
        [SerializeField] private float _knockbackResistance;
        [SerializeField] private float _damageReduction;
        [SerializeField] private float _staminaCostReduction;
        [SerializeField] private float _staminaRegenMultiplier;
        
        public float StunResistance => _stunResistance != 0 ? _stunResistance : GlobalStats.Instance.StunResistance(_constitution);
        public float StunDurationReduction => _stunDurationReduction != 0 ? _stunDurationReduction : GlobalStats.Instance.StunDurationReduction(_constitution);
        public float KnockbackResistance => _knockbackResistance != 0 ? _knockbackResistance : GlobalStats.Instance.KnockbackResistance(_constitution);
        public float DamageReduction => _damageReduction != 0 ? _damageReduction : GlobalStats.Instance.DamageReduction(_constitution);
        public float StaminaCostReduction => _staminaCostReduction != 0 ? _staminaCostReduction : GlobalStats.Instance.StaminaCostReduction(_agility);
        public float StaminaRegen => _staminaRegenMultiplier != 0 ? _staminaRegenMultiplier : GlobalStats.Instance.StaminaRegen(_agility);

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out resource);
            resource.pipeline.Register(this);
        }

        public void Apply(ref HitInfo hitInfo)
        {
            if (StunResistance > hitInfo.hit.stunPower) hitInfo.stunSuccess = false;
            hitInfo.hit.damage *= DamageReduction;
            hitInfo.hit.stunDuration *= StunDurationReduction;
            hitInfo.hit.knockback *= KnockbackResistance;
        }
    }

}