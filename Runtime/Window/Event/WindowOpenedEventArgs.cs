namespace ZEngine.Window
{
    public sealed class WindowOpenedEventArgs : GameEventArgs<WindowOpenedEventArgs>
    {
        public UIWindow window;

        public override void Release()
        {
            base.Release();
            window = null;
        }

        public static WindowOpenedEventArgs Create(UIWindow window)
        {
            WindowOpenedEventArgs windowOpenedEventArgs = Engine.Class.Loader<WindowOpenedEventArgs>();
            windowOpenedEventArgs.window = window;
            return windowOpenedEventArgs;
        }
    }
}