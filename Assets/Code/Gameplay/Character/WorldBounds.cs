using UnityEngine;

namespace Movement3D.Gameplay
{
    [RequireComponent(typeof(SphereCollider))]
    public class WorldBounds : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            var controller = other.gameObject.GetComponent<EnemyController>();
            
            if(controller != null) controller.Deactivate(out var shared);
        }
    }
}