using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ZGame.FileSystem
{
    public class FileManager : IManager
    {
        private List<VFSData> segments = new List<VFSData>();
        private List<IOHandle> ioList = new List<IOHandle>();

        private int sizeLimit = 1024 * 1024 * 5;
        private int countLimit = 20;


        public FileManager()
        {
            string filePath = Application.persistentDataPath + "/vfs.ini";
            if (!File.Exists(filePath))
            {
                return;
            }

            segments = JsonConvert.DeserializeObject<List<VFSData>>(File.ReadAllText(filePath));
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
            IEnumerable<VFSData> fileData = segments.Where(x => x.name == fileName);
            foreach (var file in fileData)
            {
                file.Free();
            }

            Saved();
        }

        public bool EnsureFileVersion(string fileName, int version)
        {
            VFSData vfsData = segments.Find(x => x.name == fileName);
            if (vfsData is null)
            {
                return false;
            }

            return vfsData.version == version;
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

        private IOHandle GetFileHandle(string vfs)
        {
            IOHandle handle = ioList.Find(x => x.name == vfs);
            if (handle is null)
            {
                ioList.Add(handle = new IOHandle()
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
        public VFSData[] GetFreeSgement(int lenght)
        {
            List<VFSData> result = new List<VFSData>();
            int count = lenght / sizeLimit;
            count = lenght > count * sizeLimit ? count + 1 : count;
            IEnumerable<VFSData> temp = segments.Where(x => x.use is false);

            if (temp is null || temp.Count() < count)
            {
                IOHandle handle = GetFileHandle(Guid.NewGuid().ToString().Replace("-", String.Empty));
                for (int i = 0; i < countLimit; i++)
                {
                    segments.Add(new VFSData(handle.name, sizeLimit, i * sizeLimit)
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

        public VFSData[] GetFileData(string fileName)
        {
            return segments.Where(x => x.name == fileName).ToArray();
        }

        public IWriteFileResult Write(string fileName, byte[] bytes, int version)
        {
            Delete(fileName);
            VFSData[] vfsDataList = GetFreeSgement(bytes.Length);
            if (vfsDataList is null || vfsDataList.Length == 0)
            {
                return default;
            }

            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                IOHandle handle = GetFileHandle(vfsDataList[0].vfs);
                if (handle is null)
                {
                    return default;
                }

                int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Use(fileName, length, index, DateTimeOffset.Now.ToUnixTimeMilliseconds(), version);
                handle.Write(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.length;
                index++;
            }

            Saved();
            return IWriteFileResult.Create(fileName, bytes, version);
        }

        public async UniTask<IWriteFileResult> WriteAsync(string fileName, byte[] bytes, int version)
        {
            Delete(fileName);
            VFSData[] vfsDataList = GetFreeSgement(bytes.Length);
            if (vfsDataList is null || vfsDataList.Length == 0)
            {
                return default;
            }

            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                IOHandle handle = GetFileHandle(vfsDataList[0].vfs);
                if (handle is null)
                {
                    return default;
                }

                int lenght = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Use(fileName, lenght, index, DateTimeOffset.Now.ToUnixTimeMilliseconds(), version);
                await handle.WriteAsync(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.length;
                index++;
            }

            Saved();
            return IWriteFileResult.Create(fileName, bytes, version);
        }

        public IReadFileResult Read(string fileName)
        {
            if (Exsit(fileName) is false)
            {
                return default;
            }

            VFSData[] vfsDatas = GetFileData(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                return null;
            }

            int offset = 0;
            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                IOHandle handle = GetFileHandle(VARIABLE.vfs);
                if (handle is null)
                {
                    continue;
                }

                handle.Read(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.fileLenght;
            }

            return IReadFileResult.Create(fileName, bytes, vfsDatas[0].time, vfsDatas[0].version);
        }

        public async UniTask<IReadFileResult> ReadAsync(string fileName)
        {
            if (Exsit(fileName) is false)
            {
                return default;
            }

            VFSData[] vfsDatas = GetFileData(fileName);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                return default;
            }

            int offset = 0;
            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            foreach (var VARIABLE in vfsDatas)
            {
                IOHandle handle = GetFileHandle(VARIABLE.vfs);
                if (handle is null)
                {
                    continue;
                }

                await handle.ReadAsync(VARIABLE.offset, bytes, offset, VARIABLE.fileLenght);
                offset += VARIABLE.fileLenght;
            }

            return IReadFileResult.Create(fileName, bytes, vfsDatas[0].time, vfsDatas[0].version);
        }
    }
}