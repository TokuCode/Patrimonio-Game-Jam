using UnityEngine;

namespace Movement3D.Gameplay
{
    [DefaultExecutionOrder(-50)]
    public class EnemyAnimator : MonoBehaviour
    {
        [SerializeField] private EnemyController _player;
        private Animator animator;
        private AnimatorOverrideController animatorOverride;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animatorOverride = new(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverride;
        }
        
        public void SetBlend(float blend)
        {
            if(blend == animator.GetFloat("Horizontal")) return;
            animator.SetFloat("Horizontal", blend);
        }

        public void SetVertical(float vertical)
        {
            if(vertical == animator.GetFloat("Vertical")) return;
            animator.SetFloat("Vertical", vertical);
        }

        public void SetCrouch(bool crouch)
        {
            animator.SetBool("Crouch", crouch);
        }

        public void Attack()
        {
            animator.SetTrigger("Attack");
        }

        public void Freeze(bool freeze)
        {
            animator.SetBool("Freeze", freeze);
        }

        public void Death()
        {
            animator.SetTrigger("Death");
        }

        public void AttackOverride(FullAttack attack)
        {
            animatorOverride["a_Idle_Battle"] = attack.animationClip;
        }
    }
}