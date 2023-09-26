using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IReadFileExecuteHandle : IExecuteHandle<IReadFileExecuteHandle>
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IReadFileExecuteHandle Create(string name, int version)
        {
            InternalVFSReaderFileExecuteHandle internalVfsReaderFileExecuteHandle = Activator.CreateInstance<InternalVFSReaderFileExecuteHandle>();
            internalVfsReaderFileExecuteHandle.name = name;
            internalVfsReaderFileExecuteHandle.version = version;
            return internalVfsReaderFileExecuteHandle;
        }

        class InternalVFSReaderFileExecuteHandle : GameExecuteHandle<IReadFileExecuteHandle>, IReadFileExecuteHandle
        {
            public string name { get; set; }
            public long time { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }

            protected override IEnumerator DOExecute()
            {
                VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
                if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != version)
                {
                    status = Status.Failed;
                    yield break;
                }

                bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
                version = vfsDatas[0].version;
                long time = vfsDatas[0].time;
                int offset = 0;
                for (int i = 0; i < vfsDatas.Length; i++)
                {
                    vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                    offset += vfsDatas[i].fileLenght;
                    yield return new WaitForEndOfFrame();
                }

                status = Status.Success;
            }

            public void Dispose()
            {
                version = 0;
                name = String.Empty;
                bytes = Array.Empty<byte>();
                time = 0;
                GC.SuppressFinalize(this);
            }
        }
    }
}