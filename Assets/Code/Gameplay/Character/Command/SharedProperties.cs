using UnityEngine;

namespace Movement3D.Gameplay
{
    public class SharedProperties
    {
        public Vector3 position;
        public Vector3 direction;
    }
    
    public class PlayerSharedProperties : SharedProperties
    {
        public float healthRatio;
        public float staminaCount;
        public bool isDepleted;
    }

    public class EnemySharedProperties : SharedProperties { }
}