using UnityEngine;

namespace Movement3D.Gameplay
{
    public class LogicalAttackClock : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        private Attack _attack;

        private void Start()
        {
            player.Dependencies.TryGetFeature(out _attack);
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
