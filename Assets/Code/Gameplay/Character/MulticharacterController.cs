using System;
using System.Collections.Generic;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class MulticharacterController : Singleton<MulticharacterController>
    {
        [SerializeField] private List<PlayerController> _characters;
        private SharedProperties _lastSharedProperties;
        private PlayerController _currentPlayer;
        public PlayerController CurrentPlayer => _currentPlayer;
        private int _currentPlayerIndex;
        public event Action<PlayerController> OnSwitchCharacter;

        private void Start()
        {
            foreach (var player in _characters)
            {
                player.Deactivate(out _lastSharedProperties);
            }
            _currentPlayerIndex = 0;
            _currentPlayer = _characters[_currentPlayerIndex];
            _currentPlayer.Reactivate(_lastSharedProperties);
            OnSwitchCharacter?.Invoke(_currentPlayer);
        }

       public void Switch(int direction)
        {
            if(direction == 0) return;
            
            _currentPlayer.Deactivate(out _lastSharedProperties);
            _currentPlayerIndex = (_currentPlayerIndex + direction + _characters.Count) % _characters.Count;
            _currentPlayer = _characters[_currentPlayerIndex];
            _currentPlayer.Reactivate(_lastSharedProperties);
            
            OnSwitchCharacter?.Invoke(_currentPlayer);
        }
    }
}