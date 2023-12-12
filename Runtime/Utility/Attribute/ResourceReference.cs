using System;

namespace ZGame
{
    public sealed class ResourceReference : Attribute
    {
        public string path;
        public int layer;

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