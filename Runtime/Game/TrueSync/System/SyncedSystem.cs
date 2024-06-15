using ZGame.Game;

namespace Game.TrueSync.System
{
    public class SyncedSystem : IFixedUpdateSystem
    {
        public uint priority { get; }

        public void DoAwake(World world, params object[] args)
        {
        }

        public void DoFixedUpdate(World world)
        {
        }

        public void Release()
        {
        }
    }
}