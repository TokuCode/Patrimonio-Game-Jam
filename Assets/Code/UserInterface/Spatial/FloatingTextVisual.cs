using Movement3D.Gameplay;
using TMPro;
using UnityEngine;

namespace Movement3D.UserInterface
{
    public class FloatingTextVisual : MonoBehaviour
    {
        private FloatingText _floatingText;
        private TextMeshProUGUI _textMesh;

        private void Start()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
            _textMesh.text = string.Empty;
            _floatingText = GetComponent<FloatingText>();
            if (_floatingText != null)
            {
                _floatingText.OnInit += UpdateText;
                _floatingText.OnFinish += ClearText;
            }
        }

        private void OnDisable()
        {
            if (_floatingText != null)
            {
                _floatingText.OnInit -= UpdateText;
                _floatingText.OnFinish -= ClearText;
            }
        }

        private void UpdateText(int number)
        {
            _textMesh.text = number.ToString();
        }
        
        private void ClearText() => _textMesh.text = string.Empty;
    }
}