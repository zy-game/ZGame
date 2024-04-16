using System.Collections.Generic;
using System.IO;
using System.Text;
using ZGame.Networking;

namespace ZGame.Game
{
    /// <summary>
    /// 帧同步信息
    /// </summary>
    public class FrameData : RoomMsg
    {
        /// <summary>
        /// 帧编号
        /// </summary>
        public int frame;

        /// <summary>
        /// 当前帧所有玩家的输入
        /// </summary>
        public List<UserInput> userList = new();

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            frame = reader.ReadInt32(); //读取帧编号
            int count = reader.ReadInt32(); //读取玩家数量
            for (int i = 0; i < count; i++)
            {
                UserInput user = RefPooled.Spawner<UserInput>();
                user.uid = reader.ReadUInt32(); //读取玩家编号
                int fpCount = reader.ReadInt32(); //读取玩家输入数量
                for (int j = 0; j < fpCount; j++)
                {
                    byte key = reader.ReadByte(); //读取玩家输入类型
                    long value = reader.ReadInt64(); //读取玩家输入值
                    user.Set(key, value); //设置玩家输入
                }

                SetUserInputData(user);
            }
        }

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            writer.Write(frame); //写入帧编号
            writer.Write(userList.Count); //写入玩家数量
            foreach (var user in userList)
            {
                writer.Write(user.uid); //写入玩家编号
                writer.Write(user.inputs.Count); //写入玩家输入数量
                foreach (var item in user.inputs)
                {
                    writer.Write(item.Key); //写入玩家输入类型
                    writer.Write(item.Value); //写入玩家输入值
                }
            }
        }

        /// <summary>
        /// 合并帧数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FrameData Merge(params FrameData[] data)
        {
            FrameData frameData = RefPooled.Spawner<FrameData>();
            foreach (FrameData sync in data)
            {
                foreach (UserInput inputData in sync.userList)
                {
                    if (frameData.Contains(inputData.uid))
                    {
                        frameData.RemoveUserInputData(inputData.uid);
                    }

                    frameData.SetUserInputData(inputData);
                }
            }

            return frameData;
        }

        /// <summary>
        /// 添加玩家输入数据
        /// </summary>
        /// <param name="data"></param>
        public void SetUserInputData(UserInput data)
        {
            userList.Add(data);
        }

        /// <summary>
        /// 获取当前玩家的输入数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserInput GetUserInputData(uint id)
        {
            UserInput userInput = userList.Find(x => x.uid == id);
            if (userInput is null)
            {
                userInput = RefPooled.Spawner<UserInput>();
            }

            return userInput;
        }

        /// <summary>
        /// 移除玩家的输入数据
        /// </summary>
        /// <param name="id"></param>
        public void RemoveUserInputData(uint id)
        {
            userList.RemoveAll(x => x.uid == id);
        }

        /// <summary>
        /// 是否存在玩家的输入数据
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool Contains(uint uid)
        {
            return userList.Exists(x => x.uid == uid);
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("frame:" + frame);
            foreach (UserInput inputData in userList)
            {
                sb.Append("{" + inputData.ToString() + "}");
            }

            return sb.ToString();
        }

        public void Release()
        {
            frame = 0;
            userList.Clear();
        }
    }
}