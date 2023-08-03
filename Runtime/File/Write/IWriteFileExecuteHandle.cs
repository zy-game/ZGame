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

    class DefaultWriteFileAsyncExecuteHandle : IWriteFileExecuteHandle
    {
        private List<ISubscribeExecuteHandle> _subscribes = new List<ISubscribeExecuteHandle>();


        public string name { get; set; }
        public byte[] bytes { get; set; }
        public Status status { get; set; }
        public VersionOptions version { get; set; }
        public float progress { get; private set; }

        public void Release()
        {
            status = Status.None;
            _subscribes.ForEach(Engine.Class.Release);
            _subscribes.Clear();
            name = String.Empty;
            bytes = Array.Empty<byte>();
            version = VersionOptions.None;
            progress = 0;
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            _subscribes.Add(subscribe);
        }

        public void Execute(params object[] args)
        {
            if (args is null || args.Length is 0)
            {
                status = Status.Failed;
                Engine.Console.Error("Not Find Write File Patg or fileData");
                ExecuteSubscribe();
                return;
            }

            name = (string)args[0];
            bytes = (byte[])args[1];
            version = (VersionOptions)args[2];
            OnStartWriteFile().StartCoroutine();
        }

        private IEnumerator OnStartWriteFile()
        {
            VFSData vfsData = default;
            //todo 根据vfs布局写入文件
            if (VFSOptions.instance.layout == VFSLayout.ReadWritePriority || VFSOptions.instance.vfsState == Switch.Off)
            {
                //todo 如果单个文件小于等于一个VFS数据块，则写入VFS，如果大于单个VFS数据块，则写入单独的VFS中
                vfsData = VFSManager.instance.GetVFSData(Mathf.Max(VFSOptions.instance.sgementLenght, bytes.Length));
                if (vfsData is null)
                {
                    status = Status.Failed;
                    ExecuteSubscribe();
                    yield break;
                }

                progress = 1f;
                vfsData.Write(bytes, 0, bytes.Length, version);
                ExecuteSubscribe();
                status = Status.Success;
                yield break;
            }

            int count = bytes.Length.SpiltCount(VFSOptions.instance.sgementLenght);
            int offset = 0;
            int length = 0;
            for (int i = 0; i < count; i++)
            {
                //todo 不用理会文件是否连续
                vfsData = VFSManager.instance.GetVFSData();
                if (vfsData is null)
                {
                    status = Status.Failed;
                    ExecuteSubscribe();
                    yield break;
                }

                offset += i * length;
                progress = (float)offset / bytes.Length;
                length = Mathf.Min(VFSOptions.instance.sgementLenght, bytes.Length - offset);
                vfsData.Write(bytes, offset, length, version);
            }

            ExecuteSubscribe();
            status = Status.Success;
        }

        private void ExecuteSubscribe()
        {
            foreach (var VARIABLE in _subscribes)
            {
                VARIABLE.Execute(this);
            }
        }
    }
}