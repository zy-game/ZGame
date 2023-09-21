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
        VersionOptions version { get; }

        internal static IWriteFileExecute Create(string name, byte[] bytes, VersionOptions version)
        {
            InternalVFSWriteFileExecute internalVfsWriteFileExecute = Activator.CreateInstance<InternalVFSWriteFileExecute>();
            internalVfsWriteFileExecute.name = name;
            internalVfsWriteFileExecute.bytes = bytes;
            internalVfsWriteFileExecute.version = version;
            return internalVfsWriteFileExecute;
        }

        class InternalVFSWriteFileExecute : AbstractExecute, IWriteFileExecute
        {
            public string name { get; set; }
            public byte[] bytes { get; set; }
            public VersionOptions version { get; set; }

            public override void Dispose()
            {
                name = String.Empty;
                bytes = Array.Empty<byte>();
                version = null;
                GC.SuppressFinalize(this);
            }

            protected override void ExecuteCommand()
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