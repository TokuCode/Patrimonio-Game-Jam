using System;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.VFX;

namespace Movement3D.Gameplay
{
    public class VfxController : MonoBehaviour
    {
        private VisualEffect _vfx;
        [SerializeField] private float _lifeSpan;
        private CountdownTimer _timer;

        public void Set()
        {
            _timer.Start();
            SetPosition();
        }

        private void Awake()
        {
            _vfx = GetComponent<VisualEffect>();
            _timer = new CountdownTimer(_lifeSpan);
            _timer.OnTimerStop += () => { gameObject.SetActive(false); };
        }

        private void Update()
        {
            _timer.Tick(Time.deltaTime);
            SetPosition();
        }

        private void SetPosition()
        { 
           _vfx.SetVector3("Position", transform.position);   
        }
    }
}