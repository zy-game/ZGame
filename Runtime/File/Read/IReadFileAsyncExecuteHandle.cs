using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    /// <summary>
    /// 文件读取句柄
    /// </summary>
    public interface IReadFileAsyncExecuteHandle : IExecuteAsyncHandle<IReadFileAsyncExecuteHandle>, IReadFileExecuteHandle
    {
    }

    class GameReadFileAsyncExecuteHandle : ExecuteAsyncHandle<IReadFileAsyncExecuteHandle>, IReadFileAsyncExecuteHandle
    {
        public long time { get; set; }
        public string name { get; set; }
        public byte[] bytes { get; set; }
        public ExecuteStatus status { get; set; }
        public VersionOptions version { get; set; }

        public override void Execute(params object[] args)
        {
            name = args[0].ToString();
            GetCoroutine().Startup();
        }

        protected override IEnumerator GenericExeuteCoroutine()
        {
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                status = ExecuteStatus.Failed;
                this.Completion();
                yield break;
            }

            bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            version = vfsDatas[0].version;
            time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
                yield return new WaitForEndOfFrame();
            }

            status = ExecuteStatus.Success;
            this.Completion();
        }
    }
}