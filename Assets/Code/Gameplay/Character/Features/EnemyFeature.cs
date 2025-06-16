namespace Movement3D.Gameplay
{
    public class EnemyFeature : Feature
    {
        protected EnemyInvoker _invoker;
        
        public virtual void InitializeFeature(Controller controller)
        {
            base.InitializeFeature(controller);
            if (controller is EnemyController enemy)
            {
                _invoker = enemy.Invoker;
            }
        } 
    } 
}