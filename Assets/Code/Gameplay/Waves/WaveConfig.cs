using UnityEngine;
using System.Collections.Generic;

namespace Movement3D.Gameplay
{
    [System.Serializable]
    public class WaveConfig
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