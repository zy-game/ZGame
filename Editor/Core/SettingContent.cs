using System;

namespace ZGame.Editor
{
    public sealed class SettingContent : Attribute
    {
        public Type type;

        public SettingContent(Type type)
        {
            this.type = type;
        }
    }
}