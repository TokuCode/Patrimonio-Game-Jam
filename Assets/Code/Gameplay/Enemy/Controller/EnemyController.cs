using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Movement3D.Gameplay
{
    public class EnemyController : Controller
    {
        [SerializeField] private string _Name;
        public string Name => _Name;
        [SerializeField] private Canvas _spaitalUI;
        [SerializeField] private FloatingText _damageNumber;
        [SerializeField] private EnemyAnimator _animator;
        public EnemyAnimator Animator => _animator;
        public FloatingText DamageNumber => _damageNumber;
        
        public CapsuleCollider Collider { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public NavMeshAgent NavMeshAgent { get; private set; }
        public CoroutineRunner Coroutine { get; private set; }
        public EnemyInvoker Invoker { get; private set; }
        
        protected List<EnemyFeature> _features;
        
        private void Awake()
        {
            _features = GetComponents<EnemyFeature>().ToList();
            Collider = GetComponent<CapsuleCollider>();
            Rigidbody = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            NavMeshAgent.updateRotation = false;
            Coroutine = GetComponent<CoroutineRunner>();

            Invoker = new(this);
            
            foreach (var feature in _features)
            {
                Dependencies.TryAddFeature(feature);
            }
            
            foreach (var feature in _features)
            {
                feature.InitializeFeature(this);
            } 
            
            _spaitalUI.worldCamera = Camera.main;
        }

        private void Update()
        {
            foreach (var feature in _features)
            {
                feature.UpdateFeature();
            }
        }

        private void FixedUpdate()
        {
            foreach (var feature in _features)
            {
                feature.FixedUpdateFeature();
            } 
        }

        public override void Deactivate(out SharedProperties shared)
        {
            shared = new SharedProperties();
            foreach (var feature in _features)
            {
                feature.ResetFeature(ref shared);
            }
            
            gameObject.SetActive(false); 
            shared.position = transform.position;
            shared.direction = transform.forward;
            Invoker.Velocity.Execute(Vector3.zero);
        }

        public override void Reactivate(SharedProperties shared)
        {
            transform.position = shared.position;
            transform.forward = shared.direction;
            gameObject.SetActive(true);
            
            foreach (var feature in _features)
            {
                feature.ReInitializeFeature(this, shared);
            }
        } 
    }
}