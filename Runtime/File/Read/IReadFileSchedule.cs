using System;
using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    public interface IReadFileScheduleResult : ISchedule<IReadFileScheduleResult>
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IReadFileScheduleResult Create(string name, int version)
        {
            InternalVfsReadderSchedule internalVfsReadderSchedule = Activator.CreateInstance<InternalVfsReadderSchedule>();
            internalVfsReadderSchedule.name = name;
            internalVfsReadderSchedule.version = version;
            return internalVfsReadderSchedule;
        }

        class InternalVfsReadderSchedule : IReadFileScheduleResult
        {
            public string name { get; set; }
            public long time { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }
            public IReadFileScheduleResult result => this;

            public void Dispose()
            {
                version = 0;
                name = String.Empty;
                bytes = Array.Empty<byte>();
                time = 0;
                GC.SuppressFinalize(this);
            }

            public void Execute(params object[] args)
            {
                VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
                if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != version)
                {
                    return;
                }

                bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
                version = vfsDatas[0].version;
                long time = vfsDatas[0].time;
                int offset = 0;
                for (int i = 0; i < vfsDatas.Length; i++)
                {
                    vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                    offset += vfsDatas[i].fileLenght;
                }
            }
        }
    }
}