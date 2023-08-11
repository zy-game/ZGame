namespace ZEngine.Window
{
    [UIOptions("Resources/Toast", UIOptions.Layer.Pop)]
    public class UI_Toast : UIWindow
    {
        public UI_Toast SetToast(string text)
        {
            return this;
        }
    }
}