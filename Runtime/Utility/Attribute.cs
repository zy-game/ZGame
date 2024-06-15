using System;

namespace ZGame
{
    public class SelectorAttribute : Attribute
    {
        public bool isDrawLabel;
        public string ValuesGetter;

        public SelectorAttribute(string getter, bool isDrawLabel)
        {
            this.isDrawLabel = isDrawLabel;
            ValuesGetter = getter;
        }
    }

    public sealed class RefPath : Attribute
    {
        public string path;

        public RefPath(string path)
        {
            this.path = path;
        }
    }

    public sealed class UIBackup : Attribute
    {
    }

    public sealed class UIName : Attribute
    {
        public string name;
        public int code;

        public UIName(string name)
        {
            this.name = name;
        }

        public UIName(int code)
        {
            this.code = code;
        }
    }
}