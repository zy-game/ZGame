using System.Linq;
using ZGame;
using ZGame.Game;
using ZGame.Game.LockStep;

namespace SHDLD.System
{
    public class PhysiceSystem : IUpdateSystem
    {
        public uint priority { get; }

        public virtual void DoAwake(World world, params object[] args)
        {
        }

        public virtual void DoUpdate(World world)
        {
            foreach (var synced in world.AllOf<Synced>())
            {
                Command command = synced.command;
                if (command is null)
                {
                    AppCore.Logger.Log("command is null:" + synced.uid);
                    continue;
                }

                var x = command.Get((byte)InputID.Horizontal);
                var y = command.Get((byte)InputID.Vertical);
                var input = new BEPUutilities.Vector3(x, 0, y);
                synced.entity.LinearVelocity = input;
            }
        }

        public virtual void Release()
        {
        }
    }
}