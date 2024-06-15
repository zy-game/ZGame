using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.Events;

namespace ZGame.UI
{
    public class LoopScrollCellView : EnhancedScrollerCellView
    {
        private object _userData;
        public float height;

        public object userData
        {
            get { return _userData; }
        }

        public void SetData(object args)
        {
            _userData = args;
        }
    }
}