using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FixMath.NET;
using ZGame.Networking;

namespace ZGame.Game.LockStep
{
    public sealed class MSG_UserInput : MSGRoom
    {
        public Command command;

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            command = RefPooled.Alloc<Command>();
            command.Read(reader);
        }

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            command.Write(writer);
        }

        public override void Release()
        {
            base.Release();
            RefPooled.Free(command);
        }
    }
}