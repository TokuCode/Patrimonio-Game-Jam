using Movement3D.Helpers;
using Unity.Cinemachine;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class PlayerController : Controller
    {
        [SerializeField] private string _playerName;
        public string Name => _playerName;
        [SerializeField] private bool _isPlayer;
        public bool IsPlayer => _isPlayer;
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

        [SerializeField] private Canvas _spaitalUI;
        [SerializeField] private FloatingText _damageNumber;
        public FloatingText DamageNumber => _damageNumber;
        
        public CapsuleCollider Capsule { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        
        [SerializeField] private PlayerAnimator _animator;
        public PlayerAnimator Animator => _animator;
        
        private IControls _controls;
        public Pipeline<InputPayload> InputPipeline { get; private set; } = new();
        public Invoker Invoker { get; private set; }

        protected override void Awake()
        {
            Capsule = GetComponent<CapsuleCollider>();
            Rigidbody = GetComponent<Rigidbody>();
            _explorerCamera = GameObject.FindGameObjectWithTag("ExplorationCam").GetComponent<CinemachineCamera>();
            _combatCamera = GameObject.FindGameObjectWithTag("CombatCam").GetComponent<CinemachineCamera>();
            _strategyCamera = GameObject.FindGameObjectWithTag("StrategyCam").GetComponent<CinemachineCamera>();
            _immersiveCamera = GameObject.FindGameObjectWithTag("ImmersiveCam").GetComponent<CinemachineCamera>();
            
            if(_isPlayer) _controls = InputReader.Instance;
            Invoker = new Invoker(this);
            
            base.Awake();
            if (_isPlayer)
            {
                _spaitalUI.gameObject.SetActive(false);
                InputReader.Instance.CacheController(this);
            }
            else _spaitalUI.worldCamera = Camera.main;
        }

        protected override void Update()
        {
            base.Update();
            ReadInput(UpdateContext.Update);
            Invoker.Update(Time.deltaTime);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
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
            base.Deactivate(out shared);
            shared.position = transform.position;
        }

        public override void Reactivate(SharedProperties shared)
        {
            transform.position = shared.position;
            base.Reactivate(shared);
        }
    }
}