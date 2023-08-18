namespace ZEngine.Window
{
    public sealed class WindowDisableEventArgs : GameEventArgs<WindowDisableEventArgs>
    {
        public UIWindow window;
        public override void Release()
        {
            base.Release();
            window = null;
        }
        public static WindowDisableEventArgs Create(UIWindow window)
        {
            WindowDisableEventArgs windowDisableEventArgs = Engine.Class.Loader<WindowDisableEventArgs>();
            windowDisableEventArgs.window = window;
            return windowDisableEventArgs;
        }
    }
}