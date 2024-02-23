using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Config;

namespace ZGame.FileSystem
{
    public class VFSManager : Singleton<VFSManager>
    {
        private List<VFSChunk> chunkList = new List<VFSChunk>();
        private List<VFStream> vfStreamList = new List<VFStream>();


        public override void Dispose()
        {
            vfStreamList.ForEach(x => x.Dispose());
            Saved();
        }

        protected override void OnAwake()
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

        private VFStream OpenStream(string vfs)
        {
            VFStream handle = vfStreamList.Find(x => x.name == vfs);
            if (handle is null)
            {
                vfStreamList.Add(handle = new VFStream(Application.persistentDataPath + "/" + vfs));
            }

            return handle;
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
        /// 获取未使用的VFS数据块，如果不存在未使用的数据块，则创建一个新的VFS
        /// </summary>
        /// <param name="lenght">指定VFS数据块大小</param>
        /// <returns></returns>
        public VFSChunk[] FindFreeChunkList(int lenght)
        {
            List<VFSChunk> result = new List<VFSChunk>();
            int chunkSize = BasicConfig.instance.vfsConfig.chunkSize;
            int maxCount = BasicConfig.instance.vfsConfig.chunkCount;
            int count = MathEx.MaxSharinCount(lenght, chunkSize);
            IEnumerable<VFSChunk> temp = chunkList.Where(x => x.use is false);
            if (temp is null || temp.Count() < count)
            {
                VFStream handle = OpenStream(Guid.NewGuid().ToString().Replace("-", String.Empty));
                for (int i = 0; i < maxCount; i++)
                {
                    chunkList.Add(new VFSChunk(handle.name, chunkSize, i * chunkSize));
                }

                temp = chunkList.Where(x => x.use is false);
            }

            for (int i = 0; i < count; i++)
            {
                result.Add(temp.ElementAt(i));
            }

            return result.ToArray();
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
            VFSChunk[] vfsDataList = FindFreeChunkList(bytes.Length);
            if (vfsDataList is null || vfsDataList.Length == 0)
            {
                return;
            }

            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                VFStream handle = OpenStream(vfsDataList[0].vfs);
                if (handle is null)
                {
                    return;
                }

                int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Use(fileName, length, index, version);
                handle.Write(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.length;
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
            VFSChunk[] vfsDataList = FindFreeChunkList(bytes.Length);
            if (vfsDataList is null || vfsDataList.Length == 0)
            {
                return;
            }

            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                VFStream handle = OpenStream(vfsDataList[0].vfs);
                if (handle is null)
                {
                    return;
                }

                int lenght = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Use(fileName, lenght, index, version);
                await handle.WriteAsync(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.length;
                index++;
            }

            Debug.Log("write file: " + fileName);
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
            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                VFStream handle = OpenStream(VARIABLE.vfs);
                if (handle is null)
                {
                    continue;
                }

                handle.Read(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.fileLenght;
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
            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                VFStream handle = OpenStream(VARIABLE.vfs);
                if (handle is null)
                {
                    continue;
                }

                await handle.ReadAsync(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.fileLenght;
            }

            return bytes;
        }
    }
}