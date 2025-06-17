using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Switch : PlayerFeature
    {
        private PhysicsCheck physics;
        private Attack attack;
        private Resource resource;
        
        [SerializeField] private float _switchCooldown;
        private CountdownTimer _cooldownTimer;
        private bool _cooldown;
        private bool _isDead;

        public override void ResetFeature(ref SharedProperties shared)
        {
            _cooldownTimer.Stop();
            _cooldown = false;
            _isDead = false;
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out physics);
            _dependencies.TryGetFeature(out attack);
            _dependencies.TryGetFeature(out resource);
            _cooldownTimer = new CountdownTimer(_switchCooldown);
            _cooldownTimer.OnTimerStop += () => _cooldown = false;
            resource.OnDie += () => _isDead = true;
        }

        public override void UpdateFeature()
        {
            _cooldownTimer.Tick(Time.deltaTime);
        }

        public override void Apply(ref InputPayload @event)
        {
            int switchDir = @event.Switch;
            if(switchDir == 0) return;

            if (_cooldown) return;

            if (!physics.OnGround || attack.IsAttacking || resource.isStunned || _isDead) return;
            
            MulticharacterController.Instance.Switch(switchDir);
            _cooldownTimer.Start();
        }
    }
}