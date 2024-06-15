using System;
using FixMath.NET;

namespace ZGame
{
    public partial class Random
    {
        public ulong randSeed = 1;

        public Random(uint seed = 17)
        {
            randSeed = seed;
        }

        public uint Next()
        {
            randSeed = randSeed * 1103515245 + 36153;
            return (uint)(randSeed / 65536);
        }

        public Fix64 Next(Fix64 max)
        {
            return (int)(Next() % max);
        }

        // range:[min~(max-1)]
        public Fix64 Range(Fix64 min, Fix64 max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException("minValue",
                    string.Format("'{0}' cannot be greater than {1}.", min, max));

            Fix64 num = max - min;
            return this.Next(num) + min;
        }
    }

    public static class LRandom
    {
        private static Random _i = new Random(3274);

        public static uint Next()
        {
            return _i.Next();
        }

        public static Fix64 Next(Fix64 max)
        {
            return _i.Next(max);
        }

        public static Fix64 Range(Fix64 min, Fix64 max)
        {
            return _i.Range(min, max);
        }
    }
}