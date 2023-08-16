namespace ZEngine.World
{
    public class WorldManager : Single<WorldManager>
    {
        public IGameWorld current { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            Engine.Console.Log("关闭游戏管理器");
        }

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

        public ILogicSystemExecuteHandle LaunchLogicSystem<T>(params object[] paramsList)
        {
            return default;
        }
    }
}