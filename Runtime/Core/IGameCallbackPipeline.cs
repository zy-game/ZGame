namespace ZGame
{
    public interface IGameCallbackPipeline<T> : IEntity
    {
        void Complation(T args);
        void Failur();
    }

    public interface IGameProgressCallbackPipeline<T> : IGameCallbackPipeline<T>
    {
        void Progress(float progress);
    }
}