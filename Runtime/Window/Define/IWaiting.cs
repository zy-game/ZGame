namespace ZGame.Window
{
    public interface IWaiting : IGameWindow
    {
        public static IWaiting Create(float timeout = 0)
        {
            return default;
        }
    }
}