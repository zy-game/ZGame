using System;

namespace ZGame.Editor
{
    public class SubPageSetting : Attribute
    {
        public string name;
        public Type parent;

        public SubPageSetting(string name)
        {
            this.name = name;
        }

        public SubPageSetting(string name, Type parent)
        {
            this.name = name;
            this.parent = parent;
        }
    }
}