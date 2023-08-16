using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IWriteFileExecuteResult : IReference
    {
        string name { get; }
        byte[] bytes { get; }
        VersionOptions version { get; }
    }

    class WriteFileExecuteResult : IWriteFileExecuteResult
    {
        public void Release()
        {
            name = String.Empty;
            bytes = Array.Empty<byte>();
            version = null;
        }

        public string name { get; set; }
        public byte[] bytes { get; set; }
        public VersionOptions version { get; set; }
    }

    class DefaultWriteFileExecute : IExecute
    {
        public WriteFileExecuteResult result { get; set; }

        public void Release()
        {
            result = null;
            GC.SuppressFinalize(this);
        }

        public void Execute(params object[] args)
        {
            result = Engine.Class.Loader<WriteFileExecuteResult>();
            result.name = (string)args[0];
            result.bytes = (byte[])args[1];
            result.version = (VersionOptions)args[2];
            VFSData[] vfsDataList = VFSManager.instance.GetVFSData(result.bytes.Length);
            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                int length = result.bytes.Length - offset > VARIABLE.length ? VARIABLE.length : result.bytes.Length - offset;
                VARIABLE.Write(result.bytes, offset, length, result.version, index);
                offset += VARIABLE.length;
                VARIABLE.name = result.name;
                index++;
            }

            VFSManager.instance.SaveVFSData();
        }
    }
}