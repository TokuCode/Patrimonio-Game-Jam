using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Controller : MonoBehaviour
    {
        protected List<Feature> _features = new();
        public IDependencyManager Dependencies { get; } = new DependencyManager();
        protected virtual void Awake()
        {
            _features = GetComponents<Feature>().ToList();

            foreach (var feature in _features)
            {
                Dependencies.TryAddFeature(feature);
            }
            
            foreach (var feature in _features)
            {
                feature.InitializeFeature(this);
            }
        }
        
        protected virtual void Update()
        {
            foreach (var feature in _features)
            {
                feature.UpdateFeature();
            }
        }

        protected virtual void FixedUpdate()
        {
            foreach (var feature in _features)
            {
                feature.FixedUpdateFeature();
            }
        }

        public virtual void Deactivate(out SharedProperties shared)
        {
            shared = new SharedProperties();
            foreach (var feature in _features)
            {
                feature.ResetFeature(ref shared);
            }
            
            gameObject.SetActive(false);
        }

        public virtual void Reactivate(SharedProperties shared)
        {
            gameObject.SetActive(true);
            
            foreach (var feature in _features)
            {
                feature.ReInitializeFeature(this, shared);
            }
        }

        public bool Get<T>(out T feature) where T : IFeature
        {
            return Dependencies.TryGetFeature(out feature);
        }
    }
}