using UnityEngine;

namespace Movement3D.Gameplay
{
    public interface IControls
    {
        Vector2 MoveDirection { get; }
        bool Jump { get; }
        bool Run { get; }
        bool Crouch { get; }
        int Switch { get; }
    }
    
    public struct InputPayload
    {
        public Vector2 MoveDirection;
        public bool Jump;
        public bool Run;
        public bool Crouch;
        public string Signal;
        public int Switch;
        
        public UpdateContext Context;
    }
    
    public enum UpdateContext
    {
        Update,
        FixedUpdate
    }
}