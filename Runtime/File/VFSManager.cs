using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ZEngine.VFS
{
    class VFSManager : Singleton<VFSManager>
    {
        private List<VFSData> dataList = new List<VFSData>();
        private List<VFSHandle> vfsList = new List<VFSHandle>();

        internal class VFSHandle : IDisposable
        {
            public float time;
            public string name;
            public FileStream fileStream;

            public void Dispose()
            {
                time = 0;
                Engine.Console.Log("Dispose File: " + name);
                name = String.Empty;
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            vfsList.ForEach(x => x.Dispose());
            Engine.Console.Log("释放所有文件句柄");
        }

        public VFSManager()
        {
            string filePath = Engine.GetLocalFilePath("vfs");
            if (!File.Exists(filePath))
            {
                return;
            }

            dataList = Engine.Json.Parse<List<VFSData>>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// 检查VFS缓存超时
        /// </summary>
        private void CheckFileStreamTimeout()
        {
            for (int i = vfsList.Count - 1; i >= 0; i--)
            {
                VFSHandle handle = vfsList[i];
                if (Time.realtimeSinceStartup < handle.time)
                {
                    continue;
                }

                vfsList.Remove(handle);
                handle.Dispose();
            }
        }

        internal VFSHandle GetVFSHandle(string vfs)
        {
            VFSHandle handle = vfsList.Find(x => x.name == vfs);
            if (handle is not null)
            {
                return handle;
            }

            vfsList.Add(handle = new VFSHandle()
            {
                name = vfs,
                time = Time.realtimeSinceStartup,
                fileStream = new FileStream(Engine.GetLocalFilePath(vfs), FileMode.OpenOrCreate, FileAccess.ReadWrite)
            });
            return handle;
        }

        /// <summary>
        /// 保存VFS数据
        /// </summary>
        internal void SaveVFSData()
        {
            string filePath = Engine.GetLocalFilePath("vfs");
            File.WriteAllText(filePath, Engine.Json.ToJson(dataList));
        }

        /// <summary>
        /// 获取未使用的VFS数据块，如果不存在未使用的数据块，则创建一个新的VFS
        /// </summary>
        /// <param name="lenght">指定VFS数据块大小，如果不指定则使用配置大小</param>
        /// <returns></returns>
        public VFSData[] GetVFSData(int lenght)
        {
            List<VFSData> result = new List<VFSData>();
            switch (VFSOptions.instance.layout)
            {
                case VFSLayout.Speed:
                    VFSData data = dataList.Find(x => x.use == Switch.Off && x.length >= lenght);
                    if (data is null)
                    {
                        VFSHandle handle = GetVFSHandle(Engine.RandomName());
                        data = new VFSData()
                        {
                            vfs = handle.name,
                            length = lenght,
                            offset = 0,
                            time = DateTimeOffset.Now.ToUnixTimeSeconds()
                        };
                        dataList.Add(data);
                        SaveVFSData();
                    }

                    result.Add(data);
                    break;
                case VFSLayout.Szie:
                    int count = lenght / VFSOptions.instance.Lenght;
                    count = lenght > count * VFSOptions.instance.Lenght ? count + 1 : count;
                    IEnumerable<VFSData> temp = dataList.Where(x => x.use == Switch.Off);

                    if (temp is null || temp.Count() < count)
                    {
                        VFSHandle handle = GetVFSHandle(Engine.RandomName());
                        for (int i = 0; i < VFSOptions.instance.Count; i++)
                        {
                            dataList.Add(new VFSData()
                            {
                                vfs = handle.name,
                                length = VFSOptions.instance.Lenght,
                                offset = i * VFSOptions.instance.Lenght,
                                time = DateTimeOffset.Now.ToUnixTimeSeconds()
                            });
                        }

                        SaveVFSData();
                        Engine.Console.Log(dataList.Count);
                        temp = dataList.Where(x => x.use is Switch.Off);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        result.Add(temp.ElementAt(i));
                    }

                    break;
            }

            return result.ToArray();
        }

        public VFSData[] GetFileData(string fileName)
        {
            return dataList.Where(x => x.name == fileName).ToArray();
        }

        public bool Exist(string fileName)
        {
            return dataList.Find(x => x.name == fileName) is not null;
        }

        public bool Delete(string fileName)
        {
            IEnumerable<VFSData> fileData = dataList.Where(x => x.name == fileName);
            foreach (var file in fileData)
            {
                file.Free();
            }

            SaveVFSData();
            return Exist(fileName);
        }

        public int GetFileVersion(string fileName)
        {
            VFSData vfsData = dataList.Find(x => x.name == fileName);
            if (vfsData is null)
            {
                return -1;
            }

            Engine.Console.Log(fileName, vfsData.version);
            return vfsData.version;
        }

        public IWriteFileScheduleResult WriteFile(string fileName, byte[] bytes, int version)
        {
            Delete(fileName);
            IWriteFileScheduleResult writeFileScheduleResult = IWriteFileScheduleResult.Create(fileName, bytes, version);
            writeFileScheduleResult.Execute();
            return writeFileScheduleResult;
        }

        public IWriteFileScheduleHandle WriteFileAsync(string fileName, byte[] bytes, int version)
        {
            Delete(fileName);
            IWriteFileScheduleHandle writeFileScheduleHandle = IWriteFileScheduleHandle.Create(fileName, bytes, version);
            writeFileScheduleHandle.Execute();
            return writeFileScheduleHandle;
        }

        public IReadFileScheduleResult ReadFile(string fileName, int versionOptions)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            IReadFileScheduleResult readFileScheduleResult = IReadFileScheduleResult.Create(fileName, versionOptions);
            readFileScheduleResult.Execute();
            return readFileScheduleResult;
        }

        public IReadFileScheduleHandle ReadFileAsync(string fileName, int versionOptions)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            IReadFileScheduleHandle readFileScheduleHandle = IReadFileScheduleHandle.Create(fileName, versionOptions);
            readFileScheduleHandle.Execute();
            return readFileScheduleHandle;
        }
    }
}