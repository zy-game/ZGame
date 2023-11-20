using System;

namespace ZGame
{
    public sealed class Linked : Attribute
    {
        internal string path;
        internal int layer;

        public Linked(string path) : this(path, -1)
        {
        }

        public Linked(string path, int layer)
        {
            this.path = path;
            this.layer = layer;
        }
    }


}