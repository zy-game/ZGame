using System;

namespace ZGame.Editor
{
    public sealed class Options : Attribute
    {
        public Type type;

        public Options(Type type)
        {
            this.type = type;
        }
    }
}