using DG.Tweening;
using Movement3D.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Movement3D.UserInterface
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        private Resource resource;
        [SerializeField] private float _delay;

        [Header("UI Elements")]
        [SerializeField] private Image _baseBar;
        [SerializeField] private Image _delayedBar;
        [SerializeField] private Image _border;
        
        [Header("Color")]
        [SerializeField] private Color _highlight;
        private Color _baseColor;

        private void Start()
        {
            _baseColor = _border.color;
            player.Dependencies.TryGetFeature(out resource);
            resource.OnHealthChanged += OnDamage;
        }

        private void Update()
        {
            BorderColor();
        }

        private void OnDamage(Delta damage)
        {
            if(damage.delta == 0)return;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_baseBar.DOFillAmount(damage.newRatio, .3f)).SetEase(Ease.InOutSine);
            sequence.AppendInterval(_delay);
            sequence.Append(_delayedBar.DOFillAmount(damage.newRatio, .3f)).SetEase(Ease.InOutSine);
            sequence.Play();
        }

        private void BorderColor()
        {
            _border.color = resource.isInvincible ? _highlight : _baseColor;
        }
    }
}