using System.Collections.Generic;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _direction;
        private SingleHit _currentHit;
        private Vector3 _currentSpeed;
        private Vector3 _initialScale;
        private List<string> _excludeTag = new();
        private List<string> _includeTag = new();
        private int _priority;
        private CountdownTimer _timer;

        [Header("Settings")] [SerializeField] private float _speed;
        [SerializeField] private float _lifeSpan;
        [SerializeField] private SingleHit _hit;
        [SerializeField] private GameObject _vfxPrefab;
        [SerializeField] private LayerMask _solid;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _initialScale = transform.localScale;
            _timer = new CountdownTimer(_lifeSpan);
            _timer.OnTimerStop += Reset;
        }

        private void Update()
        {
            _timer.Tick(Time.deltaTime);
        }

        public void Init(Vector3 position, Vector3 direction, int priority, Attack attack, Attributes attributes, bool chain)
        {
            _direction = direction.With(y:0).normalized;
            _currentHit = _hit;
            transform.localScale = _initialScale;
            transform.forward = direction;
            transform.position = position;
            _rigidbody.position = position;
            _currentSpeed = _direction.normalized * _speed;
            _excludeTag = new List<string> { attack.gameObject.tag };
            _includeTag = new List<string>(attack.Targeted);
            _priority = priority;

            if (chain)
            {
                transform.localScale *= attack.MultiplierScale;
                _currentSpeed *= attack.MultiplierSpeed;
                _currentHit.damage *= attack.MultiplierDamage;
                _currentHit.knockback *= attack.MultiplierKnockback;
                attack.MultiplierReset();
            }

            _currentHit.damage *= attributes.AttackPower;
            _currentHit.knockback *= attributes.KnockbackPower;
            _currentHit.stunPower *= attributes.StunPower;
            _currentHit.stunDuration *= attributes.StunDurationBoost;

            _timer.Start();
            gameObject.SetActive(true);
            _rigidbody.linearVelocity = _currentSpeed;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!_excludeTag.Contains(other.gameObject.tag) && _includeTag.Contains(other.gameObject.tag))
            {
                var enemy = other.gameObject.GetComponent<EnemyController>();
                if (enemy == null)
                {
                    Reset();
                    return;
                }

                enemy.Dependencies.TryGetFeature(out EnemyResource resource);
                resource.Attack(new HitInfo
                {
                    priority = _priority,
                    hit = _hit,
                    position = transform.position,
                    projectile = true,
                    success = true,
                    stunSuccess = true
                });
                Reset();
            }
            
            if(((1 << other.gameObject.layer) & _solid) != 0) Reset();
        }

        public void Reset()
        {
            if(_vfxPrefab != null) VFXPool.Instance.Get(_vfxPrefab, transform.position);
            _timer.Reset();
            gameObject.SetActive(false);
        }
    }
}