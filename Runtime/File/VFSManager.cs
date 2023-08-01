using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ZEngine.VFS
{
    public class VFSManager : Single<VFSManager>
    {
        private List<VFSData> dataList = new List<VFSData>();
        private List<VFSHandle> vfsList = new List<VFSHandle>();

        class VFSHandle : IReference
        {
            public float time;
            public string name;
            public FileStream fileStream;

            public void Release()
            {
                time = 0;
                name = String.Empty;
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        public VFSManager()
        {
            string filePath = GetLocalFilePath("vfs");
            if (!File.Exists(filePath))
            {
                return;
            }

            SubscribeMethodHandle.Create(CheckFileStreamTimeout).Timer(VFSOptions.instance.time);
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
                Engine.Class.Release(handle);
            }
        }

        /// <summary>
        /// 获取本地缓存文件路径
        /// </summary>
        /// <param name="fileName">文件名，不包含扩展名</param>
        /// <returns></returns>
        public static string GetLocalFilePath(string fileName)
        {
            return $"{Application.persistentDataPath}/{fileName}.{VFSOptions.instance.extension}";
        }

        /// <summary>
        /// 保存VFS数据
        /// </summary>
        private void SaveVFSData()
        {
            string filePath = GetLocalFilePath("vfs");
            File.WriteAllText(filePath, Engine.Json.ToJson(dataList));
        }

        /// <summary>
        /// 获取未使用的VFS数据块，如果不存在未使用的数据块，则创建一个新的VFS
        /// </summary>
        /// <param name="lenght">指定VFS数据块大小，如果不指定则使用配置大小</param>
        /// <returns></returns>
        public VFSData GetVFSData(int lenght = 0)
        {
            VFSData vfsData = dataList.Find(x => x.name.IsNullOrEmpty() && x.fileLenght >= lenght);
            if (vfsData is null)
            {
                int count = lenght > VFSOptions.instance.sgementLenght ? 1 : VFSOptions.instance.sgementCount;
                CreateVFSystem(count, lenght);
                return GetVFSData(lenght);
            }

            return vfsData;
        }

        /// <summary>
        /// 创建VFS
        /// </summary>
        /// <param name="count">数据块数量</param>
        /// <param name="lenght">每个数据块的长度，默认为VFSOptions.sgementLenght</param>
        /// <returns></returns>
        public VFSData[] CreateVFSystem(int count, int lenght = 0)
        {
            List<VFSData> list = new List<VFSData>();
            string vfsName = Guid.NewGuid().ToString().Replace("-", String.Empty);
            string vfsPath = GetLocalFilePath(vfsName);
            if (File.Exists(vfsPath))
            {
                File.Delete(vfsPath);
            }

            FileStream fileStream = GetFileStream(vfsName);
            for (int i = 0; i < count; i++)
            {
                list.Add(new VFSData() { vfs = vfsName, length = Mathf.Max(lenght, VFSOptions.instance.sgementLenght) });
            }

            dataList.AddRange(list);
            SaveVFSData();
            return list.ToArray();
        }


        public FileStream GetFileStream(string vfsName)
        {
            VFSHandle handle = vfsList.Find(x => x.name == vfsName);
            if (handle.fileStream is not null)
            {
                handle.time = Time.realtimeSinceStartup + VFSOptions.instance.time;
                return handle.fileStream;
            }

            handle = Engine.Class.Loader<VFSHandle>();
            handle.name = vfsName;
            handle.time = Time.realtimeSinceStartup + VFSOptions.instance.time;
            handle.fileStream = new FileStream(GetLocalFilePath(vfsName), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            vfsList.Add(handle);
            return handle.fileStream;
        }

        public bool Exist(string fileName)
        {
            if (VFSOptions.instance.vfsState == Switch.On)
            {
                return dataList.Find(x => x.name == fileName) is not null;
            }

            return File.Exists(GetLocalFilePath(fileName));
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

        public VersionOptions GetFileVersion(string fileName)
        {
            VFSData vfsData = dataList.Find(x => x.name == fileName);
            if (vfsData is null)
            {
                return VersionOptions.None;
            }

            return vfsData.version;
        }

        public VFSData[] GetFileData(string fileName)
        {
            return dataList.Where(x => x.name == fileName).ToArray();
        }

        public IWriteFileExecuteHandle WriteFile(string fileName, byte[] bytes, VersionOptions version)
        {
            Delete(fileName);
            GameWriteFileExecuteHandle writeFileExecuteHandle = Engine.Class.Loader<GameWriteFileExecuteHandle>();
            writeFileExecuteHandle.Execute(fileName, bytes, version);
            SaveVFSData();
            return writeFileExecuteHandle;
        }

        public IWriteFileAsyncExecuteHandle WriteFileAsync(string fileName, byte[] bytes, VersionOptions version)
        {
            Delete(fileName);
            GameWriteFileAsyncExecuteHandle writeFileExecuteHandle = Engine.Class.Loader<GameWriteFileAsyncExecuteHandle>();
            writeFileExecuteHandle.Execute(fileName, bytes, version);
            SaveVFSData();
            return writeFileExecuteHandle;
        }

        public IReadFileExecuteHandle ReadFile(string fileName)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            GameReadFileExecuteHandle gameReadFileExecuteHandle = Engine.Class.Loader<GameReadFileExecuteHandle>();
            gameReadFileExecuteHandle.Execute(fileName);
            return gameReadFileExecuteHandle;
        }

        public IReadFileAsyncExecuteHandle ReadFileAsync(string fileName)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            GameReadFileAsyncExecuteHandle gameReadFileAsyncExecuteHandle = Engine.Class.Loader<GameReadFileAsyncExecuteHandle>();
            gameReadFileAsyncExecuteHandle.Execute(fileName);
            return gameReadFileAsyncExecuteHandle;
        }
    }
}