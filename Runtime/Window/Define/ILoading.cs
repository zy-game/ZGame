namespace ZGame.Window
{
    public interface ILoading : IGameWindow
    {
        void SetProgress(float progress);

        public static ILoading Create()
        {
            return default;
        }
    }
}