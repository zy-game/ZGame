namespace ZEngine.Window
{
    public sealed class WindowEnableEventArgs : GameEventArgs<WindowEnableEventArgs>
    {
        public UIWindow window;

        public override void Release()
        {
            base.Release();
            window = null;
        }

        public static WindowEnableEventArgs Create(UIWindow window)
        {
            WindowEnableEventArgs windowEnableEventArgs = Engine.Class.Loader<WindowEnableEventArgs>();
            windowEnableEventArgs.window = window;
            return windowEnableEventArgs;
        }
    }
}