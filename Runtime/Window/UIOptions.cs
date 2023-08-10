using System;

namespace ZEngine.Window
{
    public sealed class UIOptions : Attribute
    {
        public enum Layer
        {
            Background,
            Low,
            Middle,
            Top,
            Pop,
        }

        internal int layer;
        internal string path;

        public UIOptions(string path, Layer layer) : this(path, (int)layer)
        {
        }

        public UIOptions(string path, int layer)
        {
            this.path = path;
            this.layer = layer;
        }
    }
}