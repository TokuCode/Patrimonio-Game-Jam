using System.Collections.Generic;
using System.Linq;
using Movement3D.Helpers;
using Unity.Cinemachine;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class PlayerController : Controller
    {
        [SerializeField] private string _playerName;
        public string Name => _playerName;
        [SerializeField] private Transform _orientation;
        public Transform Orientation => _orientation;
        [SerializeField] private Transform _playerObj;
        public Transform PlayerObj => _playerObj;
        [SerializeField] private Transform _combatLookAt;
        public Transform CombatLookAt => _combatLookAt;
        [SerializeField] private Transform _cameraPosition;
        public Transform CameraPosition => _cameraPosition;
        [SerializeField] private Transform _lookAt;
        public Transform LookAt => _lookAt;
        [SerializeField] private Transform _cameraReferences;
        public Transform CameraReferences => _cameraReferences;
        [SerializeField] private Transform _cameraTrackingTarget;
        public Transform CameraTrackingTarget => _cameraTrackingTarget;
        [SerializeField] private Transform _lookAtFollow;
        public Transform LookAtFollow => _lookAtFollow;
        [SerializeField] private Transform _worldUIPosition;
        public Transform WorldUIPosition => _worldUIPosition;
        [SerializeField] private AnimationCurve _suckToTargetEase;
        public AnimationCurve SuckToTargetEase => _suckToTargetEase;
        
        private CinemachineCamera _explorerCamera;
        public CinemachineCamera ExplorerCamera => _explorerCamera;
        private CinemachineCamera _combatCamera;
        public CinemachineCamera CombatCamera => _combatCamera;
        private CinemachineCamera _strategyCamera;
        public CinemachineCamera StrategyCamera => _strategyCamera;
        private CinemachineCamera _immersiveCamera;
        public CinemachineCamera ImmersiveCamera => _immersiveCamera;
        public CapsuleCollider Capsule { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        
        [SerializeField] private PlayerAnimator _animator;
        public PlayerAnimator Animator => _animator;
        
        private IControls _controls;
        public Pipeline<InputPayload> InputPipeline { get; private set; } = new();
        public PlayerInvoker Invoker { get; private set; }
        protected List<PlayerFeature> _features;

        private void Awake()
        {
            _features = GetComponents<PlayerFeature>().ToList();
            Capsule = GetComponent<CapsuleCollider>();
            Rigidbody = GetComponent<Rigidbody>();
            _explorerCamera = GameObject.FindGameObjectWithTag("ExplorationCam").GetComponent<CinemachineCamera>();
            _combatCamera = GameObject.FindGameObjectWithTag("CombatCam").GetComponent<CinemachineCamera>();
            _strategyCamera = GameObject.FindGameObjectWithTag("StrategyCam").GetComponent<CinemachineCamera>();
            _immersiveCamera = GameObject.FindGameObjectWithTag("ImmersiveCam").GetComponent<CinemachineCamera>();
            
            _controls = InputReader.Instance;
            Invoker = new PlayerInvoker(this);
            
            foreach (var feature in _features)
            {
                Dependencies.TryAddFeature(feature);
            }
            
            foreach (var feature in _features)
            {
                feature.InitializeFeature(this);
            }             
            
            InputReader.Instance.CacheController(this);
        }

        private void Update()
        {
            foreach (var feature in _features)
            {
                feature.UpdateFeature();
            }
            ReadInput(UpdateContext.Update);
            Invoker.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            foreach (var feature in _features)
            {
                feature.FixedUpdateFeature();
            }
            ReadInput(UpdateContext.FixedUpdate);
        }

        private void ReadInput(UpdateContext context)
        {
            if (_controls == null || !gameObject.activeSelf) return;
            
            InputPayload input = new()
            {
                MoveDirection = _controls.MoveDirection,
                Jump = _controls.Jump,
                Run = _controls.Run,
                Crouch = _controls.Crouch,
                Signal = InputBuffer.Instance.Signal,
                Context = context
            };
            
            InputPipeline.Process(ref input);
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
            shared.direction = _playerObj.forward;
            Invoker.Velocity.Execute(Vector3.zero);
        }

        public override void Reactivate(SharedProperties shared)
        {
            transform.position = shared.position;
            _playerObj.forward = shared.direction;
            _orientation.forward = shared.direction;
            gameObject.SetActive(true);
            
            foreach (var feature in _features)
            {
                feature.ReInitializeFeature(this, shared);
            }
        }
    }
}