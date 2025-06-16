namespace Movement3D.Gameplay
{
    public interface IFeature
    {
        void InitializeFeature(Controller controller);
        void UpdateFeature();
        void FixedUpdateFeature();

        void ResetFeature(ref SharedProperties shared);
        void ReInitializeFeature(Controller controller, SharedProperties shared);
    }
}