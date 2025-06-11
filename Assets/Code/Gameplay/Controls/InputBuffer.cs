using System;
using System.Collections.Generic;
using System.Linq;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class InputBuffer : Singleton<InputBuffer>
    {
        public const char Separator = '-';
        public const string AllKeyword = "all";
        private List<List<string>> _signals = new()
        {
            new() { "attack", "special" },
            new() { "neutral", "charge", "release" },
            new() { "ground", "air", "fall", "jump" }
        };
        public int layerCount => _signals.Count;
        
        [SerializeField] private string _currentSignal;
        public string Signal => _currentSignal;
        
        [SerializeField] private float persistence;
        private CountdownTimer _bufferPersistence;
        private List<string> _layers;

        protected override void Awake()
        {
            _bufferPersistence = new CountdownTimer(persistence);
            _bufferPersistence.OnTimerStop += () =>
            {
                _currentSignal = string.Empty;
            };
            
            SetDefaultLayers();
        }

        private void SetDefaultLayers()
        {
            _layers = new List<string>
            {
                _signals[0][0],
                _signals[1][0],
                _signals[2][0],
            };
        }

        private void Update()
        {
            _bufferPersistence.Tick(Time.deltaTime);
        }

        public void Input(string signal, int layer)
        {
            if (layer < 0 || layer >= _signals.Count) return;
            if (!_signals[layer].Contains(signal)) return;

            _layers[layer] = signal;
            
            BuildSignal();
            
            _bufferPersistence.Reset();
            _bufferPersistence.Start();
        }

        public void BuildSignal()
        {
            _currentSignal = string.Empty;
            for(int i = 0; i < _layers.Count; i++)
            {
                var unit = _layers[i];
                if (string.IsNullOrWhiteSpace(unit)) unit = _signals[i][0];
                _currentSignal = String.Concat(_currentSignal, unit);
                if(i < _layers.Count - 1) _currentSignal = String.Concat(_currentSignal, Separator);
            }
        }
    }
}