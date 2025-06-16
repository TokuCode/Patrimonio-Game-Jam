using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyLogicalAttackClock : MonoBehaviour
    {
        [SerializeField] EnemyController enemy;
        private EnemyAttack _attack;

        private void Start()
        {
            enemy.Dependencies.TryGetFeature(out _attack);
        }

        public void Tick()
        {
            _attack.Tick();
        }

        public void End()
        {
            _attack.EndAttack();
        }
    }
}