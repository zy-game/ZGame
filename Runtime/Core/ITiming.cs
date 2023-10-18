using System.Threading.Tasks;

namespace ZEngine
{
    public interface ITiming
    {
        ulong runtime { get; }
        ushort frameRate { get; }
        void Update();

        public static ITiming Default { get; internal set; }

        public static ITiming Create(int frameRate)
        {
            return new Timing(frameRate);
        }

        class Timing : ITiming
        {
            public ulong runtime { get; }
            public ushort frameRate { get; }

            public Timing(int frameRate)
            {
            }

            public void Update()
            {
            }
        }
    }
}