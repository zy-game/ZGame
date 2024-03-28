using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Config;

namespace ZGame.VFS
{
    /// <summary>
    /// 虚拟磁盘
    /// </summary>
    class VFSDisk : IReferenceObject
    {
        class DirtOpt
        {
            public string name;
            public List<FileOpt> files;
        }

        class FileOpt
        {
            /// <summary>
            /// 文件名
            /// </summary>
            public string name;

            /// <summary>
            /// 数据长度
            /// </summary>
            public int length;

            /// <summary>
            /// 数去读取起始偏移
            /// </summary>
            public int offset;
        }

        private List<VFSDirctory> _files;
        private List<DirtOpt> _dirtOpts;

        public void Release()
        {
        }

        public static VFSDisk OpenOrCreateDisk(string name)
        {
            VFSDisk disk = GameFrameworkFactory.Spawner<VFSDisk>();
            string path = GameFrameworkEntry.GetApplicationFilePath(name);
            if (File.Exists(path) is false)
            {
                disk._dirtOpts = JsonConvert.DeserializeObject<List<DirtOpt>>(path + ".ini");
            }

            return default;
        }
    }

    class VFSDirctory : IReferenceObject
    {
        private List<VFSFile> _files;

        public void Release()
        {
        }
    }

    /// <summary>
    /// 虚拟文件对象
    /// </summary>
    class VFSFile : IReferenceObject
    {
        public string name;
        public uint version;

        public void Release()
        {
        }

        public static byte[] ReadAllBytes(string path)
        {
            return default;
        }

        public static string ReadAllText(string path)
        {
            return default;
        }

        public static void WriteAllBytes(string path, byte[] bytes, uint version)
        {
        }

        public static void WriteAllText(string path, string text, uint version)
        {
        }
    }

    /// <summary>
    /// 虚拟文件系统
    /// </summary>
    public class VFSManager : GameFrameworkModule
    {
        private VFSDisk _disk;
        private string vfsFileName;
        private List<VFSChunk> chunkList = new List<VFSChunk>();
        private List<VFStream> vfStreamList = new List<VFStream>();

        //todo  将所有文件或文件的操封装到这里

        public override void OnAwake(params object[] args)
        {
            _disk = VFSDisk.OpenOrCreateDisk(GameConfig.instance.title + " virtual data.disk");
            vfsFileName = $"{Application.persistentDataPath}/{MDFive.MD5Encrypt16("vfs.ini")}";
            if (!File.Exists(vfsFileName))
            {
                return;
            }

            chunkList = JsonConvert.DeserializeObject<List<VFSChunk>>(File.ReadAllText(vfsFileName));
        }

        public override void Release()
        {
            vfStreamList.ForEach(x => x.Dispose());
            Saved();
        }

        /// <summary>
        /// 保存VFS数据
        /// </summary>
        private void Saved()
        {
            File.WriteAllText(vfsFileName, JsonConvert.SerializeObject(chunkList));
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
                for (int i = 0; i < VFSConfig.instance.chunkCount; i++)
                {
                    chunkList.Add(new VFSChunk(vfs, VFSConfig.instance.chunkSize, i * VFSConfig.instance.chunkSize));
                }

                freeChunk = GetFreeChunk();
            }

            return freeChunk;
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
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Delete(string fileName)
        {
            IEnumerable<VFSChunk> fileData = chunkList.Where(x => x.name == fileName);
            foreach (var file in fileData)
            {
                if (VFSConfig.instance.enable is false)
                {
                    File.Delete(GameFrameworkEntry.GetApplicationFilePath(fileName));
                }

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
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public Status Write(string fileName, byte[] bytes, uint version)
        {
            Delete(fileName);
            Status status = Status.Success;
            if (VFSConfig.instance.enable is false)
            {
                try
                {
                    VFSChunk chunk = GetFreeChunk();
                    File.WriteAllBytes(GameFrameworkEntry.GetApplicationFilePath(fileName), bytes);
                    chunk.Use(fileName, bytes.Length, 0, version);
                }
                catch (Exception e)
                {
                    GameFrameworkEntry.Logger.LogError(e);
                }

                status = Status.Fail;
            }
            else
            {
                List<VFSChunk> useChunkList = new List<VFSChunk>();
                int chunkSize = VFSConfig.instance.chunkSize;
                int maxCount = Extension.MaxSharinCount(bytes.Length, chunkSize);
                for (int i = 0; i < maxCount; i++)
                {
                    VFSChunk chunk = GetFreeChunk();
                    if (chunk is null)
                    {
                        GameFrameworkEntry.Logger.LogError("没有可用的VFS数据块");
                        break;
                    }

                    VFStream stream = GetStream(chunk.vfs);
                    if (stream is null)
                    {
                        GameFrameworkEntry.Logger.LogError(new FileNotFoundException(chunk.vfs));
                        break;
                    }

                    int length = bytes.Length - i * chunkSize > chunk.length ? chunk.length : bytes.Length - i * chunkSize;
                    status = stream.Write(chunk.offset, bytes, i * chunkSize, length);
                    if (status is not Status.Success)
                    {
                        GameFrameworkEntry.Logger.LogError("写入文件数据失败");
                        break;
                    }

                    useChunkList.Add(chunk);
                    chunk.Use(fileName, length, i, version);
                }

                if (status is not Status.Success)
                {
                    chunkList.ForEach(x => x.Free());
                }
            }

            Saved();
            return status;
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public async UniTask<Status> WriteAsync(string fileName, byte[] bytes, uint version, CancellationTokenSource cancellationTokenSource = null)
        {
            Delete(fileName);
            Status status = Status.Success;
            using (CancellationTokenSourceGroup cancellationTokenSourceGroup = new CancellationTokenSourceGroup(cancellationTokenSource))
            {
                if (VFSConfig.instance.enable is false)
                {
                    try
                    {
                        VFSChunk chunk = GetFreeChunk();
                        await File.WriteAllBytesAsync(GameFrameworkEntry.GetApplicationFilePath(fileName), bytes, cancellationTokenSourceGroup.GetToken());
                        chunk.Use(fileName, bytes.Length, 0, version);
                    }
                    catch (Exception e)
                    {
                        GameFrameworkEntry.Logger.LogError(e);
                        status = Status.Fail;
                    }
                }
                else
                {
                    int chunkSize = VFSConfig.instance.chunkSize;
                    int maxCount = Extension.MaxSharinCount(bytes.Length, chunkSize);
                    UniTask<Status>[] writeTasks = new UniTask<Status>[maxCount];
                    List<VFSChunk> chunkList = new List<VFSChunk>();
                    for (int i = 0; i < maxCount; i++)
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

                        int length = bytes.Length - i * chunkSize > chunk.length ? chunk.length : bytes.Length - i * chunkSize;
                        writeTasks[i] = stream.WriteAsync(chunk.offset, bytes, i * chunkSize, length, cancellationTokenSourceGroup.GetToken());
                        chunk.Use(fileName, length, i, version);
                        chunkList.Add(chunk);
                    }

                    Status[] writeResult = await UniTask.WhenAll(writeTasks);
                    if (writeResult.All(x => x == Status.Success) is false)
                    {
                        Debug.LogError("写入文件失败：" + fileName);
                        status = Status.Fail;
                        chunkList.ForEach(x => x.Free());
                    }
                }

                Saved();
            }

            return status;
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

            if (VFSConfig.instance.enable is false)
            {
                return File.ReadAllBytes(GameFrameworkEntry.GetApplicationFilePath(fileName));
            }
            else
            {
                VFSChunk[] vfsDatas = FindFileChunkList(fileName);
                if (vfsDatas is null || vfsDatas.Length is 0)
                {
                    throw new FileNotFoundException(fileName);
                }

                byte[] bytes = new byte[vfsDatas.Sum(x => x.useLenght)];
                foreach (var VARIABLE in vfsDatas)
                {
                    VFStream handle = GetStream(VARIABLE.vfs);
                    if (handle is null)
                    {
                        return Array.Empty<byte>();
                    }

                    int offset = VARIABLE.sort * VFSConfig.instance.chunkSize;
                    handle.Read(VARIABLE.offset, bytes, VARIABLE.sort * VFSConfig.instance.chunkSize, VARIABLE.useLenght);
                }

                return bytes;
            }
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async UniTask<byte[]> ReadAsync(string fileName, CancellationTokenSource cancellationTokenSource = null)
        {
            if (Exsit(fileName) is false)
            {
                return Array.Empty<byte>();
            }

            using (CancellationTokenSourceGroup cancellationTokenSourceGroup = new CancellationTokenSourceGroup(cancellationTokenSource))
            {
                if (VFSConfig.instance.enable is false)
                {
                    return await File.ReadAllBytesAsync(GameFrameworkEntry.GetApplicationFilePath(fileName), cancellationTokenSourceGroup.GetToken());
                }

                VFSChunk[] chunkList = FindFileChunkList(fileName);
                if (chunkList is null || chunkList.Length is 0)
                {
                    return Array.Empty<byte>();
                }

                byte[] bytes = new byte[chunkList.Sum(x => x.useLenght)];
                UniTask<Status>[] readTasks = new UniTask<Status>[chunkList.Length];
                for (int i = 0; i < chunkList.Length; i++)
                {
                    VFStream handle = GetStream(chunkList[i].vfs);
                    if (handle is null)
                    {
                        readTasks[i] = UniTask.FromResult(Status.Fail);
                        GameFrameworkEntry.Logger.LogError(new FileNotFoundException(chunkList[i].vfs));
                        break;
                    }

                    int offset = chunkList[i].sort * VFSConfig.instance.chunkSize;
                    readTasks[i] = handle.ReadAsync(chunkList[i].offset, bytes, offset, chunkList[i].useLenght, cancellationTokenSourceGroup.GetToken());
                }

                Status[] readResult = await UniTask.WhenAll(readTasks);
                if (readResult.All(x => x == Status.Success) is false)
                {
                    GameFrameworkEntry.Logger.LogError("读取文件数据失败：" + fileName);
                    return Array.Empty<byte>();
                }

                return bytes;
            }
        }
    }
}