using System;
using UnityEngine.Serialization;

namespace ZGame.UI
{
    /// <summary>
    /// 界面默认层级
    /// </summary>
    public enum UILAYER : byte
    {
        BACKGROUND = 10,
        BOTTOM = 30,
        MIDDLE = 50,
        TOP = 60,
        LOADING = 80,
        WAITING = 90,
        MESSAGE = 100,
        TIPS = 110,
    }

    /// <summary>
    /// 界面显示方式
    /// </summary>
    public enum LOADTYPE : byte
    {
        ADDITION,
        OVERLAP,
    }

    public sealed class UIOptions : Attribute
    {
        public int layer;
        public Type parent;
        public LOADTYPE loadtype;

        public UIOptions(int layer, LOADTYPE loadtype = LOADTYPE.OVERLAP)
        {
            this.layer = layer;
            this.loadtype = loadtype;
        }

        public UIOptions(UILAYER layer, LOADTYPE loadtype = LOADTYPE.OVERLAP) : this((int)layer, loadtype)
        {
        }


        public UIOptions(Type parent, LOADTYPE loadtype = LOADTYPE.OVERLAP)
        {
            this.layer = -1;
            this.parent = parent;
            this.loadtype = loadtype;
        }
    }
}