using Movement3D.Gameplay;
using TMPro;
using UnityEngine;

namespace Movement3D.UserInterface
{
    public class PlayerName : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        TextMeshProUGUI _nameText;

        private void Awake()
        {
            _nameText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _nameText.text = _player.Name;
        }
    }
}