using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Config;
using ZGame.Module;

namespace ZGame.FileSystem
{
    public class VFSManager : IModule
    {
        private List<VFSChunk> chunkList = new List<VFSChunk>();
        private List<VFStream> vfStreamList = new List<VFStream>();


        public void Dispose()
        {
            vfStreamList.ForEach(x => x.Dispose());
            Saved();
        }

        public void OnAwake()
        {
            string filePath = Application.persistentDataPath + "/vfs.ini";
            if (!File.Exists(filePath))
            {
                return;
            }

            chunkList = JsonConvert.DeserializeObject<List<VFSChunk>>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// 保存VFS数据
        /// </summary>
        private void Saved()
        {
            string filePath = Application.persistentDataPath + "/vfs.ini";
            File.WriteAllText(filePath, JsonConvert.SerializeObject(chunkList));
        }

        private VFStream GetStream(string vfs)
        {
            VFStream handle = vfStreamList.Find(x => x.name == vfs);
            if (handle is null)
            {
                if (File.Exists(Application.persistentDataPath + "/" + vfs) is false)
                {
                    return default;
                }

                vfStreamList.Add(handle = new VFStream(Application.persistentDataPath + "/" + vfs));
            }

            return handle;
        }

        private VFSChunk GetFreeChunk()
        {
            VFSChunk freeChunk = chunkList.Find(x => x.use == false);
            if (freeChunk is null)
            {
                string vfs = Guid.NewGuid().ToString().Replace("-", String.Empty);
                vfStreamList.Add(new VFStream(Application.persistentDataPath + "/" + vfs));
                for (int i = 0; i < BasicConfig.instance.vfsConfig.chunkCount; i++)
                {
                    chunkList.Add(new VFSChunk(vfs, BasicConfig.instance.vfsConfig.chunkSize, i * BasicConfig.instance.vfsConfig.chunkSize));
                }

                freeChunk = GetFreeChunk();
            }

            return freeChunk;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Delete(string fileName)
        {
            IEnumerable<VFSChunk> fileData = chunkList.Where(x => x.name == fileName);
            foreach (var file in fileData)
            {
                file.Free();
            }

            Saved();
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool Exist(string fileName, uint version)
        {
            VFSChunk vfsChunk = chunkList.Find(x => x.name == fileName);
            if (vfsChunk is null)
            {
                return false;
            }

            return vfsChunk.version == version;
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Exsit(string fileName)
        {
            return chunkList.Find(x => x.name == fileName) is not null;
        }


        /// <summary>
        /// 获取文件占用的VFS数据块
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public VFSChunk[] FindFileChunkList(string fileName)
        {
            return chunkList.Where(x => x.name == fileName).ToArray();
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public void Write(string fileName, byte[] bytes, uint version)
        {
            Delete(fileName);
            int offset = 0;
            int index = 0;
            while (offset < bytes.Length)
            {
                VFSChunk chunk = GetFreeChunk();
                if (chunk is null)
                {
                    throw new Exception("没有可用的VFS数据块");
                }

                VFStream stream = GetStream(chunk.vfs);
                if (stream is null)
                {
                    throw new FileNotFoundException(chunk.vfs);
                }

                int length = bytes.Length - offset > chunk.length ? chunk.length : bytes.Length - offset;
                stream.Write(chunk.offset, bytes, offset, length);
                chunk.Use(fileName, length, index, version);
                offset += length;
                index++;
            }

            Saved();
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public async UniTask WriteAsync(string fileName, byte[] bytes, uint version)
        {
            Delete(fileName);
            int offset = 0;
            int index = 0;
            while (offset < bytes.Length)
            {
                VFSChunk chunk = GetFreeChunk();
                if (chunk is null)
                {
                    throw new Exception("没有可用的VFS数据块");
                }

                VFStream stream = GetStream(chunk.vfs);
                if (stream is null)
                {
                    throw new FileNotFoundException(chunk.vfs);
                }

                int lenght = bytes.Length - offset > chunk.length ? chunk.length : bytes.Length - offset;
                chunk.Use(fileName, lenght, index, version);
                await stream.WriteAsync(chunk.offset, bytes, offset, chunk.useLenght);
                offset += chunk.useLenght;
                index++;
            }

            Debug.Log("write file: " + fileName + " Lenght:" + bytes.Length + " ChunkCount:" + index);
            Saved();
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] Read(string fileName)
        {
            if (Exsit(fileName) is false)
            {
                return Array.Empty<byte>();
            }

            VFSChunk[] vfsDatas = FindFileChunkList(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                return Array.Empty<byte>();
            }

            int offset = 0;
            byte[] bytes = new byte[vfsDatas.Sum(x => x.useLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                VFStream handle = GetStream(VARIABLE.vfs);
                if (handle is null)
                {
                    throw new FileNotFoundException(VARIABLE.vfs);
                }

                handle.Read(VARIABLE.offset, bytes, offset, VARIABLE.useLenght);
                offset += VARIABLE.useLenght;
            }

            return bytes;
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async UniTask<byte[]> ReadAsync(string fileName)
        {
            if (Exsit(fileName) is false)
            {
                return Array.Empty<byte>();
            }

            VFSChunk[] vfsDatas = FindFileChunkList(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                return Array.Empty<byte>();
            }

            int offset = 0;
            byte[] bytes = new byte[vfsDatas.Sum(x => x.useLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                VFStream handle = GetStream(VARIABLE.vfs);
                if (handle is null)
                {
                    throw new FileNotFoundException(VARIABLE.vfs);
                }

                await handle.ReadAsync(VARIABLE.offset, bytes, offset, VARIABLE.useLenght);
                offset += VARIABLE.useLenght;
            }

            return bytes;
        }
    }
}