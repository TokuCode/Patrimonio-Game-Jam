using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyIdentity : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;
        public EnemyData Data => _data;

        [SerializeField] private Region _nativeRegion;
        [SerializeField] private Region _currentRegion;
    }
}