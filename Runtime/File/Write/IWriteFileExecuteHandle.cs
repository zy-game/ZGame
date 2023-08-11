using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    /// <summary>
    /// 文件写入句柄
    /// </summary>
    public interface IWriteFileExecuteHandle : IExecuteHandle<IWriteFileExecuteHandle>
    {
        /// <summary>
        /// 文件名
        /// </summary>
        string name { get; }

        /// <summary>
        /// 文件数据
        /// </summary>
        byte[] bytes { get; }

        /// <summary>
        /// 文件版本
        /// </summary>
        VersionOptions version { get; }
    }

    class DefaultWriteFileExecuteHandle : IWriteFileExecuteHandle
    {
        private List<ISubscribeHandle> _subscribes = new List<ISubscribeHandle>();

        public string name { get; set; }
        public byte[] bytes { get; set; }
        public Status status { get; set; }
        public VersionOptions version { get; set; }

        public void Release()
        {
            status = Status.None;
            _subscribes.ForEach(Engine.Class.Release);
            _subscribes.Clear();
            name = String.Empty;
            bytes = Array.Empty<byte>();
            version = VersionOptions.None;
        }

        public void Subscribe(ISubscribeHandle subscribe)
        {
            _subscribes.Add(subscribe);
        }

        public void Execute(params object[] args)
        {
            status = Status.Execute;
            if (args is null || args.Length is 0)
            {
                Engine.Console.Error("Not Find Write File Patg or fileData");
                _subscribes.ForEach(x => x.Execute(this));
                status = Status.Failed;
                return;
            }

            name = (string)args[0];
            bytes = (byte[])args[1];
            version = (VersionOptions)args[2];
            OnStart().StartCoroutine();
        }

        IEnumerator OnStart()
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
                yield return Timeout.Create(0.01f);
            }

            VFSManager.instance.SaveVFSData();
            status = Status.Success;
            _subscribes.ForEach(x => x.Execute(this));
        }

        public IEnumerator ExecuteComplete()
        {
            return WaitFor.Create(() => status == Status.Failed || status == Status.Success);
        }
    }
}