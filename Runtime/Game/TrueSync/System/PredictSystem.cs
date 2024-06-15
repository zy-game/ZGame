using ZGame;
using ZGame.Game;
using ZGame.Game.LockStep;
using ZGame.Networking;

namespace Game.TrueSync.System
{
    public class PredictSystem : IFixedUpdateSystem
    {
        public uint priority { get; }

        public virtual void DoAwake(World world, params object[] args)
        {
        }

        public virtual void DoFixedUpdate(World world)
        {
         
        }

        public virtual void Release()
        {
        }
    }
}