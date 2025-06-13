using System;
using DG.Tweening;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class FloatingText : MonoBehaviour
    {
        private int number;
        [SerializeField] private float duration;
        [SerializeField] private float multiplier;
        private Vector3 startScale;
        
        public event Action<int> OnInit;
        public event Action OnFinish;

        private void Awake()
        {
            startScale = transform.localScale;
        }

        public void Init(int number)
        {
            this.number = number;
            OnInit?.Invoke(number);
            transform.DOScale(startScale, duration).From(startScale * multiplier).SetEase(Ease.OutSine).OnComplete(() => OnFinish?.Invoke());
        }
    }
}