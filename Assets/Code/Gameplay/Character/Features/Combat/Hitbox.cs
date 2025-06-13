using System.Collections.Generic;
using System.Linq;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.TextCore.Text;
using UnityEngine.VFX;

namespace Movement3D.Gameplay
{
    public class Hitbox
    {
        private Attack _attack;
        
        public bool active { get; private set; }
        private StopwatchTimer _timer = new();
        
        //Cache
        private Transform _bodyPart;
        private LayerMask _enemyLayer;
        private List<string> _excludeTag;
        private List<string> _includeTag;
        private SingleHit _hit;
        private GameObject _vfxPrefab;
        private VisualEffect _vfx;
        private bool _hasHit;
        
        public Hitbox(Attack attack)
        {
            _attack = attack;
            _excludeTag = new List<string> { attack.gameObject.tag };
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

        public void Init(SingleHit hit)
        {
            _hit = hit;
            hit.damage *= _attack.MultiplierDamage;
            hit.knockback *= _attack.MultiplierKnockback;
            hit.radius *= _attack.MultiplierScale;
            _vfxPrefab = hit.vfxPrefab;
            _attack._bodyPartsDictionary.TryGetValue(hit.bodyPartName, out _bodyPart);
            _hasHit = false;
            if(_attack.CurrentAttack.chargeAttack || _attack.CurrentAttack.defensePosture) AttackVFX();
        }

        public Vector3 GetPosition()
        {
            if (!active) return default;

            return _bodyPart.position + _hit.positionOffset;
        }

        public void CheckHit()
        {
            var position = GetPosition();

            var collisions = Physics.OverlapSphere(position, _hit.radius, _enemyLayer).ToList();

            bool hit = false;
            foreach (var collider in collisions)
            {
                string tag = collider.gameObject.tag;
                
                if (_excludeTag.Contains(tag) || !_includeTag.Contains(tag)) continue;
                
                HitEnemy(collider.gameObject.GetComponent<PlayerController>(), position);
                
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
            resource.Attack(new HitInfo
            {
                priority = _attack.CurrentAttack.priority,
                hit = _hit,
                position = position,
                success = true,
                projectile = false
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

    public class HitboxPool
    {
        private ObjectPool<Hitbox> _hitboxPool;
        private List<Hitbox> _activeHitboxes = new();
        private Attack _attack;
        
        public HitboxPool(int prewarm, Attack attack)
        {
            _attack = attack;
            
            Hitbox CreateFunc()
            {
                return new Hitbox(attack);
            }

            void OnGet(Hitbox hitbox)
            {
                hitbox.OnGet();
            }

            void OnRelease(Hitbox hitbox)
            {
                hitbox.OnRelease();
            }

            void Destroy(Hitbox hitbox)
            {
                hitbox.OnRelease();
            }
            
            _hitboxPool = new(CreateFunc, OnGet, OnRelease, Destroy, defaultCapacity: prewarm);
        }

        public Hitbox GetHit(SingleHit hitInfo)
        {
            var hit = _hitboxPool.Get();
            hit.Init(hitInfo);
            return hit;
        }

        public void ReturnHit(Hitbox hitbox)
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

        public void OnStartAttack(List<SingleHit> hits)
        {
            if(hits.Count == 0) return;
            
            foreach (var hit in hits)
            {
                var hitbox = GetHit(hit);
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