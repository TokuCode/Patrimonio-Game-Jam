using DG.Tweening;
using Movement3D.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Movement3D.UserInterface
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private bool _isSpatial;
        [SerializeField] private EnemyController enemy;
        private EnemyResource resource;
        [SerializeField] private float _delay;

        [Header("UI Elements")]
        [SerializeField] private RectTransform _healthBar;
        [SerializeField] private Image _baseBar;
        [SerializeField] private Image _delayedBar;

        private void Start()
        {
            enemy.Dependencies.TryGetFeature(out resource);
            resource.OnHealthChanged += OnDamage;
            resource.OnDie += OnDeath;
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

        public void OnDeath()
        {
            _baseBar.fillAmount = 1;
            _delayedBar.fillAmount = 1;
            _healthBar.gameObject.SetActive(false);
        }
    }
}