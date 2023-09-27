using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IReadFileScheduleHandle : IScheduleHandle<IReadFileScheduleHandle>
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IReadFileScheduleHandle Create(string name, int version)
        {
            InternalVfsReaderFileScheduleHandle internalVfsReaderFileScheduleHandle = Activator.CreateInstance<InternalVfsReaderFileScheduleHandle>();
            internalVfsReaderFileScheduleHandle.name = name;
            internalVfsReaderFileScheduleHandle.version = version;
            return internalVfsReaderFileScheduleHandle;
        }

        class InternalVfsReaderFileScheduleHandle : IReadFileScheduleHandle
        {
            public string name { get; set; }
            public long time { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }
            public Status status { get; set; }
            public IReadFileScheduleHandle result => this;
            private ISubscriber subscriber;

            private IEnumerator DOExecute()
            {
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

            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                DOExecute().StartCoroutine(OnComplate);
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(this);
                }

            }

            public void Dispose()
            {
                time = 0;
                version = 0;
                subscriber?.Dispose();
                subscriber = null;
                name = String.Empty;
                status = Status.None;
                bytes = Array.Empty<byte>();
                GC.SuppressFinalize(this);
                GC.SuppressFinalize(this);
            }

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }
        }
    }
}