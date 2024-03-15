using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Config;
using ZGame.Module;

namespace ZGame.FileSystem
{
    public class VFSManager : IModule
    {
        private string vfsFileName;
        private List<VFSChunk> chunkList = new List<VFSChunk>();
        private List<VFStream> vfStreamList = new List<VFStream>();

        public void Dispose()
        {
            vfStreamList.ForEach(x => x.Dispose());
            Saved();
        }

        public void OnAwake()
        {
            vfsFileName = $"{Application.persistentDataPath}/{MDFive.MD5Encrypt16("vfs.ini")}";
            if (!File.Exists(vfsFileName))
            {
                return;
            }

            chunkList = JsonConvert.DeserializeObject<List<VFSChunk>>(File.ReadAllText(vfsFileName));
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
                for (int i = 0; i < BasicConfig.instance.vfsConfig.chunkCount; i++)
                {
                    chunkList.Add(new VFSChunk(vfs, BasicConfig.instance.vfsConfig.chunkSize, i * BasicConfig.instance.vfsConfig.chunkSize));
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
                if (WorkApi.IsEnableVirtualFileSystem is false)
                {
                    File.Delete(WorkApi.GetApplicationFilePath(fileName));
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
        public ResultStatus Write(string fileName, byte[] bytes, uint version)
        {
            Delete(fileName);
            ResultStatus resultStatus = ResultStatus.Success;
            if (WorkApi.IsEnableVirtualFileSystem is false)
            {
                try
                {
                    VFSChunk chunk = GetFreeChunk();
                    File.WriteAllBytes(WorkApi.GetApplicationFilePath(fileName), bytes);
                    chunk.Use(fileName, bytes.Length, 0, version);
                }
                catch (Exception e)
                {
                    WorkApi.Logger.LogError(e);
                }

                resultStatus = ResultStatus.Fail;
            }
            else
            {
                List<VFSChunk> useChunkList = new List<VFSChunk>();
                int chunkSize = BasicConfig.instance.vfsConfig.chunkSize;
                int maxCount = MathEx.MaxSharinCount(bytes.Length, chunkSize);
                for (int i = 0; i < maxCount; i++)
                {
                    VFSChunk chunk = GetFreeChunk();
                    if (chunk is null)
                    {
                        WorkApi.Logger.LogError("没有可用的VFS数据块");
                        break;
                    }

                    VFStream stream = GetStream(chunk.vfs);
                    if (stream is null)
                    {
                        WorkApi.Logger.LogError(new FileNotFoundException(chunk.vfs));
                        break;
                    }

                    int length = bytes.Length - i * chunkSize > chunk.length ? chunk.length : bytes.Length - i * chunkSize;
                    resultStatus = stream.Write(chunk.offset, bytes, i * chunkSize, length);
                    if (resultStatus != ResultStatus.Success)
                    {
                        WorkApi.Logger.LogError("写入文件数据失败");
                        break;
                    }

                    useChunkList.Add(chunk);
                    chunk.Use(fileName, length, i, version);
                }

                if (resultStatus is not ResultStatus.Success)
                {
                    chunkList.ForEach(x => x.Free());
                }
            }

            Saved();
            return resultStatus;
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public async UniTask<ResultStatus> WriteAsync(string fileName, byte[] bytes, uint version, CancellationTokenSource cancellationTokenSource = null)
        {
            Delete(fileName);
            ResultStatus resultStatus = ResultStatus.Success;
            using (CancellationTokenSourceGroup cancellationTokenSourceGroup = new CancellationTokenSourceGroup(cancellationTokenSource))
            {
                if (WorkApi.IsEnableVirtualFileSystem is false)
                {
                    try
                    {
                        VFSChunk chunk = GetFreeChunk();
                        await File.WriteAllBytesAsync(WorkApi.GetApplicationFilePath(fileName), bytes, cancellationTokenSourceGroup.GetToken());
                        chunk.Use(fileName, bytes.Length, 0, version);
                    }
                    catch (Exception e)
                    {
                        WorkApi.Logger.LogError(e);
                        resultStatus = ResultStatus.Fail;
                    }
                }
                else
                {
                    int chunkSize = BasicConfig.instance.vfsConfig.chunkSize;
                    int maxCount = MathEx.MaxSharinCount(bytes.Length, chunkSize);
                    UniTask<ResultStatus>[] writeTasks = new UniTask<ResultStatus>[maxCount];
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

                    ResultStatus[] writeResult = await UniTask.WhenAll(writeTasks);
                    if (writeResult.All(x => x == ResultStatus.Success) is false)
                    {
                        Debug.LogError("写入文件失败：" + fileName);
                        resultStatus = ResultStatus.Fail;
                        chunkList.ForEach(x => x.Free());
                    }
                }

                Saved();
            }

            return resultStatus;
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

            if (WorkApi.IsEnableVirtualFileSystem is false)
            {
                return File.ReadAllBytes(WorkApi.GetApplicationFilePath(fileName));
            }
            else
            {
                VFSChunk[] vfsDatas = FindFileChunkList(fileName);
                if (vfsDatas is null || vfsDatas.Length is 0)
                {
                    return Array.Empty<byte>();
                }

                byte[] bytes = new byte[vfsDatas.Sum(x => x.useLenght)];
                foreach (var VARIABLE in vfsDatas)
                {
                    VFStream handle = GetStream(VARIABLE.vfs);
                    if (handle is null)
                    {
                        throw new FileNotFoundException(VARIABLE.vfs);
                    }

                    int offset = VARIABLE.sort * BasicConfig.instance.vfsConfig.chunkSize;
                    handle.Read(VARIABLE.offset, bytes, VARIABLE.sort * BasicConfig.instance.vfsConfig.chunkSize, VARIABLE.useLenght);
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
                if (WorkApi.IsEnableVirtualFileSystem is false)
                {
                    return await File.ReadAllBytesAsync(WorkApi.GetApplicationFilePath(fileName), cancellationTokenSourceGroup.GetToken());
                }

                VFSChunk[] chunkList = FindFileChunkList(fileName);
                if (chunkList is null || chunkList.Length is 0)
                {
                    return Array.Empty<byte>();
                }

                byte[] bytes = new byte[chunkList.Sum(x => x.useLenght)];
                UniTask<ResultStatus>[] readTasks = new UniTask<ResultStatus>[chunkList.Length];
                for (int i = 0; i < chunkList.Length; i++)
                {
                    VFStream handle = GetStream(chunkList[i].vfs);
                    if (handle is null)
                    {
                        readTasks[i] = UniTask.FromResult(ResultStatus.Fail);
                        WorkApi.Logger.LogError(new FileNotFoundException(chunkList[i].vfs));
                        break;
                    }

                    int offset = chunkList[i].sort * BasicConfig.instance.vfsConfig.chunkSize;
                    readTasks[i] = handle.ReadAsync(chunkList[i].offset, bytes, offset, chunkList[i].useLenght, cancellationTokenSourceGroup.GetToken());
                }

                ResultStatus[] readResult = await UniTask.WhenAll(readTasks);
                if (readResult.All(x => x == ResultStatus.Success) is false)
                {
                    WorkApi.Logger.LogError("读取文件数据失败：" + fileName);
                    return Array.Empty<byte>();
                }

                return bytes;
            }
        }
    }
}