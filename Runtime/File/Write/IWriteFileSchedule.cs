using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IWriteFileScheduleResult : ISchedule<IWriteFileScheduleResult>
    {
        string name { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IWriteFileScheduleResult Create(string name, byte[] bytes, int version)
        {
            InternalVfsWriteFileSchedule internalVfsWriteFileSchedule = Activator.CreateInstance<InternalVfsWriteFileSchedule>();
            internalVfsWriteFileSchedule.name = name;
            internalVfsWriteFileSchedule.bytes = bytes;
            internalVfsWriteFileSchedule.version = version;
            return internalVfsWriteFileSchedule;
        }

        class InternalVfsWriteFileSchedule : IWriteFileScheduleResult
        {
            public string name { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }
            public IWriteFileScheduleResult result => this;

            public void Execute(params object[] args)
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

            public void Dispose()
            {
                name = String.Empty;
                bytes = Array.Empty<byte>();
                version = 0;
                GC.SuppressFinalize(this);
            }
        }
    }
}