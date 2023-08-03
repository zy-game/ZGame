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
    public interface IReadFileExecuteHandle : IExecuteHandle<IReadFileExecuteHandle>
    {
        /// <summary>
        /// 文件名
        /// </summary>
        string name { get; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        long time { get; }

        /// <summary>
        /// 文件数据
        /// </summary>
        byte[] bytes { get; }

        /// <summary>
        /// 文件版本
        /// </summary>
        VersionOptions version { get; }
    }

    class DefaultReadFileAsyncExecuteHandle : IReadFileExecuteHandle
    {
        public long time { get; set; }
        public string name { get; set; }
        public byte[] bytes { get; set; }
        public Status status { get; set; }
        public float progress { get; private set; }
        public VersionOptions version { get; set; }
        private List<ISubscribeExecuteHandle> _subscribes = new List<ISubscribeExecuteHandle>();

        public void Release()
        {
            version = VersionOptions.None;
            bytes = Array.Empty<byte>();
            time = 0;
            name = String.Empty;
            progress = 0;
            _subscribes.ForEach(Engine.Class.Release);
            _subscribes.Clear();
        }

        public void Execute(params object[] paramsList)
        {
            name = paramsList[0].ToString();
            status = Status.Execute;
            StartReadFile().StartCoroutine();
        }

        private IEnumerator StartReadFile()
        {
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                status = Status.Failed;
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

            foreach (var VARIABLE in _subscribes)
            {
                VARIABLE.Execute(this);
            }

            status = Status.Success;
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            _subscribes.Add(subscribe);
        }
    }
}