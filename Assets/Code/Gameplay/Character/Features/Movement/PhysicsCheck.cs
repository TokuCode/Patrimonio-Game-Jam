using UnityEngine;

namespace Movement3D.Gameplay
{
    public class PhysicsCheck : Feature
    {
        private const float ExtraDistanceGround = .01f;
        private const float ExtraDistanceHead = .1f;
        private const float ExtraDistanceSlope = .15f;

        [Header("Ground, Slope and Ladder Check")] 
        [SerializeField] private LayerMask _whatIsGround;

        [SerializeField] private bool _onGround;
        public bool OnGround => _onGround;
        [SerializeField] private float _lastGroundTime;
        public float LastGroundTime => _lastGroundTime;
        private RaycastHit _groundHit;
        [SerializeField] private bool _onSlope;
        private RaycastHit slopeHit;
        public Vector3 SlopeNormal => _onSlope ? slopeHit.normal : Vector3.up;
        public bool OnSlope => _onSlope;
        private bool _previousOnSlope;
        public bool PreviousOnSlope => _previousOnSlope;
        [SerializeField] private float _maxSlopeAngle;
        [SerializeField] private bool _blockedHead;
        public bool BlockedHead => _blockedHead;

        public override void UpdateFeature()
        {
            GroundCheck();
            CheckBlockedHead();
            SlopeCheck();
        }

        private void GroundCheck()
        {
            var position = _invoker.CenterPosition.Get();
            var radius = _invoker.Radius.Get();
            var height = _invoker.Height.Get();
            var footSize = new Vector3(radius, ExtraDistanceGround, radius);
            
            _onGround = Physics.BoxCast(position, footSize, Vector3.down, out _groundHit, Quaternion.identity, .1f + height/2, _whatIsGround);

            if (_onGround) _lastGroundTime = Time.time;
        }

        private void CheckBlockedHead()
        {
            var position = _invoker.CenterPosition.Get();
            var radius = _invoker.Radius.Get();
            var height = _invoker.Height.Get();
            var footSize = new Vector3(radius, ExtraDistanceHead, radius);
            float extensionLength = height * 1.5f;

            _blockedHead = Physics.BoxCast(position, footSize, Vector3.up, Quaternion.identity,
                extensionLength, _whatIsGround);
        }

        private void SlopeCheck()
        {
            _previousOnSlope = _onSlope;
            
            var position = _invoker.CenterPosition.Get();
            var height = _invoker.Height.Get();
            
            if (Physics.Raycast(position, Vector3.down, out slopeHit, ExtraDistanceSlope + height/2, _whatIsGround))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                _onSlope = angle != 0 && angle <= _maxSlopeAngle;
            }
            else _onSlope = false;
        }

        public Vector3 ProjectOnSlope(Vector3 inputVector)
        {
            if (!_onSlope) return inputVector;

            return Vector3.ProjectOnPlane(inputVector, slopeHit.normal);
        }
    }
}