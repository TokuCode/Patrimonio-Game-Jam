using System;
using Movement3D.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Movement3D.Gameplay
{
    public class DestroyableProp : MonoBehaviour
    {
        [SerializeField] private float _maxHealth;
        private float _currentHealth;
        
        [Header("Shake Effect")]
        [SerializeField] private float _shakeAmount;
        [SerializeField] private float _shakeSpeed;
        [SerializeField] private float _shakeDuration;
        private CountdownTimer _shakeTimer;
        private bool _shaking;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _shakeTimer = new CountdownTimer(_shakeDuration);
            _shakeTimer.OnTimerStop += () => _shaking = false;
        }

        public void Attack(float damage)
        {
            if(damage <= 0 || _shaking) return;
            
            _currentHealth -= damage;
            if(_currentHealth <= 0) gameObject.SetActive(false);
            else
            {
                _shaking = true;
                _shakeTimer.Start();
            }
        }

        private void Update()
        {
            _shakeTimer.Tick(Time.deltaTime);
            Shake();
        }

        private void Shake()
        {
            if (!_shaking) return;

            var randVec = Random.insideUnitCircle;
            
            transform.position += new Vector3(randVec.x, 0, randVec.y).normalized * (Mathf.Sin(Time.time * _shakeSpeed) * _shakeAmount * Time.deltaTime);
        }
    }
}