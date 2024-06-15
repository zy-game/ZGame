using ZGame.Networking;
using NotImplementedException = System.NotImplementedException;

namespace ZGame.Game.LockStep.System
{
    public class InputSystem : IUpdateSystem
    {
        public uint priority { get; }

        public virtual void DoAwake(World world, params object[] args)
        {
        }

        public virtual void DoUpdate(World world)
        {
            foreach (var inputed in world.AllOf<Inputed>())
            {
                using (MSG_UserInput userInput = RefPooled.Alloc<MSG_UserInput>())
                {
                    userInput.command = inputed.command.Clone();
                    AppCore.Network.Write(((TrueSyncWorld)world).cid, MSGPacket.Serialize((int)MSG_LockStep.CS_PLAYER_INPUT, userInput));
                }
            }
        }

        public virtual void Release()
        {
        }
    }
}