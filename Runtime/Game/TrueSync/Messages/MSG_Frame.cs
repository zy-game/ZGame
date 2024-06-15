using System.Collections.Generic;
using System.IO;
using System.Text;
using FixMath.NET;
using Newtonsoft.Json;
using ZGame.Networking;

namespace ZGame.Game.LockStep
{
    /// <summary>
    /// 帧同步信息
    /// </summary>
    public class MSG_Frame : MSGRoom
    {
        public FrameData frame;

        public override void Release()
        {
            base.Release();
            RefPooled.Free(frame);
        }

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            frame = FrameData.Create(reader.ReadInt64(), new List<Command>());
            int count = reader.ReadInt32(); //读取玩家数量
            for (int i = 0; i < count; i++)
            {
                Command command = RefPooled.Alloc<Command>();
                command.Read(reader);
                frame.Set(command.uid, command);
            }
        }

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            writer.Write(frame.frameID); //写入帧编号
            writer.Write(frame.commands.Count); //写入玩家数量
            foreach (var user in frame.commands)
            {
                user.Write(writer);
            }
        }
    }
}