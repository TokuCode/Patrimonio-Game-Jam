namespace Movement3D.Gameplay
{
    public interface ICommand<T>
    {
        void Execute(T args);
    }

    public interface IRequest<T>
    {
        T Get();
    }
}