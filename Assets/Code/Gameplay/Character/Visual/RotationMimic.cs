using UnityEngine;

namespace Movement3D.Gameplay
{
    public class RotationMimic : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void Update()
        {
            transform.rotation = target.rotation;
        }
    }
}