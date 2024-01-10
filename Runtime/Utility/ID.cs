using System;

namespace ZGame
{
    public class ID
    {
        public static string GetString()
        {
            return Guid.NewGuid().ToString().Replace("-", String.Empty);
        }

        public static string GetString(int length)
        {
            return Guid.NewGuid().ToString().Replace("-", String.Empty).Substring(0, length);
        }

        public static uint GetUint()
        {
            return (uint)DateTime.Now.Ticks;
        }

        public static uint GetUint(int length)
        {
            return (uint)DateTime.Now.Ticks % (uint)Math.Pow(10, length);
        }
    }
}