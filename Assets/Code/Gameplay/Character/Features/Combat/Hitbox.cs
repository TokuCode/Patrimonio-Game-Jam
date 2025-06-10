using System.Collections.Generic;
using System.Linq;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.Pool;
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
        private VisualEffect _visualEffect;
        private bool _hasHit;
        
        public Hitbox(Attack attack, GameObject visualEffect)
        {
            _attack = attack;
            _excludeTag = new List<string> { attack.gameObject.tag };
            _includeTag = new List<string>(attack.Targeted);
            _enemyLayer = attack.AttackLayer;
            _visualEffect = visualEffect.GetComponent<VisualEffect>();
        }

        public void Update(float deltaTime)
        {
            if (!active) return;
            _timer.Tick(deltaTime);

            float time = _timer.GetTime();
            if (_timer.IsRunning && time >= _hit.delay && time <= _hit.delay + _hit.duration) CheckHit();
        }

        public void Init(SingleHit hit)
        {
            _hit = hit;
            _attack._bodyPartsDictionary.TryGetValue(hit.bodyPartName, out _bodyPart);
            _hasHit = false;
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
                
                //Hit Logic
                
                hit = true;
            }

            if (hit && !_hasHit)
            {
                _hasHit = true;
                OnHit(position);
            }
        }

        public void AttackVFX(Vector3 position)
        {
            //Vfx Logic
            _visualEffect.Reinit();
            _visualEffect.SetVector3("Position", position);
            _visualEffect.Play();
        }

        public void OnHit(Vector3 position)
        {
            AttackVFX(position);
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
            _visualEffect.Stop();
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
        
        public HitboxPool(int prewarm, Attack attack, GameObject _vfx)
        {
            Hitbox CreateFunc()
            {
                var vfx = GameObject.Instantiate(_vfx, Vector3.zero, Quaternion.identity);
                return new Hitbox(attack, vfx);
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

        public void OnStartAttack(List<SingleHit> hits)
        {
            foreach (var hit in hits)
            {
                var hitbox = GetHit(hit);
                _activeHitboxes.Add(hitbox);
            }
        }

        public void OnEndAttack()
        {
            _activeHitboxes.ForEach(hitbox => ReturnHit(hitbox));
            _activeHitboxes.Clear();
        }
    }
}