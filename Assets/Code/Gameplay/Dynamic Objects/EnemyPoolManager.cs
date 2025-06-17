using System.Collections.Generic;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.Pool;

namespace Movement3D.Gameplay
{
    public class EnemyPoolManager : Singleton<EnemyPoolManager>
    {
        [SerializeField] private List<PoolConfigObject> PooledPrefabsList;

        HashSet<GameObject> m_Prefabs = new ();

        Dictionary<GameObject, ObjectPool<EnemyController>> m_PooledObjects = new ();

        private void Start()
        {
            // Registers all objects in PooledPrefabsList to the cache.
            foreach (var configObject in PooledPrefabsList)
            {
                RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
            }
        }

        private void OnDestroy()
        {
            foreach (var prefab in m_Prefabs)
            {
                m_PooledObjects[prefab].Clear();
            }
            m_PooledObjects.Clear();
            m_Prefabs.Clear();
        }

        public EnemyController Get(GameObject prefab, Vector3 position)
        {
            var go = m_PooledObjects[prefab].Get();

            go.Reactivate(new EnemySharedProperties
            {
                position = position,
                direction = Vector3.forward
            });

            return go;
        }

        /// <summary>
        /// Return an object to the pool (reset objects before returning).
        /// </summary>
        public void Return(EnemyController go, GameObject prefab)
        {
            m_PooledObjects[prefab].Release(go);
        }

        /// <summary>
        /// Builds up the cache for a prefab.
        /// </summary>
        void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
        {
            EnemyController CreateFunc()
            {
                return Instantiate(prefab).GetComponent<EnemyController>();
            }

            void ActionOnGet(EnemyController go)
            {
                go.gameObject.SetActive(true);
            }

            void ActionOnRelease(EnemyController go)
            {
                go.Deactivate(out var shared);
            }

            void ActionOnDestroy(EnemyController go)
            {
            }

            m_Prefabs.Add(prefab);

            // Create the pool
            m_PooledObjects[prefab] = new ObjectPool<EnemyController>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

            // Populate the pool
            var prewarmNetworkObjects = new List<EnemyController>();
            for (var i = 0; i < prewarmCount; i++)
            {
                prewarmNetworkObjects.Add(m_PooledObjects[prefab].Get());
            }
            foreach (var networkObject in prewarmNetworkObjects)
            {
                m_PooledObjects[prefab].Release(networkObject);
            }
        }
    }
}