namespace Movement3D.Gameplay
{
    public interface IFeature
    {
        void InitializeFeature(Controller controller);
        void UpdateFeature();
        void FixedUpdateFeature();
    }
}