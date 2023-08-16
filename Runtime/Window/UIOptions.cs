using System;

namespace ZEngine.Window
{
    public sealed class UIOptions : Attribute
    {
        public enum Layer
        {
            Low,
            Middle,
            Top,
        }

        internal Layer layer;
        internal string path;

        public UIOptions(string path, Layer layer)
        {
            this.path = path;
            this.layer = layer;
        }
    }
}