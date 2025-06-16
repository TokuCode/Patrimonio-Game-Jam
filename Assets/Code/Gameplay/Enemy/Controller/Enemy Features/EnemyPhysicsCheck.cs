using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyPhysicsCheck : EnemyFeature
    {
        private const float ExtraDistanceGround = .01f;

        [SerializeField] private LayerMask _whatIsGround;
        [SerializeField] private bool _onGround;
        public bool OnGround => _onGround; 
        
        public override void ResetFeature(ref SharedProperties shared)
        {
            _onGround = false;
        }

        public override void UpdateFeature()
        {
            GroundCheck();
        }

        private void GroundCheck()
        {
            var position = _invoker.Center.Get();
            var radius = _invoker.Radius.Get();
            var height = _invoker.Height.Get();
            var footSize = new Vector3(radius, ExtraDistanceGround, radius);
            
            _onGround = Physics.BoxCast(position, footSize, Vector3.down, out var hit, Quaternion.identity, .1f + height/2, _whatIsGround);
        } 
    }
}