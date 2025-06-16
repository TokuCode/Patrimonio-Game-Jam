using UnityEngine;
using System.Collections.Generic;

namespace Movement3D.Gameplay
{
    [CreateAssetMenu(fileName = "New WaveConfig", menuName = "WaveConfig")]
    public class WaveConfig : ScriptableObject
    {
        public List<WavePhase> phases;
        public float postWaveRestTime;
    }

    [System.Serializable]
    public struct WavePhase
    {
        public List<EnemySpawnInfo> enemies;
        public float spawnDelay;
    }

    [System.Serializable]
    public struct EnemySpawnInfo
    {
        public GameObject enemy;
        public int count;
    }
}