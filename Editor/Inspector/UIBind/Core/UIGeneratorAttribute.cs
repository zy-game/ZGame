using System;

namespace ZGame.Editor.UIBind
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UIGeneratorAttribute : Attribute
    {
        public Type componentType;

        public UIGeneratorAttribute(Type componentType)
        {
            this.componentType = componentType;
        }
    }
}