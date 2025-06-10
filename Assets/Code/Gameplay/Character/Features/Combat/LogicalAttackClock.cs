using UnityEngine;

namespace Movement3D.Gameplay
{
    public class LogicalAttackClock : MonoBehaviour
    {
        private Attack _attack;

        private void Start()
        {
            PlayerController.Singleton.Dependencies.TryGetFeature(out _attack);
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
