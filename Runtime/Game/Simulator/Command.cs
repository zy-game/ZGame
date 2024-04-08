using System.Collections.Generic;
using System.IO;
using TrueSync;
using ZGame.Networking;

namespace ZGame.Game
{
    public enum SyncCode : byte
    {
        JOIN = 100,
        LEAVE = 101,
        READY = 102,
        SYNC = 103,
    }

    public class Ready : IMessaged
    {
        public uint uid;

        public static byte[] Encode(Ready data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.uid);
                    return ms.ToArray();
                }
            }
        }

        public static Ready Decode(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Ready ready = GameFrameworkFactory.Spawner<Ready>();
                    ready.uid = br.ReadUInt32();
                    return ready;
                }
            }
        }

        public void Release()
        {
        }
    }

    public class Leave : IMessaged
    {
        public uint uid;

        public static byte[] Encode(Leave data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.uid);
                    return ms.ToArray();
                }
            }
        }

        public static Leave Decode(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Leave leave = GameFrameworkFactory.Spawner<Leave>();
                    leave.uid = br.ReadUInt32();
                    return leave;
                }
            }
        }

        public void Release()
        {
        }
    }

    public class Join : IMessaged
    {
        public uint uid;
        public string path;
        public TSVector position;
        public TSQuaternion rotation;

        public void Release()
        {
        }

        public static byte[] Create(uint uid, string path, TSVector position, TSQuaternion rotation)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(uid);
                    bw.Write(path);
                    bw.Write(position.x.RawValue);
                    bw.Write(position.y.RawValue);
                    bw.Write(position.z.RawValue);
                    bw.Write(rotation.x.RawValue);
                    bw.Write(rotation.y.RawValue);
                    bw.Write(rotation.z.RawValue);
                    bw.Write(rotation.w.RawValue);
                    return ms.ToArray();
                }
            }
        }

        public static Join Decode(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Join join = GameFrameworkFactory.Spawner<Join>();
                    join.uid = br.ReadUInt32();
                    join.path = br.ReadString();
                    join.position = new TSVector(br.ReadInt64(), br.ReadInt64(), br.ReadInt64());
                    join.rotation = new TSQuaternion(br.ReadInt64(), br.ReadInt64(), br.ReadInt64(), br.ReadInt64());
                    return join;
                }
            }
        }
    }

    /// <summary>
    /// 帧同步信息
    /// </summary>
    public class SyncData : IMessaged
    {
        /// <summary>
        /// 帧编号
        /// </summary>
        public long frame;

        /// <summary>
        /// 当前帧所有玩家的输入
        /// </summary>
        public List<InputData> frameData = new();

        public static byte[] Encode(SyncData data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.frame);
                    bw.Write(data.frameData.Count);
                    foreach (var item in data.frameData)
                    {
                        bw.Write(item.owner);
                        bw.Write(item.fpList.Count);
                        foreach (var fp in item.fpList)
                        {
                            bw.Write(fp.Key);
                            bw.Write(fp.Value.RawValue);
                        }
                    }

                    return ms.ToArray();
                }
            }
        }

        public static SyncData Decode(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    SyncData syncData = GameFrameworkFactory.Spawner<SyncData>();
                    syncData.frame = br.ReadInt64();
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        InputData inputData = GameFrameworkFactory.Spawner<InputData>();
                        inputData.owner = br.ReadUInt32();
                        int fpCount = br.ReadInt32();
                        for (int j = 0; j < fpCount; j++)
                        {
                            inputData.fpList.Add(br.ReadByte(), br.ReadInt64());
                        }
                    }

                    return syncData;
                }
            }
        }


        /// <summary>
        /// 添加玩家输入数据
        /// </summary>
        /// <param name="data"></param>
        public void AddFrameData(InputData data)
        {
            frameData.Add(data);
        }

        /// <summary>
        /// 获取当前玩家的输入数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public InputData GetFrameData(uint id)
        {
            InputData inputData = frameData.Find(x => x.owner == id);
            if (inputData is null)
            {
                inputData = GameFrameworkFactory.Spawner<InputData>();
            }

            return inputData;
        }

        public void RemoveFrameData(uint id)
        {
            frameData.RemoveAll(x => x.owner == id);
        }

        public bool Contains(uint uid)
        {
            return frameData.Exists(x => x.owner == uid);
        }

        public static SyncData Merge(params SyncData[] data)
        {
            SyncData syncData = GameFrameworkFactory.Spawner<SyncData>();
            foreach (SyncData sync in data)
            {
                foreach (InputData inputData in sync.frameData)
                {
                    if (syncData.Contains(inputData.owner))
                    {
                        syncData.RemoveFrameData(inputData.owner);
                    }

                    syncData.AddFrameData(inputData);
                }
            }

            return syncData;
        }

        public void Release()
        {
            frameData.Clear();
        }
    }

    /// <summary>
    /// 玩家输入数据
    /// </summary>
    public class InputData : IReferenceObject
    {
        /// <summary>
        /// 玩家id
        /// </summary>
        public uint owner;

        public Dictionary<byte, FP> fpList = new();

        /// <summary>
        /// 序列化输入数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encode(InputData data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(data.owner);
                    bw.Write(data.fpList.Count);
                    foreach (var item in data.fpList)
                    {
                        bw.Write(item.Key);
                        bw.Write(item.Value.RawValue);
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 反序列化输入数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static InputData Decode(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    InputData inputData = GameFrameworkFactory.Spawner<InputData>();
                    inputData.owner = br.ReadUInt32();
                    int count = br.ReadInt32();
                    inputData.fpList = new Dictionary<byte, FP>();
                    for (int i = 0; i < count; i++)
                    {
                        inputData.fpList.Add(br.ReadByte(), br.ReadInt64());
                    }

                    return inputData;
                }
            }
        }

        /// <summary>
        /// 获取输入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FP Get(byte id)
        {
            if (fpList.TryGetValue(id, out var value))
            {
                return value;
            }

            return default(FP);
        }

        /// <summary>
        /// 设置输入数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void Set(byte id, FP value)
        {
            if (fpList.ContainsKey(id))
            {
                fpList[id] = value;
            }
            else
            {
                fpList.Add(id, value);
            }
        }


        public void Clear()
        {
            fpList.Clear();
        }


        public void Release()
        {
            Clear();
        }
    }
}