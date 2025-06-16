using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyGrimReaper : MonoBehaviour
    {
        [SerializeField] EnemyController enemy;
        private EnemyResource _resource;

        private void Start()
        {
            enemy.Dependencies.TryGetFeature(out _resource);
        }

        public void Die()
        {
            _resource.EffectiveDeath();
        }
    }
}