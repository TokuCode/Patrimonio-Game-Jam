using Movement3D.Gameplay;
using UnityEngine;
using Movement3D.Helpers;

namespace Movement3D.UserInterface
{
    public class Crosshair : MonoBehaviour
    {
        private Camera _main;
        private PlayerController _player;
        private ThirdPersonCamera _cameraController;
        
        
        [Header("UI Elements")]
        [SerializeField] RectTransform crosshair;
        
        [Header("Target Snap")]
        [SerializeField] private float _maxTargetRange;
        [SerializeField] private float _targetCheckRadius;
        [SerializeField] private LayerMask _whatIsSolid;
        [SerializeField] private Vector2 _deadZone;
        [SerializeField, Range(0f, 1f)] private float _smoothing;
        private float interpolation => 1 - _smoothing / 2f;
    
        [Header("Runtime")]
        [SerializeField] private Vector3 _desiredCrosshairPosition;
    
        private void Start()
        {
            _main = Camera.main;
            _player = PlayerController.Singleton;
            _player.Dependencies.TryGetFeature(out _cameraController);
        }
    
        private void Update()
        {
            CrosshairBehaviour();
        }
    
        private void CrosshairBehaviour()
        {
            switch (_cameraController.CurrentCamera)
            {
                case ThirdPersonCamera.CameraStyle.Exploration or ThirdPersonCamera.CameraStyle.Strategy:
                    if(crosshair.gameObject.activeSelf) crosshair.gameObject.SetActive(false);
                    break;
                case ThirdPersonCamera.CameraStyle.Combat or ThirdPersonCamera.CameraStyle.Immersive:
                    if(!crosshair.gameObject.activeSelf) crosshair.gameObject.SetActive(true);
                    TargetSnap();
                    break;
            }
        }
    
        private void TargetSnap()
        {
            Vector3 targetPoint;
    
            Ray ray = _main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, _maxTargetRange, _whatIsSolid))
                targetPoint = hit.point;
            else
            {
                _desiredCrosshairPosition = Vector3.zero;
                return;
            }
    
            Vector3 center = _player.Invoker.CenterPosition.Get();
            float radius = _player.Invoker.Radius.Get();
            Vector3 gunTip = _player.Invoker.CombatLookAtPosition.Get();
            Vector3 gunTipNormalized = center + (gunTip - center).With(y: 0).normalized * radius;
            Vector3 targetPointFromGunTip;
            Vector3 direction = targetPoint - gunTipNormalized;
            if(Physics.SphereCast(gunTipNormalized, _targetCheckRadius, direction.normalized, out hit, _maxTargetRange, _whatIsSolid))
            {
                targetPointFromGunTip = hit.point;
            }
            else
            {
                _desiredCrosshairPosition = Vector3.zero;
                return;
            }
    
            Vector3 targetInViewportPoint = _main.WorldToViewportPoint(targetPointFromGunTip);
            targetInViewportPoint.x *= Screen.width;
            targetInViewportPoint.x -= Screen.width / 2f;
            targetInViewportPoint.x = Mathf.Clamp(targetInViewportPoint.x, -Screen.width * _deadZone.x / 2f, Screen.width * _deadZone.x / 2f);
            targetInViewportPoint.y *= Screen.height;
            targetInViewportPoint.y -= Screen.height / 2f;
            targetInViewportPoint.y = Mathf.Clamp(targetInViewportPoint.y, -Screen.height * _deadZone.y / 2f, Screen.height * _deadZone.y / 2f);
            targetInViewportPoint.z = 0f;
    
            _desiredCrosshairPosition = targetInViewportPoint;
            
            crosshair.anchoredPosition = Vector3.Lerp(crosshair.anchoredPosition, _desiredCrosshairPosition, interpolation);
        }
    }
}
