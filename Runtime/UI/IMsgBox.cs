using System;
using ZGame.Window;

namespace UI
{
    public interface IMsgBox : UIBase
    {
        public static IMsgBox Show(string title, string content, Action onYes, Action onNo)
        {
            return default;
        }


        public static IMsgBox Show(string content, Action onYes, Action onNo)
        {
            return default;
        }

        public static IMsgBox Show(string content, Action onYes)
        {
            return default;
        }
    }
}