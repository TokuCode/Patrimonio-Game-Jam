using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Movement3D.Helpers;

namespace Movement3D.Gameplay
{
    public class InputReader : Singleton<InputReader>, PlayerControls.IGameplayActions, IControls
    {
        PlayerControls _playerControls;
        PhysicsCheck _physics;
        PlayerController _controller;

        public event Action JumpPressed; 
        
        public Vector2 MouseDelta { get; private set; }
        public Vector2 MoveDirection { get; private set; }
        public bool Jump { get; private set; }
        public bool Run { get; private set; }
        public bool Crouch { get; private set; }
        
        public bool Charge { get; private set; }
        private string _1sttag;

        private void OnEnable()
        {
            _playerControls = new PlayerControls();
            _playerControls.Enable();
            _playerControls.Gameplay.SetCallbacks(this);
        }

        public void CacheController(PlayerController playerController)
        {
            _controller = playerController;
            playerController.Dependencies.TryGetFeature(out _physics);
        }

        public void OnLookAround(InputAction.CallbackContext context)
        {
            MouseDelta = context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveDirection = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Jump = true;
                JumpPressed?.Invoke();
            }
            else if(context.canceled) Jump = false;
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (context.performed) Run = true;
            else if(context.canceled) Run = false;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed) Crouch = true;
            else if(context.canceled) Crouch = false;
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                CacheAttackTag("attack");
                ReleaseCachedTag();
                InputBuffer.Instance.Input("neutral", 1);
                Set3rdLayerTag();
            }
        }
        
        public void OnSpecial(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                CacheAttackTag("special");
                ReleaseCachedTag();
                InputBuffer.Instance.Input("neutral", 1);
                Set3rdLayerTag();
            }
        }

        public void OnHold(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Charge = true;
                ReleaseCachedTag();
                InputBuffer.Instance.Input("charge", 1);
                Set3rdLayerTag();
            }

            else if (context.canceled)
            {
                Charge = false;
                if (Charge)
                {
                    ReleaseCachedTag();
                    InputBuffer.Instance.Input("release", 1);
                    Set3rdLayerTag();
                }
            }
        }

        private void Set3rdLayerTag()
        {
            var vertical = _controller.Invoker.Velocity.Get().y;
            var onGround = _physics.OnGround;

            string tag = "air";
            if (Jump) tag = "jump";
            else if (onGround) tag = "ground";
            else if (vertical <= 0) tag = "fall";
            
            InputBuffer.Instance.Input(tag, 2);
        }

        private void CacheAttackTag(string tag)
        {
            _1sttag = tag;
        }

        private void ReleaseCachedTag()
        {
            InputBuffer.Instance.Input(_1sttag, 0);
            _1sttag = String.Empty;
        }
    }
}