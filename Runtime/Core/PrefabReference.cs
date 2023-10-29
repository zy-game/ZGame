using System;

namespace ZGame
{
    public sealed class PrefabReference : Attribute
    {
        internal string path;
        internal int layer;

        public PrefabReference(string path) : this(path, -1)
        {
        }

        public PrefabReference(string path, int layer)
        {
            this.path = path;
            this.layer = layer;
        }
    }
}