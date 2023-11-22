using System;

namespace ZGame
{
    public sealed class ResourceReference : Attribute
    {
        internal string path;
        internal int layer;

        public ResourceReference(string path) : this(path, -1)
        {
        }

        public ResourceReference(string path, int layer)
        {
            this.path = path;
            this.layer = layer;
        }
    }


}