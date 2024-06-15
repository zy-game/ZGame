using System.Collections.Generic;

namespace ZGame.UI
{
    class UIStack : IReference
    {
        private Stack<UIGroup> uiStack = new();

        public void Release()
        {
        }

        public void Push(UIGroup uiBase)
        {
        }

        public UIGroup Backup()
        {
            return default;
        }

        public static UIStack Create()
        {
            UIStack stack = RefPooled.Alloc<UIStack>();

            return stack;
        }
    }
}