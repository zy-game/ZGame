using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IWriteFileExecute : IExecute
    {
        string name { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IWriteFileExecute Create(string name, byte[] bytes, int version)
        {
            InternalVFSWriteFileExecute internalVfsWriteFileExecute = Activator.CreateInstance<InternalVFSWriteFileExecute>();
            internalVfsWriteFileExecute.name = name;
            internalVfsWriteFileExecute.bytes = bytes;
            internalVfsWriteFileExecute.version = version;
            return internalVfsWriteFileExecute;
        }

        class InternalVFSWriteFileExecute : IExecute, IWriteFileExecute
        {
            public string name { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }

            public void Dispose()
            {
                name = String.Empty;
                bytes = Array.Empty<byte>();
                version = 0;
                GC.SuppressFinalize(this);
            }

            public void Execute()
            {
                VFSData[] vfsDataList = VFSManager.instance.GetVFSData(bytes.Length);
                int offset = 0;
                int index = 0;
                foreach (var VARIABLE in vfsDataList)
                {
                    int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                    VARIABLE.Write(bytes, offset, length, version, index);
                    offset += VARIABLE.length;
                    VARIABLE.name = name;
                    index++;
                }

                VFSManager.instance.SaveVFSData();
            }
        }
    }
}