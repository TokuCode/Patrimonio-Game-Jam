using System;

namespace Movement3D.Gameplay
{
    [Serializable]
    public struct EnemyData
    {
        public SerializableGuid Id;
        public float staminaBase;
        public float maxOutOfSightTime;
        public float manaAmountForSpecial;
        public float maxComboCount;
        
        public float attackStaminaCost;
        public float chaseStaminaCostPerSecond;
        public float sneakStaminaCostPerSecond;
        public float workStaminaCostPerSecond;
        public float energyRestRegenPerSecond;
        public float healthRegenPerSecond;
        
        public float perception;
        public float curiosity;
        public float devotion;
        public float constitution;
        public float ferociousness;

        public FullAttack attack;
        public FullAttack special;
    }
}