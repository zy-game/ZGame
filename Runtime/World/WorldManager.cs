namespace ZEngine.World
{
    public class WorldManager : Single<WorldManager>
    {
        public IGameWorld current { get; private set; }

        public IGameWorld LoadGameWorld(IGameWorldOptions options)
        {
            return default;
        }

        public IGameWorld GetGameWorld(string worldName)
        {
            return default;
        }

        public void CloseWorld(string worldName)
        {
        }

        public ILogicSystemHandle LaunchLogicSystem<T>(params object[] paramsList)
        {
            return default;
        }
    }
}