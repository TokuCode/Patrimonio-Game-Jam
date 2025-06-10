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
        
        public List<SingleHit> attacks;
    }

}