using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Movement3D.Gameplay
{
    public class NavMeshDistanceObserver : MonoBehaviour, IAgentDistanceObserver
    {
        private NavMeshAgent _navMeshAgent;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            GetComponent<AgentBehaviour>().DistanceObserver = this;
        }

        public float GetDistance(IMonoAgent agent, ITarget target, IComponentReference reference)
        {
            if (!_navMeshAgent.hasPath) return 0f;
            
            return _navMeshAgent.remainingDistance;
        }
    }
}