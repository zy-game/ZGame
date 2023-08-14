using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ZEngine.VFS
{
    public struct WriteFileExecuteResult
    {
        public string name { get; set; }
        public byte[] bytes { get; set; }
        public VersionOptions version { get; set; }

        public static WriteFileExecuteResult Create(string name, byte[] bytes, VersionOptions version)
        {
            return new WriteFileExecuteResult()
            {
                name = name,
                bytes = bytes,
                version = version
            };
        }
    }

    class DefaultWriteFileExecute : IExecute<WriteFileExecuteResult>
    {
        public WriteFileExecuteResult result { get; set; }

        public void Release()
        {
            result = default;
        }

        public void Execute(params object[] args)
        {
            result = WriteFileExecuteResult.Create((string)args[0], (byte[])args[1], (VersionOptions)args[2]);
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
            return;
        }
    }
}