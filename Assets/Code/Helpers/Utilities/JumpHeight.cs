using UnityEngine;

namespace Movement3D.Helpers
{
    public static class JumpHeight
    {
        public static float ToForce(float height)
        {
            float gravity = -Physics.gravity.y;
            return Mathf.Sqrt(height * gravity * 2);
        } 
    }
}