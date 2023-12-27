using ZGame.Window;

namespace UI
{
    public interface IWait : UIBase
    {
        void Setup(string title, float time);

        public static void Show(string s)
        {
            Show(s, 0);
        }

        public static void Show(string s, float timeout)
        {
            IWait wait = UIManager.instance.TryOpenWindow<IWait>();
            if (wait != null)
            {
                wait.Setup(s, timeout);
            }
        }


        public static void Hide()
        {
            UIManager.instance.Close<IWait>();
        }
    }
}