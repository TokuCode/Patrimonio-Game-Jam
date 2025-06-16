using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Movement3D.Gameplay
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Waves Data")]
        [SerializeField] private List<WaveConfig> waveConfigs;

        [Header("Spawn Settings")]
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private float waveDelay;
        
        [Header("Status")]
        [SerializeField] private int currentWaveIndex;
        [SerializeField] private int enemiesAlive;

        private void Start()
        {
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            foreach (var config in waveConfigs)
            {
                for (int phaseIndex = 0; phaseIndex < config.phases.Count; phaseIndex++)
                {
                    WavePhase phase = config.phases[phaseIndex];

                    foreach (var enemyInfo in phase.enemies)
                    {
                        for (int i = 0; i < enemyInfo.count; i++)
                        {
                            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
                            GameObject enemy = ObjectPoolManager.Instance.Get(enemyInfo.enemy, spawnPoint.position, spawnPoint.rotation);
                            enemiesAlive++;
                            yield return new WaitForSeconds(phase.spawnDelay);
                        }
                    }

                    yield return new WaitUntil(() => enemiesAlive <= 0);
                }

                yield return new WaitForSeconds(config.postWaveRestTime);
            }
        }
    }
}