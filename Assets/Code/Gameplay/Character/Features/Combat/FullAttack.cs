using System.Collections.Generic;
using UnityEngine;

namespace Movement3D.Gameplay
{
    [CreateAssetMenu(fileName = "New Attack", menuName = "Combat System/Attack")]
    public class FullAttack : ScriptableObject
    {
        public string attackName;
        public float attackRange;
        public AnimationClip animationClip;
        public int priority; //More priority, less possibility to be canceled and to cancel incoming attacks
        
        public List<SingleHit> attacks;
        
        [Header("Follow Up Effect")]
        public float followUp;
        [Header("Suck To Target Effect")]
        public bool suckToTarget;
        [Header("Charged Attack Effect")]
        public bool chargeAttack;
        public float chargeMultiplier;
        public float chargeTime;
        [Header("Dash Attack Effect")]
        public bool dashAttack;
        public int dashTick;
        public float dashImpulse;
        [Header("Down Attack Effect")]
        public bool downAttack;
        public float downImpulse;
        public float downTime;
        public float downMultiplier;
        [Header("Air Suspension Effect")]
        public bool airSuspension;
        [Header("Defensive Posture Effect")]
        public bool defensePosture;
        public float defensePostureMaxTime;
        [Header("Shoot Attack Effect")] 
        public bool shootAttack;
        public GameObject projectile;
        public bool chainEffects;
        public int chainTick;
    }

}