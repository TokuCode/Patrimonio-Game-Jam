using System.Collections.Generic;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class Region : MonoBehaviour
    {
        [SerializeField] private List<EnemyData> _enemyData;
        private List<EnemyIdentity> _population = new();
        
        [Header("Settings")] 
        [SerializeField] private float _radius;
        [SerializeField] private int _aggresiveTokens;
        [SerializeField] private int _workersTokens;
        [SerializeField] private int _aggresiveTokensUnderAttack;
        [SerializeField] private int maxPopulation;
        public int currentWorkers {get; private set;}
        public int aggresiveTokensAvailable { get; private set; }
        public int currentPopulation {get; private set;}
    }
}