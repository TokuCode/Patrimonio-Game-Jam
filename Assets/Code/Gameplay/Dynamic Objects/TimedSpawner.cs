using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class TimedSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private float _timeBetweenSpawns;
        private Metronome _metronome;

        private void Awake()
        {
            _metronome = new (_timeBetweenSpawns);
        }

        private void Update()
        {
            _metronome.Update(Time.deltaTime);

            while (_metronome.ShouldTick())
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            EnemyPoolManager.Instance.Get(_prefab, transform.position);
        }
    }
}