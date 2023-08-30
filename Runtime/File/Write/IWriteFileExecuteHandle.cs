using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IWriteFileExecuteHandle : IExecuteHandle<IWriteFileExecuteHandle>
    {
        string name { get; }
        byte[] bytes { get; }
        VersionOptions version { get; }

        internal static IWriteFileExecuteHandle Create(string name, byte[] bytes, VersionOptions version)
        {
            InternalVFSWriteFileExecuteHandle internalVfsWriteFileExecuteHandle = Engine.Class.Loader<InternalVFSWriteFileExecuteHandle>();
            internalVfsWriteFileExecuteHandle.name = name;
            internalVfsWriteFileExecuteHandle.bytes = bytes;
            internalVfsWriteFileExecuteHandle.version = version;
            return internalVfsWriteFileExecuteHandle;
        }

        class InternalVFSWriteFileExecuteHandle : AbstractExecuteHandle, IExecuteHandle<IWriteFileExecuteHandle>, IWriteFileExecuteHandle
        {
            public string name { get; set; }
            public byte[] bytes { get; set; }
            public VersionOptions version { get; set; }

            public void Release()
            {
                name = String.Empty;
                bytes = Array.Empty<byte>();
                version = VersionOptions.None;
                base.Release();
            }

            protected override IEnumerator ExecuteCoroutine()
            {
                VFSData[] vfsDataList = VFSManager.instance.GetVFSData(bytes.Length);
                int offset = 0;
                int index = 0;
                foreach (var VARIABLE in vfsDataList)
                {
                    int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                    VARIABLE.Write(bytes, offset, length, version, index);
                    offset += VARIABLE.length;
                    index++;
                    VARIABLE.name = name;
                    yield return WaitFor.Create(0.01f);
                }

                VFSManager.instance.SaveVFSData();
                status = Status.Success;
            }
        }
    }
}