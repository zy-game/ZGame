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
        VersionOptions version { get; }

        internal static IReadFileExecute Create(string name, VersionOptions version)
        {
            InternalVFSReadderExecute internalVfsReadderExecute = Activator.CreateInstance<InternalVFSReadderExecute>();
            internalVfsReadderExecute.name = name;
            internalVfsReadderExecute.version = version;
            return internalVfsReadderExecute;
        }

        class InternalVFSReadderExecute : AbstractExecute, IReadFileExecute
        {
            public string name { get; set; }
            public long time { get; set; }
            public byte[] bytes { get; set; }
            public VersionOptions version { get; set; }

            public override void Dispose()
            {
                version = null;
                name = String.Empty;
                bytes = Array.Empty<byte>();
                time = 0;
                GC.SuppressFinalize(this);
            }

            protected override void OnExecute()
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