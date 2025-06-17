using System.Collections.Generic;
using System.Linq;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

namespace Movement3D.Gameplay
{
    public class EnemyHitbox
    {
        private EnemyAttack _attack;
        private EnemyAttributes _attributes;
        private ForwardHandler _playerForward;
        
        public bool active { get; private set; }
        private StopwatchTimer _timer = new();
        
        //Cache
        private Transform _bodyPart;
        private LayerMask _enemyLayer;
        private List<string> _includeTag;
        private SingleHit _hit;
        private GameObject _vfxPrefab;
        private VisualEffect _vfx;
        private bool _hasHit;
        private GameObject _sender;
        
        public EnemyHitbox(EnemyAttack attack, EnemyAttributes attributes, ForwardHandler forward)
        {
            _playerForward = forward;
            _attack = attack;
            _attributes = attributes;
            _includeTag = new List<string>(attack.Targeted);
            _enemyLayer = attack.AttackLayer;
        }

        public void Update(float deltaTime)
        {
            if (!active) return;
            _timer.Tick(deltaTime);
        }

        public void FixedUpdate()
        {
            float time = _timer.GetTime();
            if (_timer.IsRunning && time >= _hit.delay && time <= _hit.delay + _hit.duration) CheckHit();
        }

        public void Init(SingleHit hit, GameObject sender)
        {
            _hit = hit;
            _hit.radius *= _attack.MultiplierScale;
            _vfxPrefab = hit.vfxPrefab;
            _attack._bodyPartsDictionary.TryGetValue(hit.bodyPartName, out _bodyPart);
            _hasHit = false;
            _sender = sender;
            if(_attack.CurrentAttack.chargeAttack || _attack.CurrentAttack.defensePosture) AttackVFX();
        }

        public Vector3 GetPosition()
        {
            if (!active) return default;

            var forward = _playerForward.Get();
            var offset = forward * _hit.positionOffset.z + Vector3.up * _hit.positionOffset.y + Vector3.Cross(Vector3.up, forward) * _hit.positionOffset.x;

            return _bodyPart.position + offset;
        }

        public void CheckHit()
        {
            var position = GetPosition();

            var collisions = Physics.OverlapSphere(position, _hit.radius, _enemyLayer).ToList();

            bool hit = false;
            foreach (var collider in collisions)
            {
                string tag = collider.gameObject.tag;
                
                if (collider.gameObject == _sender || !_includeTag.Contains(tag)) continue;

                HitEnemy(collider.gameObject.GetComponent<PlayerController>(), position);
                HitAlly(collider.gameObject.GetComponent<EnemyController>(), position);
                collider.gameObject.GetComponent<DestroyableProp>()?.Attack(_hit.damage);
                
                hit = true;
            }

            if (hit && !_hasHit)
            {
                _hasHit = true;
                OnHit();
            }
        }

        public void HitEnemy(PlayerController enemy, Vector3 position)
        {
            if (enemy == null) return;
            
            enemy.Dependencies.TryGetFeature(out Resource resource);
            if(resource == null || _attack.CurrentAttack == null) return;
            resource.Attack(new HitInfo
            {
                priority = _attack.CurrentAttack.priority,
                hit = _hit,
                position = position,
                direction = _playerForward.Get(),
                projectile = false,
                success = true,
                stunSuccess = true
            });
        }
        
        public void HitAlly(EnemyController enemy, Vector3 position)
        {
            if (enemy == null) return;
            
            enemy.Dependencies.TryGetFeature(out EnemyResource resource);
            if(resource == null || _attack.CurrentAttack == null) return;
            resource.Attack(new HitInfo
            {
                priority = _attack.CurrentAttack.priority,
                hit = _hit,
                position = position,
                direction = _playerForward.Get(),
                projectile = false,
                success = true,
                stunSuccess = true
            });
        }
        
        public void AttackVFX()
        {
            if(_vfxPrefab != null) _vfx = VFXPool.Instance.Get(_vfxPrefab, GetPosition());
        }

        public void OnHit()
        {
            AttackVFX();
        }

        public void OnGet()
        {
            active = true;
        }

        public void OnRelease()
        {
            if(!active) return;
            
            _timer.Reset();
            _timer.Stop();
            active = false;
            _bodyPart = null;
            if(_vfxPrefab != null && _vfx != null && _vfx.gameObject.activeSelf) VFXPool.Instance.Return(_vfx, _vfxPrefab);
            _vfx = null;
            _vfxPrefab = null;
        }

        public void OnAttackTick(int tick)
        {
            if (!active) return;
            if (tick == _hit.tick)
            {
                _timer.Start();
            }
        }
    }

    public class EnemyHitboxPool
    {
        private ObjectPool<EnemyHitbox> _hitboxPool;
        private List<EnemyHitbox> _activeHitboxes = new();
        private EnemyAttack _attack;
        
        public EnemyHitboxPool(int prewarm, EnemyAttack attack, EnemyAttributes attributes, ForwardHandler forward)
        {
            _attack = attack;
            
            EnemyHitbox CreateFunc()
            {
                return new EnemyHitbox(attack, attributes, forward);
            }

            void OnGet(EnemyHitbox hitbox)
            {
                hitbox.OnGet();
            }

            void OnRelease(EnemyHitbox hitbox)
            {
                hitbox.OnRelease();
            }

            void Destroy(EnemyHitbox hitbox)
            {
                hitbox.OnRelease();
            }
            
            _hitboxPool = new(CreateFunc, OnGet, OnRelease, Destroy, defaultCapacity: prewarm);
        }

        public EnemyHitbox GetHit(SingleHit hitInfo, GameObject sender)
        {
            var hit = _hitboxPool.Get();
            hit.Init(hitInfo, sender);
            return hit;
        }

        public void ReturnHit(EnemyHitbox hitbox)
        {
            _hitboxPool.Release(hitbox);
        }

        public void AttackTick(int tick)
        {
            _activeHitboxes.ForEach(hitbox => hitbox.OnAttackTick(tick));
        }

        public void Update(float deltaTime)
        {
            _activeHitboxes.ForEach(hitbox => hitbox.Update(deltaTime));
        }

        public void FixedUpdate()
        {
            _activeHitboxes.ForEach(hitbox => hitbox.FixedUpdate());
        }

        public void OnStartAttack(List<SingleHit> hits, GameObject sender)
        {
            if(hits.Count == 0) return;
            
            foreach (var hit in hits)
            {
                var hitbox = GetHit(hit, sender);
                _activeHitboxes.Add(hitbox);
            }
            _attack.MultiplierReset();
        }

        public void OnEndAttack()
        {
            _activeHitboxes.ForEach(hitbox => ReturnHit(hitbox));
            _activeHitboxes.Clear();
        }
    }
}