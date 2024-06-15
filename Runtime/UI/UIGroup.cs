using System.Collections.Generic;

namespace ZGame.UI
{
    /// <summary>
    /// UI组，将几个UI组合成一个组
    /// </summary>
    class UIGroup : IReference
    {
        private List<UIBase> uiList = new();

        public void Add(UIBase uiBase)
        {
            uiList.Add(uiBase);
        }

        public void Remove(UIBase uiBase)
        {
            uiList.Remove(uiBase);
        }

        public void Enable(params object[] args)
        {
            uiList.ForEach(x => x.Enable());
        }

        public void Disable()
        {
            uiList.ForEach(x => x.Disable());
        }

        public void Release()
        {
            uiList.ForEach(x => RefPooled.Free(x));
        }
    }
}