using UnityEngine;

namespace Movement3D.Gameplay
{
    public abstract class Controller : MonoBehaviour
    {
        public IDependencyManager Dependencies { get; } = new DependencyManager();

        public abstract void Deactivate(out SharedProperties shared);

        public abstract void Reactivate(SharedProperties shared);
    }
}