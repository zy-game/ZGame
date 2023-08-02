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
        private Status _status;
        public long time { get; set; }
        public string name { get; set; }
        public byte[] bytes { get; set; }
        public float progress { get; private set; }
        public VersionOptions version { get; set; }
        private List<ISubscribe> _subscribes = new List<ISubscribe>();

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

        public IEnumerator Execute(params object[] paramsList)
        {
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                _status = Status.Failed;
                Completion();
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

            _status = Status.Success;
            Completion();
        }

        public void Subscribe(ISubscribe subscribe)
        {
            _subscribes.Add(subscribe);
        }

        public bool EnsureExecuteSuccessfuly()
        {
            return _status == Status.Success;
        }

        void Completion()
        {
            foreach (var VARIABLE in _subscribes)
            {
                if (VARIABLE is ISubscribe<IReadFileExecuteHandle> read)
                {
                    read.Execute(this);
                }
                else
                {
                    VARIABLE.Execute(this);
                }
            }
        }
    }
}