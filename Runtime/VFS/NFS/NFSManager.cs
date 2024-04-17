using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ZGame.VFS
{
    public interface INFSClient : IReference
    {
        byte[] Download(string path);
        UniTask<byte[]> DownloadAsync(string path);
    }

    partial class NFSManager
    {
        public static NFSManager OpenOrCreateDisk(string name)
        {
            NFSManager disk = RefPooled.Spawner<NFSManager>();
            disk.name = name;
            disk._bundles = new ConcurrentStack<NFSDirctory>();
            CoreAPI.Logger.Log(name);
            CoreAPI.Logger.Log(PlayerPrefs.GetString(name, "[]"));
            List<string> vfsList = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString(name, "[]"));
            foreach (string n in vfsList)
            {
                disk._bundles.Push(NFSDirctory.OpenOrCreateBundle(n));
            }

            return disk;
        }
    }

    /// <summary>
    /// 虚拟磁盘
    /// </summary>
    partial class NFSManager : IReference
    {
        public string name;
        private INFSClient _client;
        private ConcurrentStack<NFSDirctory> _bundles;

        public void Release()
        {
            foreach (var VARIABLE in _bundles)
            {
                RefPooled.Release(VARIABLE);
            }

            _bundles.Clear();
        }

        void SaveCfg()
        {
            string cfg = JsonConvert.SerializeObject(_bundles.Select(x => x.name).ToArray());
            PlayerPrefs.SetString(this.name, cfg);
            CoreAPI.Logger.Log(cfg);
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns></returns>
        public bool Exists(string name)
        {
            return _bundles.FirstOrDefault(x => x.Exists(name)) != null;
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="version">文件版本</param>
        /// <returns></returns>
        public bool Exists(string name, uint version)
        {
            return _bundles.FirstOrDefault(x => x.Exists(name, version)) != null;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="name">文件名</param>
        public void Delete(string name)
        {
            NFSDirctory bundle = _bundles.FirstOrDefault(x => x.Exists(name));
            if (bundle is null)
            {
                return;
            }

            bundle.Delete(name);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件二进制数据</param>
        /// <param name="version">文件版本</param>
        /// <returns></returns>
        public Status Write(string name, byte[] data, uint version)
        {
            Delete(name);
            NFSDirctory bundle = _bundles.FirstOrDefault(x => x.writeable >= data.Length);
            if (bundle is null)
            {
                _bundles.Push(bundle = NFSDirctory.OpenOrCreateBundle(Guid.NewGuid().ToString().Replace("-", string.Empty)));
                SaveCfg();
            }

            return bundle.Write(name, data, version);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件二进制数据</param>
        /// <param name="version">文件版本</param>
        /// <returns></returns>
        public UniTask<Status> WriteAsync(string name, byte[] data, uint version)
        {
            Delete(name);
            NFSDirctory bundle = _bundles.FirstOrDefault(x => x.writeable >= data.Length);
            if (bundle is null)
            {
                _bundles.Push(bundle = NFSDirctory.OpenOrCreateBundle(Guid.NewGuid().ToString().Replace("-", string.Empty)));
                SaveCfg();
            }

            return bundle.WriteAsync(name, data, version);
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件二进制数据，如果读取失败将会返回空数据</returns>
        public byte[] Read(string fileName)
        {
            if (Exists(fileName) is false)
            {
            }

            NFSDirctory bundle = _bundles.FirstOrDefault(x => x.Exists(fileName));
            if (bundle is null)
            {
                return Array.Empty<byte>();
            }

            return bundle.Read(fileName);
        }

        /// <summary>
        /// 读取文件数据，如果本地不存在则从网络下载
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件二进制数据</returns>
        public async UniTask<byte[]> ReadAsync(string fileName)
        {
            if (Exists(fileName) is false)
            {
                return Array.Empty<byte>();
            }

            NFSDirctory bundle = _bundles.FirstOrDefault(x => x.Exists(fileName));
            if (bundle is null)
            {
                return Array.Empty<byte>();
            }

            return await bundle.ReadAsync(fileName);
        }
    }
}