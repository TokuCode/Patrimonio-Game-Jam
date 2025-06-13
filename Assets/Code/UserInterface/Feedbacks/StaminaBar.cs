using DG.Tweening;
using Movement3D.Gameplay;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Movement3D.UserInterface
{
    public class StaminaBar : MonoBehaviour
    {
        private Camera _main;
        [SerializeField] private PlayerController player;
        private Resource resource;
        [SerializeField] private float _delay;
        [SerializeField] private float _showDuration;
        private CountdownTimer _showTimer;

        [Header("UI Elements")]
        [SerializeField] private RectTransform _staminaBar;
        [SerializeField] private Image _baseBar;
        [SerializeField] private Image _delayedBar;
        [SerializeField] private Transform _followPosition;
        
        [Header("Color")]
        [SerializeField] private Color _depletedColor;
        private Color _baseColor;

        private void Start()
        {
            _baseColor = _baseBar.color;
            _main = Camera.main;
            player.Dependencies.TryGetFeature(out resource);
            resource.OnStaminaChanged += OnUse;
            _showTimer = new CountdownTimer(_showDuration);
            _showTimer.OnTimerStop += () =>
            {
                _staminaBar.gameObject.SetActive(false);
            };
        }

        private void Update()
        {
            _showTimer.Tick(Time.deltaTime);
            BaseColor();
            SetBarPosition();
        }

        private void OnUse(Delta delta)
        {
            if(delta.delta == 0)return;
            
            _showTimer.Reset();
            _showTimer.Stop();
            _staminaBar.gameObject.SetActive(true);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_baseBar.DOFillAmount(delta.newRatio, .3f)).SetEase(Ease.InOutSine);
            sequence.AppendInterval(_delay);
            sequence.Append(_delayedBar.DOFillAmount(delta.newRatio, .3f)).SetEase(Ease.InOutSine);
            sequence.AppendCallback(() => _showTimer.Start());
            sequence.Play();
        }

        private void SetBarPosition()
        {
            Vector2 position = _main.WorldToScreenPoint(_followPosition.transform.position);
            _staminaBar.transform.position = position;
        }

        private void BaseColor()
        {
            _baseBar.color = resource.isDepleted ? _depletedColor : _baseColor;
        }
    }
}