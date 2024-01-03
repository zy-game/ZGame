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
        private List<VFSChunk> segments = new List<VFSChunk>();
        private List<VFSStream> ioList = new List<VFSStream>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ioList.ForEach(x => x.Dispose());
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            string filePath = Application.persistentDataPath + "/vfs.ini";
            if (!File.Exists(filePath))
            {
                return;
            }

            segments = JsonConvert.DeserializeObject<List<VFSChunk>>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// 保存VFS数据
        /// </summary>
        private void Saved()
        {
            string filePath = Application.persistentDataPath + "/vfs.ini";
            File.WriteAllText(filePath, JsonConvert.SerializeObject(segments));
        }

        public void Delete(string fileName)
        {
            IEnumerable<VFSChunk> fileData = segments.Where(x => x.name == fileName);
            foreach (var file in fileData)
            {
                file.Free();
            }

            Saved();
        }

        public bool Exist(string fileName, uint version)
        {
            VFSChunk vfsChunk = segments.Find(x => x.name == fileName);
            if (vfsChunk is null)
            {
                return false;
            }

            return vfsChunk.version == version;
        }

        public bool Exsit(string fileName)
        {
            return segments.Find(x => x.name == fileName) is not null;
        }

        public void Dispose()
        {
            ioList.ForEach(x => x.Dispose());
            Saved();
        }

        private VFSStream GetFileHandle(string vfs)
        {
            VFSStream handle = ioList.Find(x => x.name == vfs);
            if (handle is null)
            {
                ioList.Add(handle = new VFSStream()
                {
                    name = vfs,
                    time = Time.realtimeSinceStartup,
                    fileStream = new FileStream(Application.persistentDataPath + "/" + vfs, FileMode.OpenOrCreate, FileAccess.ReadWrite)
                });
            }

            handle.time = Time.realtimeSinceStartup + 60;
            return handle;
        }


        /// <summary>
        /// 获取未使用的VFS数据块，如果不存在未使用的数据块，则创建一个新的VFS
        /// </summary>
        /// <param name="lenght">指定VFS数据块大小，如果不指定则使用配置大小</param>
        /// <returns></returns>
        public VFSChunk[] GetFreeSgement(int lenght)
        {
            List<VFSChunk> result = new List<VFSChunk>();
            int chunkSize = BasicConfig.instance.vfsConfig.chunkSize;
            int maxCount = BasicConfig.instance.vfsConfig.chunkCount;
            int count = lenght / chunkSize;
            count = lenght > count * chunkSize ? count + 1 : count;
            IEnumerable<VFSChunk> temp = segments.Where(x => x.use is false);

            if (temp is null || temp.Count() < count)
            {
                VFSStream handle = GetFileHandle(Guid.NewGuid().ToString().Replace("-", String.Empty));
                for (int i = 0; i < maxCount; i++)
                {
                    segments.Add(new VFSChunk(handle.name, chunkSize, i * chunkSize)
                    {
                        time = DateTimeOffset.Now.ToUnixTimeSeconds()
                    });
                }

                temp = segments.Where(x => x.use is false);
            }

            for (int i = 0; i < count; i++)
            {
                result.Add(temp.ElementAt(i));
            }

            return result.ToArray();
        }

        public VFSChunk[] GetFileData(string fileName)
        {
            return segments.Where(x => x.name == fileName).ToArray();
        }

        public void Write(string fileName, byte[] bytes, uint version)
        {
            Delete(fileName);
            VFSChunk[] vfsDataList = GetFreeSgement(bytes.Length);
            if (vfsDataList is null || vfsDataList.Length == 0)
            {
                return;
            }

            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                VFSStream handle = GetFileHandle(vfsDataList[0].vfs);
                if (handle is null)
                {
                    return;
                }

                int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Use(fileName, length, index, DateTimeOffset.Now.ToUnixTimeMilliseconds(), version);
                handle.Write(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.length;
                index++;
            }

            Saved();
        }

        public async UniTask WriteAsync(string fileName, byte[] bytes, uint version)
        {
            Delete(fileName);
            VFSChunk[] vfsDataList = GetFreeSgement(bytes.Length);
            if (vfsDataList is null || vfsDataList.Length == 0)
            {
                return;
            }

            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                VFSStream handle = GetFileHandle(vfsDataList[0].vfs);
                if (handle is null)
                {
                    return;
                }

                int lenght = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Use(fileName, lenght, index, DateTimeOffset.Now.ToUnixTimeMilliseconds(), version);
                await handle.WriteAsync(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.length;
                index++;
            }

            Debug.Log("write file: " + fileName);
            Saved();
        }

        public byte[] Read(string fileName)
        {
            if (Exsit(fileName) is false)
            {
                return Array.Empty<byte>();
            }

            VFSChunk[] vfsDatas = GetFileData(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                return Array.Empty<byte>();
            }

            int offset = 0;
            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                VFSStream handle = GetFileHandle(VARIABLE.vfs);
                if (handle is null)
                {
                    continue;
                }

                handle.Read(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.fileLenght;
            }

            return bytes;
        }

        public async UniTask<byte[]> ReadAsync(string fileName)
        {
            if (Exsit(fileName) is false)
            {
                return Array.Empty<byte>();
            }

            VFSChunk[] vfsDatas = GetFileData(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                return Array.Empty<byte>();
            }

            int offset = 0;
            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                VFSStream handle = GetFileHandle(VARIABLE.vfs);
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