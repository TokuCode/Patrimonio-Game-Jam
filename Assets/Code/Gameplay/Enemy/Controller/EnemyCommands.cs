using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace Movement3D.Gameplay
{
    public class AddKnockbackAgentCommand : ICommand<Vector3>
    {
        private const float StillThreshold = 1;
        
        private Rigidbody _rb;
        private NavMeshAgent _agent;
        private CoroutineRunner _coroutine;

        public AddKnockbackAgentCommand(Rigidbody rb, NavMeshAgent agent, CoroutineRunner coroutine)
        {
            _rb = rb;
            _agent = agent;
            _coroutine = coroutine;
        }

        public void Execute(Vector3 args)
        {
            Add(args);
        }

        public void Add(Vector3 args)
        {
            _agent.enabled = false;
            _rb.isKinematic = false;
            _rb.useGravity = true;
            
            _rb.AddForce(args, ForceMode.VelocityChange);
        }
    }

    public class SetDestinationCommand : ICommand<Vector3>
    {
        private NavMeshAgent _agent;

        public SetDestinationCommand(NavMeshAgent agent)
        {
            _agent = agent;
        }

        public void Execute(Vector3 args)
        {
            if (!_agent.enabled) return;
            _agent.SetDestination(args);
        }
    }

    public class SetAgentSpeedCommand : ICommand<float>
    {
        private NavMeshAgent _agent;

        public SetAgentSpeedCommand(NavMeshAgent agent)
        {
            _agent = agent;
        }

        public void Execute(float args)
        {
            _agent.speed = args;
        }
    }
   
    public class SetAgentAccelerationCommand : ICommand<float>
    {
        private NavMeshAgent _agent;

        public SetAgentAccelerationCommand(NavMeshAgent agent)
        {
            _agent = agent;
        }

        public void Execute(float args)
        {
            _agent.acceleration = args;
        }
    } 
    
    public class SuckToTargetDOTweenEnemy : ICommand<SuckToTargetParams>
    {
        private Rigidbody rigidbody;
        private NavMeshAgent agent;
        private Transform obj;
        private Ease ease;

        public SuckToTargetDOTweenEnemy(Ease ease, Transform obj, Rigidbody rigidbody, NavMeshAgent agent)
        {
            this.rigidbody = rigidbody;
            this.obj = obj;
            this.agent = agent;
            this.ease = ease;
        }

        public void Execute(SuckToTargetParams args)
        {
            bool isKinematic = rigidbody.isKinematic;
            bool useGravity = rigidbody.useGravity;
            bool agentEnabled = agent.enabled;
            
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            agent.enabled = false;
            obj.DOMove(args.position, args.duration).SetEase(ease).OnComplete(() =>
            {
                rigidbody.isKinematic = isKinematic;
                agent.enabled = agentEnabled;
                rigidbody.useGravity = useGravity;
            });
        }
    }

    public class EnemyStopCommand : ICommand<bool>
    {
        private NavMeshAgent _agent;
        private Rigidbody _rigidbody;

        public EnemyStopCommand(NavMeshAgent agent, Rigidbody rigidbody)
        {
            _agent = agent;
            _rigidbody = rigidbody;
        }

        public void Execute(bool args)
        {
            _agent.enabled = !args;
            _rigidbody.isKinematic = !args;
            _rigidbody.useGravity = args;
        }
    }
}