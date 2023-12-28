using ZGame.Window;

namespace UI
{
    public interface IWait : UIBase
    {
        public static void Show(string s)
        {
            Show(s, 0);
        }

        public static void Show(string s, float timeout)
        {
            UIManager.instance.Open<IWait>(s, timeout);
        }

        public static void Hide()
        {
            UIManager.instance.Close<IWait>();
        }
    }
}