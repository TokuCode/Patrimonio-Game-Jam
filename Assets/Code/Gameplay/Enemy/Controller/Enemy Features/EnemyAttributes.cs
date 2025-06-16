using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyAttributes : EnemyFeature, IProcess<HitInfo>
    {
        private EnemyResource resource;
        
        [Header("Attributes")] 
        [SerializeField, Range(0, 10)] private int _constitution;
        public float Constitution => _constitution;
        [SerializeField, Range(0, 10)] private int _agility;
        public int Agility => _agility;
        [SerializeField, Range(0, 10)] private int _force;
        public float Force => _force;
        
        public float MaxHealth => GlobalStats.Instance.MaxHealth(_constitution);
        public float StunResistance => GlobalStats.Instance.StunResistance(_constitution);
        public float StunDurationReduction => GlobalStats.Instance.StunDurationReduction(_constitution);
        public float KnockbackResistance => GlobalStats.Instance.KnockbackResistance(_constitution);
        public float DamageReduction => GlobalStats.Instance.DamageReduction(_constitution);
        public float StaminaCostReduction => GlobalStats.Instance.StaminaCostReduction(_agility);
        public float StaminaRegen => GlobalStats.Instance.StaminaRegen(_agility);
        public float Speed => GlobalStats.Instance.Speed(_agility);
        public float RunSpeed => GlobalStats.Instance.RunSpeed(_agility);
        public float Acceleration => GlobalStats.Instance.Acceleration(_agility);
        public float RunAcceleration => GlobalStats.Instance.Acceleration(_agility);
        public float JumpForce => GlobalStats.Instance.JumpForce(_agility);
        public float AttackPower => GlobalStats.Instance.AttackPower(_force);
        public float KnockbackPower => GlobalStats.Instance.KnockbackPower(_force);
        public float StunPower => GlobalStats.Instance.StunPower(_force);
        public float StunDurationBoost => GlobalStats.Instance.StunDurationBoost(_force);

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