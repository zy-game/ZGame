using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IReadFileExecuteHandle : IExecuteHandle<IReadFileExecuteHandle>
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        VersionOptions version { get; }
    }

    class DefaultReadFileExecuteHandle : AbstractExecuteHandle, IExecuteHandle<IReadFileExecuteHandle>, IReadFileExecuteHandle
    {
        public string name { get; set; }
        public long time { get; set; }
        public byte[] bytes { get; set; }
        public VersionOptions version { get; set; }

        public override void Release()
        {
            base.Release();
        }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            name = paramsList[0].ToString();
            version = paramsList[1] is null ? VersionOptions.None : (VersionOptions)paramsList[1];
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != version)
            {
                status = Status.Failed;
                yield break;
            }

            bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            version = vfsDatas[0].version;
            long time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
                yield return new WaitForEndOfFrame();
            }

            status = Status.Success;
        }
    }
}