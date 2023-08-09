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
        byte[] result { get; }

        /// <summary>
        /// 文件版本
        /// </summary>
        VersionOptions version { get; }
    }

    class DefaultWriteFileExecuteHandle : IWriteFileExecuteHandle
    {
        private List<ISubscribeHandle> _subscribes = new List<ISubscribeHandle>();

        public string name { get; set; }
        public byte[] result { get; set; }
        public Status status { get; set; }
        public VersionOptions version { get; set; }

        public void Release()
        {
            status = Status.None;
            _subscribes.ForEach(Engine.Class.Release);
            _subscribes.Clear();
            name = String.Empty;
            result = Array.Empty<byte>();
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
            result = (byte[])args[1];
            version = (VersionOptions)args[2];
            OnStart().StartCoroutine();
        }

        IEnumerator OnStart()
        {
            VFSData vfsData = default;
            //todo 根据vfs布局写入文件
            if (VFSOptions.instance.layout == VFSLayout.ReadWritePriority || VFSOptions.instance.vfsState == Switch.Off)
            {
                //todo 如果单个文件小于等于一个VFS数据块，则写入VFS，如果大于单个VFS数据块，则写入单独的VFS中
                vfsData = VFSManager.instance.GetVFSData(Mathf.Max(VFSOptions.instance.sgementLenght, result.Length));
                if (vfsData is null)
                {
                    _subscribes.ForEach(x => x.Execute(this));
                    status = Status.Failed;
                    yield break;
                }

                vfsData.Write(result, 0, result.Length, version);
                status = Status.Success;
                _subscribes.ForEach(x => x.Execute(this));
                yield break;
            }

            int count = result.Length.SpiltCount(VFSOptions.instance.sgementLenght);
            int offset = 0;
            int length = 0;
            for (int i = 0; i < count; i++)
            {
                //todo 不用理会文件是否连续
                vfsData = VFSManager.instance.GetVFSData();
                if (vfsData is null)
                {
                    status = Status.Failed;
                    _subscribes.ForEach(x => x.Execute(this));
                    yield break;
                }

                offset += i * length;
                length = Mathf.Min(VFSOptions.instance.sgementLenght, result.Length - offset);
                vfsData.Write(result, offset, length, version);
            }

            status = Status.Success;
            _subscribes.ForEach(x => x.Execute(this));
        }

        public IEnumerator ExecuteComplete()
        {
            return WaitFor.Create(() => status == Status.Failed || status == Status.Success);
        }
    }
}