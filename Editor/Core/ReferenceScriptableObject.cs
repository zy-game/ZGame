using System;

namespace ZGame.Editor
{
    public sealed class ReferenceScriptableObject : Attribute
    {
        public Type type;

        public ReferenceScriptableObject(Type type)
        {
            this.type = type;
        }
    }
}