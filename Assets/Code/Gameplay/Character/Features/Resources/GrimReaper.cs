using UnityEngine;

namespace Movement3D.Gameplay
{
    public class GrimReaper : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        private Resource _resource;

        private void Start()
        {
            player.Dependencies.TryGetFeature(out _resource);
        }

        public void Die()
        {
            _resource.EffectiveDeath();
        }
    }
}