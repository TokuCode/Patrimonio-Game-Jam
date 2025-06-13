using System.Collections.Generic;
using Movement3D.Helpers;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

namespace Movement3D.Gameplay
{
    public class VFXPool : Singleton<VFXPool>
    {
        [SerializeField] private List<PoolConfigObject> PooledPrefabsList;

        HashSet<GameObject> m_Prefabs = new ();

        Dictionary<GameObject, ObjectPool<VisualEffect>> m_PooledObjects = new ();

        private void Start()
        {
            // Registers all objects in PooledPrefabsList to the cache.
            foreach (var configObject in PooledPrefabsList)
            {
                RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
            }
        }

        public VisualEffect Get(GameObject prefab, Vector3 position)
        {
            var vfx = m_PooledObjects[prefab].Get();
            vfx.transform.position = position;
            vfx.gameObject.SetActive(true);

            return vfx;
        }

        /// <summary>
        /// Return an object to the pool (reset objects before returning).
        /// </summary>
        public void Return(VisualEffect vfx, GameObject prefab)
        {
            m_PooledObjects[prefab].Release(vfx);
        }

        /// <summary>
        /// Builds up the cache for a prefab.
        /// </summary>
        void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
        {
            VisualEffect CreateFunc()
            {
                return Instantiate(prefab).GetComponent<VisualEffect>();
            }

            void ActionOnGet(VisualEffect vfx)
            {
                vfx.gameObject.GetComponent<VfxController>().Set();
            }

            void ActionOnRelease(VisualEffect vfx)
            {
                if (vfx == null) return;
                
                vfx.gameObject.SetActive(false);
            }

            void ActionOnDestroy(VisualEffect vfx)
            {
                Destroy(vfx.gameObject);
            }

            m_Prefabs.Add(prefab);

            // Create the pool
            m_PooledObjects[prefab] = new ObjectPool<VisualEffect>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

            // Populate the pool
            var prewarmNetworkObjects = new List<VisualEffect>();
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