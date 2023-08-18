namespace ZEngine.Window
{
    public sealed class WindowCloseEventArgs : GameEventArgs<WindowCloseEventArgs>
    {
        public UIWindow window;
        public override void Release()
        {
            base.Release();
            window = null;
        }

        public static WindowCloseEventArgs Create(UIWindow window)
        {
            WindowCloseEventArgs windowCloseEventArgs = Engine.Class.Loader<WindowCloseEventArgs>();
            windowCloseEventArgs.window = window;
            return windowCloseEventArgs;
        }
    }
}