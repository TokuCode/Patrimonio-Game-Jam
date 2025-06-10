using DG.Tweening;
using Movement3D.Helpers;
using UnityEngine;

namespace Movement3D.Gameplay
{
    public class RequestCenterPosition : IRequest<Vector3>
    {
        private Transform transform;
        private CapsuleCollider capsule;

        public RequestCenterPosition(Transform transform, CapsuleCollider capsule)
        {
            this.transform = transform;
            this.capsule = capsule;
        }

        public Vector3 Get()
        {
            return transform.position + Vector3.up * capsule.height / 2;
        }
    }

    public class LocalScaleHandler : IRequest<Vector3>, ICommand<Vector3>
    {
        private Transform transform;

        public LocalScaleHandler(Transform transform)
        {
            this.transform = transform;
        }

        public Vector3 Get()
        {
            return transform.localScale;
        }

        public void Execute(Vector3 args)
        {
            transform.localScale = args;
        }
    }

    public class HeightHandler : IRequest<float>, ICommand<float>
    {
        private CapsuleCollider capsule;

        public HeightHandler(CapsuleCollider capsule)
        {
            this.capsule = capsule;
        }

        public void Execute(float args)
        {
            capsule.height = args;
        }

        public float Get()
        {
            return capsule.height;
        }
    }

    public class RadiusHanlder : IRequest<float>, ICommand<float>
    {
        private CapsuleCollider capsule;

        public RadiusHanlder(CapsuleCollider capsule)
        {
            this.capsule = capsule;
        }

        public void Execute(float args)
        {
            capsule.radius = args;
        }

        public float Get()
        {
            return capsule.radius;
        }
    }

    public class VelocityHandlder : IRequest<Vector3>, ICommand<Vector3>
    {
        private Rigidbody rigidbody;

        public VelocityHandlder(Rigidbody rigidbody)
        {
            this.rigidbody = rigidbody;
        }

        public void Execute(Vector3 args)
        {
            rigidbody.linearVelocity = args;
        }

        public Vector3 Get()
        {
            return rigidbody.linearVelocity;
        }
    }

    public struct AddForceParams
    {
        public Vector3 force;
        public ForceMode forceMode;

        public AddForceParams(Vector3 force, ForceMode forceMode)
        {
            this.force = force;
            this.forceMode = forceMode;
        }

        public AddForceParams(Vector3 direction, float force, ForceMode forceMode)
        {
            this.force = direction.normalized * force;
            this.forceMode = forceMode;
        }

        public AddForceParams(float x = 0, float y = 0, float z = 0, ForceMode forceMode = ForceMode.Force)
        {
            force = new Vector3(x, y, z);
            this.forceMode = forceMode;
        }
    }

    public class AddForceCommand : ICommand<AddForceParams>
    {
        private Rigidbody rigidbody;

        public AddForceCommand(Rigidbody rigidbody)
        {
            this.rigidbody = rigidbody;
        }

        public void Execute(AddForceParams args)
        {
            rigidbody.AddForce(args.force, args.forceMode);
        }
    }

    public class AddRigidbodyPosition : ICommand<float>
    {
        private Rigidbody rigidbody;

        public AddRigidbodyPosition(Rigidbody rigidbody)
        {
            this.rigidbody = rigidbody;
        }

        public void Execute(float args)
        {
            rigidbody.position += Vector3.up * args;
        }
    }

    public class ForwardHandler : IRequest<Vector3>, ICommand<Vector3>
    {
        private Transform obj;

        public ForwardHandler(Transform obj)
        {
            this.obj = obj;
        }

        public Vector3 Get()
        {
            return obj.forward;
        }

        public void Execute(Vector3 args)
        {
            obj.forward = args;
        }
    }
    
    public class RightHandler : IRequest<Vector3>, ICommand<Vector3>
    {
        private Transform obj;

        public RightHandler(Transform obj)
        {
            this.obj = obj;
        }

        public Vector3 Get()
        {
            return obj.right;
        }

        public void Execute(Vector3 args)
        {
            obj.right = args;
        }
    }

    public class RotationHandler : IRequest<float>, ICommand<float>
    {
        private Transform obj;

        public RotationHandler(Transform obj)
        {
            this.obj = obj;
        }

        public float Get()
        {
            return obj.rotation.eulerAngles.y;
        }

        public void Execute(float args)
        {
            obj.rotation = Quaternion.Euler(0, args, 0);
        }
    }

    public class UseGravityHandler : ICommand<bool>, IRequest<bool>
    {
        private Rigidbody rigidbody;

        public UseGravityHandler(Rigidbody rigidbody)
        {
            this.rigidbody = rigidbody;
        }

        public bool Get()
        {
            return rigidbody.useGravity;
        }

        public void Execute(bool arg)
        {
            rigidbody.useGravity = arg;
        }
    }

    public class PositionHandler : IRequest<Vector3>, ICommand<Vector3>
    {
        private Transform obj;

        public PositionHandler(Transform obj)
        {
            this.obj = obj;
        }

        public Vector3 Get()
        {
            return obj.position;
        }

        public void Execute(Vector3 args)
        {
            obj.position = args;
        }
    }

    public class LocalPositionHandler : ICommand<Vector3>, IRequest<Vector3>
    {
        private Transform obj;

        public LocalPositionHandler(Transform obj)
        {
            this.obj = obj;
        }

        public void Execute(Vector3 args)
        {
            obj.localPosition = args;
        }

        public Vector3 Get()
        {
            return obj.localPosition;
        }
    }

    public class FullRotationHandler : IRequest<Quaternion>, ICommand<Quaternion>
    {
        private Transform obj;

        public FullRotationHandler(Transform obj)
        {
            this.obj = obj;
        }

        public void Execute(Quaternion args)
        {
            obj.rotation = args;
        }

        public Quaternion Get()
        {
            return obj.rotation;
        }
    }

    public struct SuckToTargetParams
    {
        public Vector3 position;
        public float duration;
    }

    public class SuckToTargetCommand : ICommand<SuckToTargetParams>
    {
        private Rigidbody rigidbody;
        private Ease ease;
        private AnimationCurve curve;
        
        private StopwatchTimer timer;
        private Vector3 target;
        private float duration;

        public SuckToTargetCommand(Rigidbody rigidbody, AnimationCurve curve)
        {
            this.rigidbody = rigidbody;
            this.curve = curve;
            timer = new StopwatchTimer();
            timer.OnTimerStop += () =>
            {
                this.rigidbody.isKinematic = false;
            };
        }

        public void Execute(SuckToTargetParams args)
        {
            timer.Reset();
            timer.Start();
            target = args.position;
            duration = args.duration;
            rigidbody.isKinematic = true;
        }

        public void Update(float deltaTime)
        {
            timer.Tick(deltaTime);
            
            if(timer.GetTime() >= duration) timer.Stop();
            
            SetPosition();
        }

        public void SetPosition()
        {
            if(!timer.IsRunning) return;
            
            float parameter = Mathf.Clamp01(timer.GetTime() / duration);
            float parameterInCurve = curve.Evaluate(parameter);
            rigidbody.MovePosition(Vector3.Lerp(rigidbody.position, target, parameterInCurve));
        }
    }

    public struct AlignCameraParams
    {
        public Vector3 direction;
        public float duration;
    }

    public class AlignCameraCommand : ICommand<AlignCameraParams>
    {
        private Transform _orientation;
        private Transform _obj;

        public AlignCameraCommand(Transform obj, Transform orientation)
        {
            _orientation = orientation;
            _obj = obj;
        }

        public void Execute(AlignCameraParams args)
        {
            _orientation.DOLookAt(_orientation.transform.position + args.direction, args.duration);
            _obj.DOLookAt(_obj.transform.position + args.direction, args.duration);
        }
    }

    public class ColliderCenterHandler : ICommand<float>, IRequest<float>
    {
        private CapsuleCollider capsule;

        public ColliderCenterHandler(CapsuleCollider capsule)
        {
            this.capsule = capsule;
        }

        public void Execute(float args)
        {
            capsule.center = Vector3.up * args;
        }

        public float Get()
        {
            return capsule.center.y;
        }
    }
}