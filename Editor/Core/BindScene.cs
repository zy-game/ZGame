using System;

namespace ZGame.Editor
{
    public class BindScene : Attribute
    {
        public string name;
        public Type parent;

        public BindScene(string name)
        {
            this.name = name;
        }

        public BindScene(string name, Type parent)
        {
            this.name = name;
            this.parent = parent;
        }
    }
}