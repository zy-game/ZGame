using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ZGame.Resource
{
    /// <summary>
    /// 虚拟文件系统管理器
    /// </summary>
    public class VFSManager : GameManager
    {
        private const int FILE_BUFFER_COUNT = 20;
        private const int FILE_BUFFER_SIZE = 1024 * 1024 * 1;
        private static string CONFIG_FILE_PATH = String.Empty;
        private List<VFSData> cfgMap = new List<VFSData>();
        private List<VFSystem> vfsList = new List<VFSystem>();

        public override void OnAwake(params object[] args)
        {
            string cfgData = "[]";
            CONFIG_FILE_PATH = AppCore.GetCachePath(AppCore.name);
#if UNITY_WEBGL
            cfgData = PlayerPrefs.GetString(AppCore.name,"[]");
#else
            if (File.Exists(CONFIG_FILE_PATH))
            {
                cfgData = File.ReadAllText(CONFIG_FILE_PATH);
            }
#endif
            AppCore.Logger.Log(cfgData);
            cfgMap = JsonConvert.DeserializeObject<List<VFSData>>(cfgData);
        }

        public override void Release()
        {
            Save();
            AppCore.Logger.Log("Release VFSManager");
            vfsList.ForEach(RefPooled.Free);
            vfsList.Clear();
        }

        /// <summary>
        /// 保存虚拟文件系统配置
        /// </summary>
        private void Save()
        {
            string cfgData = JsonConvert.SerializeObject(cfgMap);
#if UNITY_WEBGL
            PlayerPrefs.SetString(AppCore.name,cfgData);
#else
            File.WriteAllText(CONFIG_FILE_PATH, cfgData);
#endif
        }

        /// <summary>
        /// 获取虚拟文件流
        /// </summary>
        /// <param name="vfsName"></param>
        /// <returns></returns>
        private VFSystem GetVFSystem(string vfsName)
        {
            VFSystem vfs = vfsList.Find(x => x.name.Equals(vfsName));
            if (vfs is null)
            {
                vfsList.Add(vfs = VFSystem.Create(vfsName));
                AppCore.Logger.Log("Create VFS:" + vfsName);
            }

            return vfs;
        }

        /// <summary>
        /// 获取指定文件的数据配置
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private List<VFSData> GetFileDataList(string fileName)
        {
            return cfgMap.FindAll(x => x.file.IsNullOrEmpty() is false && x.file.Equals(fileName));
        }

        /// <summary>
        /// 获取未使用的虚拟文件配置
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private List<VFSData> GetFreeFileList(int length)
        {
            List<VFSData> dataList = cfgMap.FindAll(x => x.use is false);
            int count = (int)Math.Ceiling(length / (float)FILE_BUFFER_SIZE);
            if (dataList.Count >= count)
            {
                return dataList.GetRange(0, count);
            }

            int newCount = count - dataList.Count;
            while (dataList.Count < count)
            {
                string vfsName = Guid.NewGuid().ToString().Replace("-", String.Empty);
                for (int i = 0; i < FILE_BUFFER_COUNT; i++)
                {
                    VFSData data = VFSData.Create(vfsName, i * FILE_BUFFER_SIZE);
                    if (dataList.Count < count)
                    {
                        dataList.Add(data);
                    }

                    cfgMap.Add(data);
                }
            }

            return dataList;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Delete(string fileName)
        {
            List<VFSData> files = cfgMap.FindAll(x => x.file.Equals(fileName));
            foreach (var VARIABLE in files)
            {
                VARIABLE.Unuse();
            }

            File.Delete(AppCore.GetCachePath(fileName));
            Save();
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool Exist(string fileName, uint version)
        {
            return cfgMap.Exists(x => x.file.Equals(fileName) && x.version == version);
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Exist(string fileName)
        {
            return cfgMap.Exists(x => x.file.Equals(fileName));
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public Status Write(string fileName, byte[] bytes, uint version)
        {
            Delete(fileName);
            List<VFSData> files = GetFreeFileList(bytes.Length);
            int offset = 0;
            int writeLenght = Math.Min(bytes.Length, FILE_BUFFER_SIZE);
            for (int i = 0; i < files.Count; i++)
            {
                VFSystem vfSystem = GetVFSystem(files[i].vfs);
                if (vfSystem.Write(files[i].offset, bytes, offset, writeLenght) is not Status.Success)
                {
                    files.ForEach(x => x.Unuse());
                    return Status.Fail;
                }

                files[i].Use(fileName, version, writeLenght, i);
                offset += writeLenght;
                writeLenght = Math.Min(bytes.Length - offset, FILE_BUFFER_SIZE); //bytes.Length - offset > FILE_BUFFER_SIZE ? FILE_BUFFER_SIZE : bytes.Length - offset;
            }

            Save();
            return Status.Success;
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public Status Write(string fileName, Stream stream, uint version)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return Write(fileName, ms.ToArray(), version);
            }
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public async UniTask<Status> WriteAsync(string fileName, byte[] bytes, uint version, CancellationToken cancellationToken = default)
        {
            Delete(fileName);
            List<VFSData> files = GetFreeFileList(bytes.Length);
            int offset = 0;
            int writeLenght = Math.Min(bytes.Length, FILE_BUFFER_SIZE);
            for (int i = 0; i < files.Count; i++)
            {
                VFSystem vfSystem = GetVFSystem(files[i].vfs);
                if (await vfSystem.WriteAsync(files[i].offset, bytes, offset, writeLenght, cancellationToken) is not Status.Success)
                {
                    files.ForEach(x => x.Unuse());
                    return Status.Fail;
                }

                files[i].Use(fileName, version, writeLenght, i);
                offset += writeLenght;
                writeLenght = Math.Min(bytes.Length - offset, FILE_BUFFER_SIZE); //bytes.Length - offset > FILE_BUFFER_SIZE ? FILE_BUFFER_SIZE : bytes.Length - offset;
            }

            Save();
            return Status.Success;
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public UniTask<Status> WriteAsync(string fileName, Stream stream, uint version, CancellationToken cancellationToken = default)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return WriteAsync(fileName, ms.ToArray(), version, cancellationToken);
            }
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件数据</returns>
        public byte[] Read(string fileName)
        {
            List<VFSData> fileDatas = GetFileDataList(fileName);
            if (fileDatas.Count == 0)
            {
                return Array.Empty<byte>();
            }

            fileDatas.Sort((x, y) => x.index.CompareTo(y.index));
            byte[] bytes = new byte[fileDatas.Sum(x => x.lenght)];
            int offset = 0;
            for (int i = 0; i < fileDatas.Count; i++)
            {
                if (fileDatas[i].index != i)
                {
                    return Array.Empty<byte>();
                }

                VFSystem vfsystem = GetVFSystem(fileDatas[i].vfs);
                if (vfsystem.Read(fileDatas[i].offset, bytes, offset, fileDatas[i].lenght) is not Status.Success)
                {
                    return Array.Empty<byte>();
                }

                offset += fileDatas[i].lenght;
            }

            return bytes;
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件数据</returns>
        public async UniTask<byte[]> ReadAsync(string fileName, CancellationToken cancellationToken = default)
        {
            List<VFSData> fileDatas = GetFileDataList(fileName);
            if (fileDatas.Count == 0)
            {
                return Array.Empty<byte>();
            }

            fileDatas.Sort((x, y) => x.index.CompareTo(y.index));
            byte[] bytes = new byte[fileDatas.Sum(x => x.lenght)];
            int offset = 0;
            for (int i = 0; i < fileDatas.Count; i++)
            {
                if (fileDatas[i].index != i)
                {
                    return Array.Empty<byte>();
                }

                VFSystem vfsystem = GetVFSystem(fileDatas[i].vfs);
                if (await vfsystem.ReadAsync(fileDatas[i].offset, bytes, offset, fileDatas[i].lenght, cancellationToken) is not Status.Success)
                {
                    return Array.Empty<byte>();
                }

                offset += fileDatas[i].lenght;
            }

            return bytes;
        }
    }
}