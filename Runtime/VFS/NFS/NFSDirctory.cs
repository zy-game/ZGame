using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ZGame.VFS
{
    partial class NFSDirctory
    {
        public static NFSDirctory OpenOrCreateBundle(string bundleName)
        {
            NFSDirctory bundle = GameFrameworkFactory.Spawner<NFSDirctory>();
            bundle.name = bundleName;
            List<NFSFileOptions> list = JsonConvert.DeserializeObject<List<NFSFileOptions>>(PlayerPrefs.GetString(bundleName, "[]"));
            bundle.optList = new ConcurrentStack<NFSFileOptions>();
            if (list.Count > 0)
            {
                bundle.optList.PushRange(list.ToArray());
            }

            if (bundle.optList is null || bundle.optList.Count == 0)
            {
                bundle.optList = new ConcurrentStack<NFSFileOptions>();
                for (int i = 0; i < VFSConfig.instance.chunkCount; i++)
                {
                    bundle.optList.Push(NFSFileOptions.Create(bundleName, i * VFSConfig.instance.chunkSize, VFSConfig.instance.chunkSize));
                }
            }

            return bundle;
        }
    }

    /// <summary>
    /// 文件包
    /// </summary>
    partial class NFSDirctory : IReferenceObject
    {
        private FileStream _stream;
        private object lockObj = new object();

        private ConcurrentStack<NFSFileOptions> optList;

        /// <summary>
        /// 包名
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// 可写入大小
        /// </summary>
        public long writeable => GetWriteableSize();

        /// <summary>
        /// 文件流句柄
        /// </summary>
        public FileStream stream
        {
            get
            {
                if (_stream is null)
                {
                    _stream = new FileStream(GameFrameworkEntry.GetApplicationFilePath(name), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                }

                return _stream;
            }
        }

        void SaveCfg()
        {
            string cfg = JsonConvert.SerializeObject(this.optList.ToList());
            PlayerPrefs.SetString(this.name, cfg);
            GameFrameworkEntry.Logger.Log(this.name + " -> " + cfg);
        }


        private long GetWriteableSize()
        {
            if (VFSConfig.instance.enable is false)
            {
                return optList.FirstOrDefault(x => x.state == 0) is null ? 0 : long.MaxValue;
            }

            return optList.Sum(x => x.length);
        }


        /// <summary>
        /// 回收
        /// </summary>
        public void Release()
        {
            optList.Clear();
            _stream?.Flush();
            _stream?.Close();
            _stream = null;
        }

        public bool TryGetFileVersion(string fileName, out uint version)
        {
            NFSFileOptions options = optList.FirstOrDefault(x => x.name == fileName);
            if (options is null)
            {
                version = 0;
                return false;
            }

            version = options.version;
            return true;
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns></returns>
        public bool Exists(string fileName)
        {
            return optList.FirstOrDefault(x => x.name == fileName) is not null;
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="version">文件版本</param>
        /// <returns></returns>
        public bool Exists(string fileName, uint version)
        {
            return optList.FirstOrDefault(x => x.name == fileName && x.version == version) is not null;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void Delete(string fileName)
        {
            foreach (var VARIABLE in optList)
            {
                if (VARIABLE.name.Equals(fileName))
                {
                    VARIABLE.Unuse();
                }
            }

            if (VFSConfig.instance.enable is false)
            {
                File.Delete(GameFrameworkEntry.GetApplicationFilePath(fileName));
            }

            SaveCfg();
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="data">文件二进制数据</param>
        /// <param name="version">文件版本</param>
        /// <returns></returns>
        public Status Write(string fileName, byte[] data, uint version)
        {
            if (Exists(fileName))
            {
                Delete(fileName);
            }

            if (VFSConfig.instance.enable is false)
            {
                NFSFileOptions options = optList.FirstOrDefault(x => x.state == 0);
                if (options is null)
                {
                    return Status.Fail;
                }

                options.Use(0, fileName, data.Length, version);
                File.WriteAllBytes(GameFrameworkEntry.GetApplicationFilePath(fileName), data);
                SaveCfg();
                return Status.Success;
            }

            int chunkSize = VFSConfig.instance.chunkSize;
            // 计算需要写入的块数  
            int totalChunks = (data.Length + chunkSize - 1) / chunkSize;
            // 遍历每个块并写入MemoryStream  
            for (int i = 0; i < totalChunks; i++)
            {
                NFSFileOptions options = optList.FirstOrDefault(x => x.state == 0);
                if (options is null)
                {
                    return Status.Fail;
                }

                // 计算当前块的起始索引  
                int startIndex = i * chunkSize;
                // 计算当前块要写入的字节数，注意避免越界  
                int bytesToWrite = Math.Min(options.length, data.Length - startIndex);
                // 定位到文件中的指定偏移量  
                stream.Seek(options.offset, SeekOrigin.Begin);
                // 将当前块数据写入文件  
                stream.Write(data, startIndex, bytesToWrite);
                options.Use(i, fileName, bytesToWrite, version);
            }

            stream.Flush();
            SaveCfg();
            return Status.Success;
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="data">文件二进制数据</param>
        /// <param name="version">文件版本</param>
        /// <returns></returns>
        public async UniTask<Status> WriteAsync(string fileName, byte[] data, uint version)
        {
            if (Exists(fileName))
            {
                Delete(fileName);
            }

            if (VFSConfig.instance.enable is false)
            {
                NFSFileOptions options = optList.FirstOrDefault(x => x.state == 0);
                if (options is null)
                {
                    return Status.Fail;
                }

                options.Use(0, fileName, data.Length, version);
                await File.WriteAllBytesAsync(GameFrameworkEntry.GetApplicationFilePath(fileName), data);
                SaveCfg();
                return Status.Success;
            }

            int chunkSize = VFSConfig.instance.chunkSize;
            // 计算需要写入的块数  
            int totalChunks = (data.Length + chunkSize - 1) / chunkSize;
            // 遍历每个块并写入MemoryStream  
            for (int i = 0; i < totalChunks; i++)
            {
                NFSFileOptions options = optList.FirstOrDefault(x => x.state == 0);
                if (options is null)
                {
                    return Status.Fail;
                }

                // 计算当前块的起始索引  
                int startIndex = i * chunkSize;
                // 计算当前块要写入的字节数，注意避免越界  
                int bytesToWrite = Math.Min(options.length, data.Length - startIndex);
                // 定位到文件中的指定偏移量  
                stream.Seek(options.offset, SeekOrigin.Begin);
                // 将当前块数据写入文件  
                options.Use(i, fileName, bytesToWrite, version);
                await stream.WriteAsync(data, startIndex, bytesToWrite);
            }

            await stream.FlushAsync();
            SaveCfg();
            return Status.Success;
        }

        /// <summary>
        /// 读取文件二进制数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件二进制数据，如果读取失败则返回</returns>
        public byte[] Read(string fileName)
        {
            if (VFSConfig.instance.enable is false)
            {
                return File.ReadAllBytes(GameFrameworkEntry.GetApplicationFilePath(fileName));
            }

            List<NFSFileOptions> opts = optList.Where(x => x.name == fileName).ToList();
            if (opts is null || opts.Count == 0)
            {
                return Array.Empty<byte>();
            }

            opts.Sort((x, y) => x.offset.CompareTo(y.offset));
            byte[] bytes = new byte[opts.Sum(x => x.dataLength)];
            int offset = 0;
            for (int i = 0; i < opts.Count; i++)
            {
                NFSFileOptions options = opts[i];
                stream.Seek(options.offset, SeekOrigin.Begin);
                stream.Read(bytes, offset, options.dataLength);
                offset += options.dataLength;
            }

            return bytes;
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件二进制数据，如果读取文件失败则返回空数据</returns>
        public async UniTask<byte[]> ReadAsync(string fileName)
        {
            if (VFSConfig.instance.enable is false)
            {
                return await File.ReadAllBytesAsync(GameFrameworkEntry.GetApplicationFilePath(fileName));
            }

            List<NFSFileOptions> opts = optList.Where(x => x.name == fileName).ToList();
            if (opts is null || opts.Count == 0)
            {
                return Array.Empty<byte>();
            }

            opts.Sort((x, y) => x.offset.CompareTo(y.offset));
            byte[] bytes = new byte[opts.Sum(x => x.dataLength)];
            int offset = 0;
            for (int i = 0; i < opts.Count; i++)
            {
                NFSFileOptions options = opts[i];
                stream.Seek(options.offset, SeekOrigin.Begin);
                await stream.ReadAsync(bytes, offset, options.dataLength);
                offset += options.dataLength;
            }

            return bytes;
        }
    }
}