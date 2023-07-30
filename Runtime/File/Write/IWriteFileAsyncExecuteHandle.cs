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
    public interface IWriteFileAsyncExecuteHandle : IExecuteAsyncHandle<IWriteFileAsyncExecuteHandle>, IWriteFileExecuteHandle
    {
    }

    class GameWriteFileAsyncExecuteHandle : ExecuteAsyncHandle<IWriteFileAsyncExecuteHandle>, IWriteFileAsyncExecuteHandle
    {
        public string name { get; set; }
        public byte[] bytes { get; set; }
        public ExecuteStatus status { get; set; }
        public VersionOptions version { get; set; }

        public override void Execute(params object[] args)
        {
            if (args is null || args.Length is 0)
            {
                status = ExecuteStatus.Failed;
                Engine.Console.Error("Not Find Write File Patg or fileData");
                this.Completion();
                return;
            }

            name = (string)args[0];
            bytes = (byte[])args[1];
            version = (VersionOptions)args[2];
            GetCoroutine().Startup();
        }

        protected override IEnumerator GenericExeuteCoroutine()
        {
            VFSData vfsData = default;
            //todo 根据vfs布局写入文件
            if (VFSOptions.instance.layout == VFSLayout.ReadWritePriority || VFSOptions.instance.vfsState == Switch.Off)
            {
                //todo 如果单个文件小于等于一个VFS数据块，则写入VFS，如果大于单个VFS数据块，则写入单独的VFS中
                vfsData = VFSManager.instance.GetVFSData(Mathf.Max(VFSOptions.instance.sgementLenght, bytes.Length));
                if (vfsData is null)
                {
                    status = ExecuteStatus.Failed;
                    this.Completion();
                    yield break;
                }

                progress = 1f;
                vfsData.Write(bytes, 0, bytes.Length, version);
                this.Completion();
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
                    status = ExecuteStatus.Failed;
                    this.Completion();
                    yield break;
                }

                offset += i * length;
                progress = (float)offset / bytes.Length;
                length = Mathf.Min(VFSOptions.instance.sgementLenght, bytes.Length - offset);
                vfsData.Write(bytes, offset, length, version);
            }

            this.Completion();
        }
    }
}