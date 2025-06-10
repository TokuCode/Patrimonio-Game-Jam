using UnityEngine;

namespace Movement3D.Gameplay
{
    public class FirstPersonCamera : Feature
    {
        private Camera _main;
        private Movement movement;
        
        [Header("Camera Rig")]
        [SerializeField] private float _distanceToPlayer;

        [Header("Camera Coordinates")]
        [SerializeField] private float _azimutal;
        [SerializeField] private float _polar;
        [Space, SerializeField] private float _minPolar;
        [SerializeField] private float _maxPolar;

        [Header("Camera Movement")]
        [SerializeField] private Vector2 _sensibility;
        [SerializeField, Range(0f, 1f)] private float _smoothing;
        [SerializeField, Range(0f, 1f)] private float _positionSmoothing;
        [SerializeField] private bool _invertPolar;
        private float Interpolation => .5f + (1 - _smoothing) / 2f;
        private float InterpolationPosition => .5f + (1 - _positionSmoothing) / 2f;

        [Header("Cursor")]
        [SerializeField] private bool _cursorVisible;
        [SerializeField] private CursorLockMode _cursorLockMode;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out movement);
            _main = Camera.main;
        }

        public override void UpdateFeature()
        {
            if(!enabled) return;
            
            SetCursor();
        }

        public override void Apply(ref InputPayload @event)
        {
            if(!enabled) return;
            
            if (@event.Context != UpdateContext.Update) return;
            
            MoveCamera(@event.MouseDelta);
            PlaceCamera();
        }

        private void PlaceCamera()
        {
            Quaternion actualRotation = _invoker.LookAtRotation.Get();
            Quaternion targetRotation = Quaternion.Euler(_polar, _azimutal, 0f);
            Quaternion finalRotation = Quaternion.Slerp(actualRotation, targetRotation, Interpolation);
            
            _invoker.LookAtRotation.Execute(finalRotation);
            _invoker.OrientationRotation.Execute(finalRotation.eulerAngles.y);
            _invoker.PlayerRotation.Execute(finalRotation.eulerAngles.y);
        }
        
        private void PlaceCameraInstantly()
        {
            Quaternion targetRotation = Quaternion.Euler(_polar, _azimutal, 0f);
            
            _invoker.LookAtRotation.Execute(targetRotation);
            _invoker.OrientationRotation.Execute(targetRotation.eulerAngles.y);
            _invoker.PlayerRotation.Execute(targetRotation.eulerAngles.y);
        }
        
        private void MoveCamera(Vector2 mouseDelta)
        {
            if (mouseDelta == Vector2.zero || movement.IsMovementBlocked) return;

            _azimutal = (_azimutal + mouseDelta.x * _sensibility.x) % 360;
            _polar = Mathf.Clamp(_polar - (_invertPolar ? -1 : 1) * mouseDelta.y * _sensibility.y, _minPolar, _maxPolar);
        }

        private void SetCursor()
        {
            Cursor.visible = _cursorVisible;
            Cursor.lockState = _cursorLockMode;
        }    
        
        public void UpdateCoordinates()
        {
            Quaternion playerRotation = _main.transform.rotation;
            _azimutal = playerRotation.eulerAngles.y;
            _polar = playerRotation.eulerAngles.x;
            if(_polar > 180) _polar -= 360;
            
            PlaceCameraInstantly();
        } 
    }
}