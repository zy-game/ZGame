using ZGame.Window;

namespace UI
{
    public interface UIWait : UIBase
    {
        public static void Show(string s)
        {
            Show(s, 0);
        }

        public static void Show(string s, float timeout)
        {
            UIManager.instance.Open<UIWait>(s, timeout);
        }

        public static void Hide()
        {
            UIManager.instance.Close<UIWait>();
        }
    }
}