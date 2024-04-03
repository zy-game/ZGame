using System;

namespace ZGame
{
    public sealed class RefPath : Attribute
    {
        public string path;

        public RefPath(string path)
        {
            this.path = path;
        }
    }
}