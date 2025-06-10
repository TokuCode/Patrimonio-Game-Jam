using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Movement3D.Helpers;

namespace Movement3D.Gameplay
{
    public class InputReader : Singleton<InputReader>, PlayerControls.IGameplayActions, IControls
    {
        PlayerControls _playerControls;

        public event Action JumpPressed; 
        
        public Vector2 MouseDelta { get; private set; }
        public Vector2 MoveDirection { get; private set; }
        public bool Jump { get; private set; }
        public bool Run { get; private set; }
        public bool Crouch { get; private set; }

        private void OnEnable()
        {
            _playerControls = new PlayerControls();
            _playerControls.Enable();
            _playerControls.Gameplay.SetCallbacks(this);
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

        public void OnAttackLight(InputAction.CallbackContext context)
        {
            if(context.performed) InputBuffer.Instance.Input("light");
        }

        public void OnAttackHeavy(InputAction.CallbackContext context)
        {
            if(context.performed) InputBuffer.Instance.Input("heavy");
        }
    }
}