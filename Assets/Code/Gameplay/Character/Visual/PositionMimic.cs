using UnityEngine;

namespace Movement3D.Gameplay
{
    public class PositionMimic : MonoBehaviour
    {
        [SerializeField] private Transform target;
        private Vector3 offset;
        private void Awake()
        {
            offset = Quaternion.Inverse(target.rotation) * (transform.position - target.position);
        }

        private void Update()
        {
            SetPositionMimic();
        }

        private void SetPositionMimic()
        {
            transform.position = target.position + target.rotation * offset;
        }
    }
}
