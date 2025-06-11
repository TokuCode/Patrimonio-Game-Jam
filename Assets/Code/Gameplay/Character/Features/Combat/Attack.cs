using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Attack : Feature
    {
        [Serializable]
        public struct BodyPart
        {
            public string name;
            public Transform transform;
        }
        
        private Movement movement;
        private ThirdPersonCamera camera;
        private PlayerAnimator animator;
        
        [SerializeField] private BodyPart[] _bodyParts;
        public Dictionary<string, Transform> _bodyPartsDictionary = new();

        private bool _isAttacking;
        public bool IsAttacking => _isAttacking;

        public event Action OnStartAttack;
        public event Action OnEndAttack;
        
        [Header("Hit Settings")]
        [SerializeField] private List<string> _targetedTags;
        public List<string> Targeted => _targetedTags;
        [SerializeField] private LayerMask _attackLayer;
        public LayerMask AttackLayer => _attackLayer;
        private HitboxPool _hitboxPool;
        private int tick;
        private FullAttack _currentAttack;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject _visualEffectPrefab;

        [Header("Suck To Target Settings")] 
        [SerializeField] private float _aimAngle;
        [SerializeField] private float _minRange;
        [SerializeField] private float _defaultTargetRadius;
        [SerializeField] private float _suckToTargetduration;
        [SerializeField] private float _alignDuration;
        
        //Extras Variables for Effects
        private bool waiting;

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out camera);
            if (controller is PlayerController player) animator = player.Animator;
            CacheBodyParts();
            _hitboxPool = new(4, this, _visualEffectPrefab);
        }

        public override void UpdateFeature()
        {
            _hitboxPool.Update(Time.deltaTime);
        }

        private void CacheBodyParts()
        {
            foreach (var bodyPart in _bodyParts)
            {
                _bodyPartsDictionary.Add(bodyPart.name, bodyPart.transform);
            }
        }

        public void StartAttack(FullAttack attack)
        {
            if(_isAttacking) return;
            
            _isAttacking = true;
            _currentAttack = attack;
            _hitboxPool.OnStartAttack(attack.attacks);
            tick = 0;
            if(camera.CurrentCamera != ThirdPersonCamera.CameraStyle.Immersive) CheckTargetForSnap();
            animator.AttackOverride(attack);
            animator.Attack();
            movement.Block();
            camera.locked = true;
            
            OnStartAttack?.Invoke();
        }

        public void EndAttack()
        {
            if(!_isAttacking) return;
            
            if (!_currentAttack.chargeAttack)
            {
                _isAttacking = false;
                _currentAttack = null;
            }
            else waiting = true;
            
            _hitboxPool.OnEndAttack();
            movement.Free();
            camera.locked = false;
            
            OnEndAttack?.Invoke();
        }

        public void Tick()
        {
            if(!IsAttacking) return;
            
            _hitboxPool.AttackTick(tick);
            tick++;
        }

        private void CheckTargetForSnap()
        {
            if (!_currentAttack.suckToTarget) return;
            
            float range = Mathf.Max(_minRange, _currentAttack.attackRange);
            var floorPosition = _invoker.FloorPosition.Get();
            
            var collissions = Physics.OverlapSphere(floorPosition, range, _attackLayer);

            float closestDistance = float.MaxValue;
            PlayerController closest = null;
            foreach (var collission in collissions)
            {
                string tag = collission.gameObject.tag;

                if (gameObject.CompareTag(tag) || !_targetedTags.Contains(tag)) continue; 
                
                var temp = collission.GetComponent<PlayerController>();
                
                if(temp == null) continue;

                var centerTarget = temp.Invoker.CenterPosition.Get();
                var center = _invoker.CenterPosition.Get();
                var directionToTarget = (centerTarget - center).With(y: 0).normalized;
                var forward = _invoker.PlayerForward.Get();
                var angle = Vector3.SignedAngle(directionToTarget, forward, Vector3.forward);

                if (Mathf.Abs(angle) > _aimAngle / 2) return;
                
                var distance = Vector3.Distance(centerTarget, center);
                if (distance < closestDistance)
                {
                    closest = temp;
                    closestDistance = distance;
                }
            }

            if (closest == null) return;
            
            SuckToTarget(closest);
        }

        private Vector3 GetTargetOutPosition(PlayerController target)
        {
            var radius = _invoker.Radius.Get();
            var targetCenter = target.Invoker.CenterPosition.Get(); 
            var targetFloor = target.Invoker.FloorPosition.Get();
            var center = _invoker.CenterPosition.Get();
            var directionFromTarget = (center - targetCenter).With(y: 0).normalized;
            return targetFloor + directionFromTarget * radius;
        }
        
        private void SuckToTarget(PlayerController target)
        {
            var targetCenter = target.Invoker.CenterPosition.Get(); 
            var floor = _invoker.FloorPosition.Get();
            var center = _invoker.CenterPosition.Get();
            var directionToTarget = (targetCenter - center).With(y: 0).normalized;
            var suckPosition = GetTargetOutPosition(target);
            var suckDistance = (suckPosition - floor).With(y: 0).magnitude;
            
            if(suckDistance >= _minRange) _invoker.SuckToTarget.Execute(new SuckToTargetParams
            {
                position = GetTargetOutPosition(target),
                duration = _suckToTargetduration
            });

            _invoker.AlignCamera.Execute(new AlignCameraParams
            {
                direction = directionToTarget,
                duration = _alignDuration
            });
        }

        public void WaitForReleaseOrInterruption()
        {
            if(!waiting || !_currentAttack.chargeAttack) return;
            waiting = false;
            
            _isAttacking = false;
            _currentAttack = null;
        }
    }

    [Serializable]
    public struct SingleHit
    {
        //Hit Info Space
        public string bodyPartName;
        public Vector3 positionOffset;
        public float radius;
        
        //Hit Info Time
        public int tick;
        public float delay;
        public float duration;
        
        //Primary Effects
        public int priority; //More priority, less possibility to be canceled and to cancel incoming attacks
        public float damage;
        public Vector2 knockback;
        public float stunTime;
        public Vector2 followUp;
    }

    [Serializable]
    public struct HitInfo
    {
        public SingleHit hit;
        
        public Vector3 position;

        public bool success;
    }
}