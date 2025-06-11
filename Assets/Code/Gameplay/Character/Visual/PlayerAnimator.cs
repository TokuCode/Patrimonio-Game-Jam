using UnityEngine;

namespace Movement3D.Gameplay
{
    public class PlayerAnimator : MonoBehaviour
    {
        public enum EyePosition { normal, happy, angry, dead}
    
        [SerializeField] private PlayerController _player;
        private Animator animator;
        private AnimatorOverrideController animatorOverride;
        Renderer[] characterMaterials; 

        private void Awake()
        {
            animator = GetComponent<Animator>();
            characterMaterials = GetComponentsInChildren<Renderer>();
            animatorOverride = new(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverride;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha5)) EmoteHappy();
            else if(Input.GetKeyDown(KeyCode.Alpha6)) EmoteCombat();
            else if(Input.GetKeyDown(KeyCode.Alpha7)) EmoteScary();
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

        public void EmoteHappy() => animator.SetTrigger("Happy");
        public void EmoteScary() => animator.SetTrigger("Scary");
        public void EmoteCombat() => animator.SetTrigger("Combat");
        
        void ChangeEyeOffset(EyePosition pos)
        {
            Vector2 offset = Vector2.zero;

            switch (pos)
            {
                case EyePosition.normal:
                    offset = new Vector2(0, 0);
                    break;
                case EyePosition.happy:
                    offset = new Vector2(.33f, 0);
                    break;
                case EyePosition.angry:
                    offset = new Vector2(.66f, 0);
                    break;
                case EyePosition.dead:
                    offset = new Vector2(.33f, .66f);
                    break;
            }

            for (int i = 0; i < characterMaterials.Length; i++)
            {
                if (characterMaterials[i].transform.CompareTag("PlayerEyes"))
                    characterMaterials[i].material.SetTextureOffset("_MainTex", offset);
            }
        }

        public void AttackOverride(FullAttack attack)
        {
            animatorOverride["a_Idle_Battle"] = attack.animationClip;
        }
    }
}