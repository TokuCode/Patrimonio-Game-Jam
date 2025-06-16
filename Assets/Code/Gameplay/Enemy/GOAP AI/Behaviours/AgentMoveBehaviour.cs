using System;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Movement3D.Gameplay
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private AgentBehaviour _goapAgent;
        private ITarget _currentTarget;
        [SerializeField] private float _minMoveDistance = .25f;
        
        private Vector3 _lastTargetPosition;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _goapAgent = GetComponent<AgentBehaviour>();
        }

        private void OnEnable()
        {
            _goapAgent.Events.OnTargetChanged += OnTargetChanged;
        }

        private void OnDisable()
        {
            _goapAgent.Events.OnTargetChanged -= OnTargetChanged;
        }

        private void OnTargetChanged(ITarget newTarget, bool inRange)
        {
            _currentTarget = newTarget;
            _lastTargetPosition = _currentTarget.Position;
            _navMeshAgent.SetDestination(_currentTarget.Position);
        }

        private void Update()
        {
            if (_currentTarget == null) return;

            if (Vector3.Distance(_currentTarget.Position, _lastTargetPosition) > _minMoveDistance)
            {
                _lastTargetPosition = _currentTarget.Position;
                _navMeshAgent.SetDestination(_currentTarget.Position);
            }
        }
    }
}