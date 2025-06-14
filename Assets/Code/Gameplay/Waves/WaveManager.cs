using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Movement3D.Gameplay
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float waveDelay = 5f;
        [SerializeField] private List<GameObject> enemyPrefabs;
        
        private int enemiesAlive = 0;

        public void Start()
        {
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            int waveNumber = 1;

            while (true) // Infinite loop
            {
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(SpawnWaveProcedurally(waveNumber));

                while (enemiesAlive > 0)
                    yield return null;

                waveNumber++;
                yield return new WaitForSeconds(waveDelay);
            }
        }

        private IEnumerator SpawnWaveProcedurally(int waveNumber)
        {
            int totalEnemies = 5 + waveNumber * 2;
            float spawnDelay = Mathf.Max(0.5f, 2f - waveNumber * 0.05f); // get faster over time

            for (int i = 0; i < totalEnemies; i++)
            {
                GameObject prefabToSpawn = ChooseEnemyForWave(waveNumber);
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                
                //GameObject enemy = ObjectPoolManager.Instance.Get(prefabToSpawn, spawnPoint.position, Quaternion.identity);

                enemiesAlive++;
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        
        private GameObject ChooseEnemyForWave(int waveNumber)
        {
            int maxIndex = Mathf.Min(waveNumber / 5, enemyPrefabs.Count - 1);
            return enemyPrefabs[Random.Range(0, maxIndex + 1)];
        }
    }
}