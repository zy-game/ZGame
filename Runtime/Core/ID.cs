using System;

namespace ZGame
{
    public sealed class ID
    {
        public static string New()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}