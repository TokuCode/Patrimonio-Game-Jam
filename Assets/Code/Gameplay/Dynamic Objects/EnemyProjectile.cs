using System.Collections.Generic;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyProjectile : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _direction;
        private SingleHit _currentHit;
        private Vector3 _currentSpeed;
        private Vector3 _initialScale;
        private List<string> _includeTag = new();
        private int _priority;
        private CountdownTimer _timer;
        private GameObject _sender;

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

        public void Init(Vector3 position, Vector3 direction, int priority, EnemyAttack attack, EnemyAttributes attributes, bool chain, GameObject sender)
        {
            _direction = direction.With(y:0).normalized;
            _currentHit = _hit;
            transform.localScale = _initialScale;
            transform.forward = direction;
            transform.position = position;
            _rigidbody.position = position;
            _currentSpeed = _direction.normalized * _speed;
            _includeTag = new List<string>(attack.Targeted);
            _priority = priority;
            _sender = sender;

            if (chain)
            {
                transform.localScale *= attack.MultiplierScale;
                _currentSpeed *= attack.MultiplierSpeed;
                _currentHit.damage *= attack.MultiplierDamage;
                _currentHit.knockback *= attack.MultiplierKnockback;
                attack.MultiplierReset();
            }

            _timer.Start();
            gameObject.SetActive(true);
            _rigidbody.linearVelocity = _currentSpeed;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject != _sender && _includeTag.Contains(other.gameObject.tag))
            {
                var enemy = other.gameObject.GetComponent<PlayerController>();
                if (enemy != null)
                {
                    if (enemy.Dependencies.TryGetFeature(out Resource resource))
                    {
                        resource.Attack(new HitInfo
                        {
                            priority = _priority,
                            hit = _hit,
                            position = transform.position,
                            direction = _direction,
                            projectile = true,
                            success = true,
                            stunSuccess = true
                        });
                    }
                }

                var ally = other.gameObject.GetComponent<EnemyController>();
                if (ally != null)
                {
                    if (ally.Dependencies.TryGetFeature(out EnemyResource enemyResource))
                    {
                        enemyResource.Attack(new HitInfo
                        {
                            priority = _priority,
                            hit = _hit,
                            position = transform.position,
                            direction = _direction,
                            projectile = true,
                            success = true,
                            stunSuccess = true
                        }); 
                    }
                }
                other.gameObject.GetComponent<DestroyableProp>()?.Attack(_hit.damage);
                
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