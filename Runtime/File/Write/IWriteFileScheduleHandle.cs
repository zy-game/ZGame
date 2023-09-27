using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine.VFS
{
    public interface IWriteFileScheduleHandle : IScheduleHandle<IWriteFileScheduleHandle>
    {
        string name { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IWriteFileScheduleHandle Create(string name, byte[] bytes, int version)
        {
            InternalVfsWriteFileScheduleHandle internalVfsWriteFileScheduleHandle = Activator.CreateInstance<InternalVfsWriteFileScheduleHandle>();
            internalVfsWriteFileScheduleHandle.name = name;
            internalVfsWriteFileScheduleHandle.bytes = bytes;
            internalVfsWriteFileScheduleHandle.version = version;
            return internalVfsWriteFileScheduleHandle;
        }

        class InternalVfsWriteFileScheduleHandle : IWriteFileScheduleHandle
        {
            public string name { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }
            public Status status { get; set; }
            public IWriteFileScheduleHandle result => this;
            private ISubscriber subscriber;

            private IEnumerator DOExecute()
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
                    yield return WaitFor.Create(0.01f);
                }

                VFSManager.instance.SaveVFSData();
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
                version = 0;
                status = Status.None;
                subscriber?.Dispose();
                subscriber = null;
                name = String.Empty;
                bytes = Array.Empty<byte>();
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