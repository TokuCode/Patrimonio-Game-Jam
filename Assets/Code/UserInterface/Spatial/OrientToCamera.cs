using UnityEngine;

namespace Movement3D.UserInterface
{
    public class OrientToCamera : MonoBehaviour
    {
        private Camera _main;
        private Canvas _spatialCanvas;

        private void Start()
        {
            _spatialCanvas = GetComponent<Canvas>();
            _main = _spatialCanvas.worldCamera;
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + _main.transform.rotation * Vector3.forward, _main.transform.rotation * Vector3.up);
        }
    }
}