using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
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
                ZGame.Console.Log("Dispose File: " + name);
                name = String.Empty;
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            vfsList.ForEach(x => x.Dispose());
            ZGame.Console.Log("释放所有文件句柄");
        }

        public VFSManager()
        {
            string filePath = ZGame.GetLocalFilePath("vfs");
            if (!File.Exists(filePath))
            {
                return;
            }

            dataList = ZGame.Json.Parse<List<VFSData>>(File.ReadAllText(filePath));
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
                fileStream = new FileStream(ZGame.GetLocalFilePath(vfs), FileMode.OpenOrCreate, FileAccess.ReadWrite)
            });
            return handle;
        }

        /// <summary>
        /// 保存VFS数据
        /// </summary>
        internal void SaveVFSData()
        {
            string filePath = ZGame.GetLocalFilePath("vfs");
            File.WriteAllText(filePath, ZGame.Json.ToJson(dataList));
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
                        VFSHandle handle = GetVFSHandle(ZGame.RandomName());
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
                        VFSHandle handle = GetVFSHandle(ZGame.RandomName());
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
                        ZGame.Console.Log(dataList.Count);
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

            ZGame.Console.Log(fileName, vfsData.version);
            return vfsData.version;
        }

        public IWriteFileResult WriteFile(string fileName, byte[] bytes, int version)
        {
            Delete(fileName);
            VFSData[] vfsDataList = VFSManager.instance.GetVFSData(bytes.Length);
            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Write(fileName, bytes, offset, length, version, index);
                offset += VARIABLE.length;
                index++;
            }

            SaveVFSData();
            return IWriteFileResult.Create(fileName, bytes, version);
        }

        public async UniTask<IWriteFileResult> WriteFileAsync(string fileName, byte[] bytes, int version)
        {
            Delete(fileName);
            VFSData[] vfsDataList = VFSManager.instance.GetVFSData(bytes.Length);
            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                await VARIABLE.WriteAsync(fileName, bytes, offset, length, version, index);
                offset += VARIABLE.length;
                index++;
            }

            SaveVFSData();
            return IWriteFileResult.Create(fileName, bytes, version);
        }

        public IReadFileResult ReadFile(string fileName, int versionOptions)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            VFSData[] vfsDatas = GetFileData(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != versionOptions)
            {
                return null;
            }

            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            long time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
            }

            return IReadFileResult.Create(fileName, bytes, time, versionOptions);
        }

        public async UniTask<IReadFileResult> ReadFileAsync(string fileName, int versionOptions)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            VFSData[] vfsDatas = GetFileData(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != versionOptions)
            {
                return default;
            }

            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            long time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                await vfsDatas[i].ReadAsync(bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
            }

            return IReadFileResult.Create(fileName, bytes, time, versionOptions);
        }
    }
}