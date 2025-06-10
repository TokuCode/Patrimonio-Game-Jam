using System.Collections.Generic;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class InputBuffer : Singleton<InputBuffer>
    {
        [SerializeField] private List<string> _signals = new()
        {
            "light",
            "heavy"
        };
        private string _currentSignal;
        public string Signal => _currentSignal;
        [SerializeField] private float persistence;
        private CountdownTimer _bufferPersistence;

        protected override void Awake()
        {
            _bufferPersistence = new CountdownTimer(persistence);
            _bufferPersistence.OnTimerStop += () =>
            {
                _currentSignal = string.Empty;
            };
        }

        private void Update()
        {
            _bufferPersistence.Tick(Time.deltaTime);
        }

        public void Input(string signal)
        {
            if (!_signals.Contains(signal)) return;
            
            _currentSignal = signal;
            _bufferPersistence.Reset();
            _bufferPersistence.Start();
        }
    }
}