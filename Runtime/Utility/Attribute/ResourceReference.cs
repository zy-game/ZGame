using System;

namespace ZGame
{
    public sealed class ResourceReference : Attribute
    {
        public string path;

        public ResourceReference(string path)
        {
            this.path = path;
        }
    }
}