using Movement3D.Gameplay;
using TMPro;
using UnityEngine;

namespace Movement3D.UserInterface
{
    public class PlayerName : MonoBehaviour
    {
        [SerializeField] private Controller _player;
        TextMeshProUGUI _nameText;

        private void Awake()
        {
            _nameText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            
            if(_player is EnemyController enemy) _nameText.text = enemy.Name;
        }
    }
}