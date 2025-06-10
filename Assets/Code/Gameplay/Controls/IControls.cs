using UnityEngine;

namespace Movement3D.Gameplay
{
    public interface IControls
    {
        Vector2 MouseDelta { get; }
        Vector2 MoveDirection { get; }
        bool Jump { get; }
        bool Run { get; }
        bool Crouch { get; }
    }
    
    public struct InputPayload
    {
        public Vector2 MouseDelta;
        public Vector2 MoveDirection;
        public bool Jump;
        public bool Run;
        public bool Crouch;
        public string Signal;
        public UpdateContext Context;
    }
    
    public enum UpdateContext
    {
        Update,
        FixedUpdate
    }
}