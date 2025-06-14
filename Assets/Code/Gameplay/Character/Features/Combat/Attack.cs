using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Attack : Feature, IProcess<HitInfo>
    {
        private const int postureDefensiveMaxSpan = 10;
        
        [Serializable]
        public struct BodyPart
        {
            public string name;
            public Transform transform;
        }
        
        private Movement movement;
        private ThirdPersonCamera camera;
        private PlayerAnimator animator;
        private Resource resource;
        private PhysicsCheck physics;
        private ComboReader combo;
        
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
        public FullAttack CurrentAttack => _currentAttack;

        [Header("Suck To Target Settings")] 
        [SerializeField] private float _aimAngle;
        [SerializeField] private float _minRange;
        [SerializeField] private float _defaultTargetRadius;
        [SerializeField] private float _suckToTargetduration;
        [SerializeField] private float _alignDuration;
        [SerializeField] private LayerMask _solid;

        [Header("Aim Distance Calculation")]
        [SerializeField] private float _aimAngleWeight;
        [SerializeField] private float _aimRadiusWeight;
        
        //Extras Variables for Effects
        private bool _waiting;
        public bool Waiting => _waiting;
        private bool _charging;
        public bool Charging => _charging;
        private float _waitTime;
        private float _releaseMultiplier;

        [Header("Tuning Settings")]
        [SerializeField] private float _shootOffset;
        [SerializeField] private float _multiplierScale; 
        public float MultiplierScale => _multiplierScale * (_releaseMultiplier - 1) + 1;
        [SerializeField] private float _multiplierDamage; 
        public float MultiplierDamage => _multiplierDamage + (_releaseMultiplier - 1) + 1;
        [SerializeField] private float _multiplierKnockback; 
        public float MultiplierKnockback => _multiplierKnockback + (_releaseMultiplier - 1) + 1;
        [SerializeField] private float _multiplierSpeed; 
        public float MultiplierSpeed => _multiplierSpeed * (_releaseMultiplier - 1) + 1;
        
        private bool _isDashing;
        public bool IsDashing => _isDashing;
        private bool _isSuspended;
        public bool IsSuspended => _isSuspended;
        private bool _onPosture;
        public bool OnPosture => _onPosture;
        private CountdownTimer _postureTimer;
        private Vector3 _lastDirectionToTarget;

        public override void ResetFeature(ref SharedProperties shared)
        {
            _isAttacking = false;
            _currentAttack = null;
            _hitboxPool.OnEndAttack();
            _isDashing = false;
            _isSuspended = false;
            _onPosture = false;
            _charging = false;
            _lastDirectionToTarget = Vector3.zero;
            _waiting = false;
            _postureTimer.Stop();
            MultiplierReset();
        }

        public override void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            _dependencies.TryGetFeature(out movement);
            _dependencies.TryGetFeature(out camera);
            _dependencies.TryGetFeature(out resource);
            _dependencies.TryGetFeature(out physics);
            _dependencies.TryGetFeature(out combo);
            if (controller is PlayerController player) animator = player.Animator;
            MultiplierReset();
            CacheBodyParts();
            _hitboxPool = new(4, this, _invoker.PlayerForward);
            resource.OnStun += CancelAttack;
            _postureTimer = new(1);
            _postureTimer.OnTimerStop += () => { Interruption(false); };
            resource.pipeline.Register(this);
        }

        public override void UpdateFeature()
        {
            _postureTimer.Tick(Time.deltaTime);
            _hitboxPool.Update(Time.deltaTime);
        }

        public override void FixedUpdateFeature()
        {
            Dash();
            WaitForGround();
            _hitboxPool.FixedUpdate();
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
            CheckTargetForSnap(attack);
            animator.AttackOverride(attack);
            animator.Attack();
            movement.Block();
            camera.locked = true;
            if(attack.downAttack) DownAttack();
            if (attack.airSuspension) AirSuspension();
            if(attack.chargeAttack || attack.downAttack || attack.defensePosture) animator.Freeze(true);
            
            OnStartAttack?.Invoke();
        }

        public void EndAttack()
        {
            if(!_isAttacking || _waiting) return;

            if (_currentAttack.chargeAttack || _currentAttack.downAttack || _currentAttack.defensePosture)
            {
                _waiting = true;
                _waitTime = Time.time;
                if(_currentAttack.downAttack) DownAttack();
                if(_currentAttack.defensePosture) DefensePosture();
                if(_currentAttack.chargeAttack) _charging = true;
            }
            else
            {
                _isDashing = false;
                _isAttacking = false;
                _currentAttack = null;
                _isSuspended = false;
                _onPosture = false;
                _charging = false;
            }
            
            movement.Free();
            camera.locked = false;
            
            OnEndAttack?.Invoke();
        }

        public void CancelAttack()
        {
            if(!_isAttacking) return;
            
            _isAttacking = false;
            _currentAttack = null;
            _hitboxPool.OnEndAttack();
            movement.Free();
            camera.locked = false;
            _isDashing = false;
            _isSuspended = false;
            _onPosture = false;
            _charging = false;
            
            OnEndAttack?.Invoke();
        }

        public bool PriorityCheck(int priority)
        {
            if(!_isAttacking || _currentAttack == null) return true;

            int myAttackPriority = _currentAttack.priority;
            return myAttackPriority < priority; //returns true if incoming attack is stronger
        }

        public void Tick()
        {
            if(!IsAttacking) return;
            
            _hitboxPool.AttackTick(tick);
            if(tick == _currentAttack.dashTick && _currentAttack.dashAttack) _isDashing = true;
            if(_currentAttack.shootAttack) Shoot(tick);
            else _isDashing = false;
            tick++;
        }

        private void CheckTargetForSnap(FullAttack attack)
        {
            _lastDirectionToTarget = Vector3.zero;
            
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
                var distance = Vector3.Distance(center, centerTarget);
                var forward = _invoker.PlayerForward.Get();
                var angle = Vector3.Angle(directionToTarget, forward);
                
                bool obstacle = Physics.Raycast(center, directionToTarget, distance, _solid);

                if (Mathf.Abs(angle) > _aimAngle / 2 || obstacle) continue;
                
                var absDistance = DistanceRadial(Mathf.Abs(angle), distance);
                if (absDistance < closestDistance)
                {
                    closest = temp;
                    closestDistance = distance;
                }
            }

            SuckToTarget(closest, attack);
            FollowUp(closest, attack);
        }

        private void DownAttack()
        {
            if(!_isAttacking || _currentAttack == null) return;
            
            if(physics.OnGround || !_currentAttack.downAttack) return;
            
            _invoker.AddForce.Execute(new(Vector3.down, _currentAttack.downImpulse, ForceMode.VelocityChange));
        }

        private void FollowUp(PlayerController closest, FullAttack attack)
        {
            if (!attack.followUp) return;
            
            var followUpForce = attack.followUpForce;
            Vector3 direction;
            if (closest == null)
            {
                var targetCenter = _invoker.CenterPosition.Get();
                var center = _invoker.CenterPosition.Get();
                direction = (targetCenter - center).With(y: 0).normalized;
            }
            else
            {
                direction = _invoker.PlayerForward.Get();
            }

            _invoker.AddForce.Execute(new(direction * followUpForce.x + Vector3.up * followUpForce.y, ForceMode.VelocityChange));
        }

        public void Dash()
        {
            if(!IsAttacking || _currentAttack == null) return;

            if (tick != _currentAttack.dashTick) return;
            
            var direction = _invoker.PlayerForward.Get();
            _invoker.AddForce.Execute(new(direction, _currentAttack.dashImpulse, ForceMode.Acceleration));
        }

        public void Shoot(int tick)
        {
            if(!IsAttacking || _currentAttack == null) return;
            
            if(!_currentAttack.shootAttack) return;

            var forward = _invoker.PlayerForward.Get();
            var shootPosition = _bodyPartsDictionary[_currentAttack.attacks[tick].bodyPartName].position + forward * _shootOffset;
            var projectile = ObjectPoolManager.Instance.Get(_currentAttack.projectile, shootPosition, Quaternion.identity).GetComponent<Projectile>();

            bool chain = _currentAttack.chainEffects && tick == _currentAttack.chainTick;

            projectile.Init(shootPosition, _lastDirectionToTarget != Vector3.zero ? _lastDirectionToTarget : forward, _currentAttack.priority, this, chain);
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

        private float DistanceRadial(float angle, float radius)
        {
            float distance = angle * _aimAngleWeight + radius * _aimRadiusWeight;
            return distance;
        }
        
        private void SuckToTarget(PlayerController target, FullAttack attack)
        {
            if (target == null) return;
            
            var targetCenter = target.Invoker.CenterPosition.Get(); 
            var center = _invoker.CenterPosition.Get();
            var distance = (targetCenter - center).With(y: 0).magnitude;
            var directionToTarget = (targetCenter - center).With(y: 0).normalized;

            _invoker.Forward.Execute(directionToTarget);
            _invoker.PlayerForward.Execute(directionToTarget);
            
            if(distance >= _minRange && attack.suckToTarget) _invoker.SuckToTarget.Execute(new SuckToTargetParams
            {
                position = GetTargetOutPosition(target),
                duration = _suckToTargetduration
            });
            
            _lastDirectionToTarget = directionToTarget;
        }

        public void Interruption(bool success)
        {
            if(!_waiting || !(_currentAttack.chargeAttack || _currentAttack.defensePosture)) return;
            _waiting = false;
            
            var lastAttack = _currentAttack;
            _isAttacking = false;
            _currentAttack = null;
            _isDashing = false;
            _isSuspended = false;
            _onPosture = false;
            _charging = false;
            _hitboxPool.OnEndAttack();
            animator.Freeze(false);
            
            if(!lastAttack.chargeAttack || !success) return;
            
            _releaseMultiplier = Mathf.Min((Time.time - _waitTime) / lastAttack.chargeTime * lastAttack.chargeMultiplier, lastAttack.chargeMultiplier) + 1;
        }

        public void WaitForGround()
        {
            if (!physics.OnGround) return;
            
            if (!_waiting || !_currentAttack.downAttack) return;
            _waiting = false;
            _releaseMultiplier = Mathf.Min((Time.time - _waitTime) / _currentAttack.downTime * _currentAttack.downMultiplier, _currentAttack.downMultiplier) + 1;
            
            _isAttacking = false;
            _currentAttack = null;
            _isDashing = false;
            _isSuspended = false;
            _onPosture = false;
            _charging = false;
            _hitboxPool.OnEndAttack();
            animator.Freeze(false);
            
            combo.ForcedTravel();
        }

        public void MultiplierReset()
        {
            _releaseMultiplier = 1;
        }

        private void AirSuspension()
        {
            if(physics.OnGround) return;
            
            var velocity = _invoker.Velocity.Get().With(y: 0);
            _invoker.Velocity.Execute(velocity);
            
            _isSuspended = true;
        }

        private void DefensePosture()
        {
            _onPosture = true;
            _postureTimer.Reset(_currentAttack.defensePostureMaxTime);
            _postureTimer.Start();
        }

        public void Apply(ref HitInfo hitInfo) //Counter Attack
        {
            if(!_onPosture) return;
            
            if(!_isAttacking || _currentAttack == null) return;

            _postureTimer.Stop();
            var incomingPriority = hitInfo.priority;
            var myAttackPriority = _currentAttack.priority;
            if (incomingPriority < _currentAttack.priority)
            {
                hitInfo.success = false;
                if(!hitInfo.projectile) combo.ForcedTravel();
            }
            else
            {
                int diff = incomingPriority - myAttackPriority;
                int reduction = Mathf.Max(10 - diff, 1);
                
                hitInfo.hit.knockback /= reduction;
                hitInfo.hit.damage /= reduction;
            }
        }
    }

    [Serializable]
    public struct SingleHit
    {
        //Hit Info Space
        [Header("Positioning")]
        public string bodyPartName;
        public Vector3 positionOffset;
        public float radius;
        
        //Hit Info Time
        [Header("Timing")]
        public int tick;
        public float delay;
        public float duration;
        
        //Primary Effects
        [Header("Effects")]
        public float damage;
        public Vector2 knockback;
        public float stunTime;
        
        //Visual
        [Header("Visual")] 
        public GameObject vfxPrefab;
    }

    [Serializable]
    public struct HitInfo
    {
        public SingleHit hit;
        public Vector3 position;
        public int priority;
        public bool projectile;
        public bool success;
    }
}