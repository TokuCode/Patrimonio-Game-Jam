using DG.Tweening;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class EnemyInvoker 
    {
        public PositionHandler Position { get; }
        public RequestCenterPosition Center { get; }
        public HeightHandler Height { get; }
        public RadiusHanlder Radius { get; }
        public VelocityHandlder Velocity { get; }
        public ForwardHandler Forward { get; } 
        public RightHandler Right { get; }
        public KillCommand Kill { get; }
        public ReviveCommand Revive { get; } 
        
        public AddKnockbackAgentCommand Knockback { get; }
        public SetDestinationCommand Destination { get; }
        public ResetPathCommand ResetPath { get; }
        public SetAgentSpeedCommand AgentSpeed { get; }
        public SetAgentAccelerationCommand AgentAcceleration { get; }
        public SuckToTargetDOTweenEnemy SuckToTarget { get; }
        public EnemyStopCommand Stop { get; }
        
        public EnemyInvoker(EnemyController enemy)
        {
            var transform = enemy.transform;
            var rigidbody = enemy.Rigidbody;
            var collider = enemy.Collider;
            var agent = enemy.NavMeshAgent;
            var coroutine = enemy.Coroutine;

            Position = new (transform);
            Center = new(transform, collider);
            Height = new(collider);
            Radius = new(collider);
            Velocity = new(rigidbody);
            Forward = new(transform);
            Right = new(transform);
            Kill = new(enemy);
            Revive = new(enemy);
            Knockback = new(rigidbody, agent, coroutine);
            Destination = new(agent);
            ResetPath = new(agent, transform);
            AgentSpeed = new(agent);
            AgentAcceleration = new(agent);
            SuckToTarget = new(Ease.OutSine, transform, rigidbody, agent);
            Stop = new(agent, rigidbody);
        }
    }
}