namespace Movement3D.Helpers
{
    public interface IProcess<T>
    {
        void Apply(ref T @event);
    }
}
