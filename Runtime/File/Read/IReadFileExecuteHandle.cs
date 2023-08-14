using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IReadFileExecuteHandle : IExecuteHandle<ReadFileExecuteResult>
    {
    }

    class DefaultReadFileExecuteHandle : ExecuteHandle<ReadFileExecuteResult>, IReadFileExecuteHandle
    {
        private string name;
        private VersionOptions version;

        public override void Release()
        {
            result = default;
            base.Release();
        }

        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            name = paramsList[0].ToString();
            version = paramsList[1] is null ? VersionOptions.None : (VersionOptions)paramsList[1];
            OnStart().StartCoroutine();
        }

        IEnumerator OnStart()
        {
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != version)
            {
                status = Status.Failed;
                OnComplete();
                yield break;
            }

            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            version = vfsDatas[0].version;
            long time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
                yield return new WaitForEndOfFrame();
            }

            result = ReadFileExecuteResult.Create(name, time, bytes, version);
            status = Status.Success;
            OnComplete();
        }
    }
}