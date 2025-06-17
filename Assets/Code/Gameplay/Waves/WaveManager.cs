using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Movement3D.Gameplay
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Waves Data")] 
        [SerializeField] private List<WaveConfig> waveConfigs;
        //[SerializeField] private List<GameObject> enemiesType;

        [Header("Spawn Settings")] 
        [SerializeField] private Transform[] enemySpawnPoints;

        [Header("Status")] 
        [SerializeField] private int currentWaveIndex;
        [SerializeField] private int enemiesAlive;

        private void Start()
        {
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            // Run configured waves
            for (int i = 0; i < waveConfigs.Count; i++)
                yield return RunWave(waveConfigs[i]);
            
            // Start procedural waves 
            /*int proceduralWaveIndex = 0;
            while (true)
            {
                WaveConfig generatedWave = GenerateProceduralWave(proceduralWaveIndex);
                yield return RunWave(generatedWave);
                proceduralWaveIndex++;
            }*/
        }

        private IEnumerator RunWave(WaveConfig waveConfig)
        {
            for (int phaseIndex = 0; phaseIndex < waveConfig.phases.Count; phaseIndex++)
            {
                WavePhase wavePhase = waveConfig.phases[phaseIndex];

                foreach (var enemyInfo in wavePhase.enemies)
                {
                    for (int i = 0; i < enemyInfo.count; i++)
                    {
                        Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
                        GameObject enemy = ObjectPoolManager.Instance.Get(enemyInfo.enemy, spawnPoint.position, spawnPoint.rotation);
                        enemiesAlive++;
                        yield return new WaitForSeconds(wavePhase.spawnDelay);
                    }
                }

                yield return new WaitUntil(() => enemiesAlive <= 0);
            }
            
            yield return new WaitForSeconds(waveConfig.postWaveRestTime);
        }

        /*private WaveConfig GenerateProceduralWave(int waveIndex)
        {
            WaveConfig wave = new WaveConfig
            {
                postWaveRestTime = 10f,
                phases = new List<WavePhase>()
            };

            int phaseCount = 5;
            for (int i = 0; i < phaseCount; i++)
            {
                WavePhase phase = new WavePhase
                {
                    spawnDelay = Mathf.Max(0.3f, 1.5f - (waveIndex * 0.05f)),
                    enemies = new List<EnemySpawnInfo>()
                };

                int enemyTypesThisPhase = Random.Range(1, 3);
                for (int j = 0; j < enemyTypesThisPhase; j++)
                {
                    EnemySpawnInfo enemyInfo = new EnemySpawnInfo
                    {
                        enemy = GetEnemyType(waveIndex),
                        count = Mathf.Clamp(2 + waveIndex, 2, 20)
                    };

                    phase.enemies.Add(enemyInfo);
                }

                wave.phases.Add(phase);
            }

            return wave;
        }
        
        private GameObject GetEnemyType(int waveIndex)
        {
            int index = Mathf.Min(waveIndex, enemiesType.Count - 1);
            return enemiesType[Random.Range(0, index + 1)];
        }*/
    }
}