namespace Movement3D.Gameplay
{
    public interface ICommand<T> where T : unmanaged
    {
        void Execute(T args);
    }

    public interface IRequest<T> where T : unmanaged
    {
        T Get();
    }
}