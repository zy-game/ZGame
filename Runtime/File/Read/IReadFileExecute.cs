using System;
using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    public interface IReadFileExecute : IExecute
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IReadFileExecute Create(string name, int version)
        {
            InternalVFSReadderExecute internalVfsReadderExecute = Activator.CreateInstance<InternalVFSReadderExecute>();
            internalVfsReadderExecute.name = name;
            internalVfsReadderExecute.version = version;
            return internalVfsReadderExecute;
        }

        class InternalVFSReadderExecute : IExecute, IReadFileExecute
        {
            public string name { get; set; }
            public long time { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }

            public void Dispose()
            {
                version = 0;
                name = String.Empty;
                bytes = Array.Empty<byte>();
                time = 0;
                GC.SuppressFinalize(this);
            }

            public void Execute()
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