using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    public class VFSManager : Single<VFSManager>
    {
        private List<VFSData> dataList = new List<VFSData>();
        private Dictionary<string, FileStream> fileHandleList = new Dictionary<string, FileStream>();

        public VFSManager()
        {
            string filePath = AppConfig.GetLocalFilePath("vfs");
            if (!File.Exists(filePath))
            {
                return;
            }

            dataList = Engine.Json.Parse<List<VFSData>>(File.ReadAllText(filePath));
        }

        private void SaveVFSData()
        {
        }

        public VFSData GetNotUseSgement()
        {
            return dataList.Find(x => x.name.IsNullOrEmpty());
        }

        public VFSData[] CreateVFS()
        {
            List<VFSData> list = new List<VFSData>();
            string vfsName = Guid.NewGuid().ToString().Replace("-", String.Empty);
            for (int i = 0; i < AppConfig.instance.vfsOptions.sgementCount; i++)
            {
                list.Add(new VFSData() { vfs = vfsName });
            }

            string vfsPath = AppConfig.GetLocalFilePath(vfsName);
            if (File.Exists(vfsPath))
            {
                File.Delete(vfsPath);
            }

            FileStream fileStream = File.Create(vfsPath);
            fileHandleList.Add(vfsName, fileStream);
            dataList.AddRange(list);
            SaveVFSData();
            return list.ToArray();
        }

        public VFSData CreateSingleVFS()
        {
            string vfsName = Guid.NewGuid().ToString().Replace("-", String.Empty);
            VFSData vfsData = new VFSData() { vfs = vfsName, offset = 0 };
            string vfsPath = AppConfig.GetLocalFilePath(vfsName);
            if (File.Exists(vfsPath))
            {
                File.Delete(vfsPath);
            }

            FileStream fileStream = File.Create(vfsPath);
            fileHandleList.Add(vfsName, fileStream);
            dataList.Add(vfsData);
            SaveVFSData();
            return vfsData;
        }

        public FileStream GetFileStream(string vfsName)
        {
            if (fileHandleList.TryGetValue(vfsName, out FileStream stream))
            {
                return stream;
            }

            fileHandleList.Add(vfsName, stream = new FileStream(AppConfig.GetLocalFilePath(vfsName), FileMode.Open, FileAccess.ReadWrite));
            return stream;
        }

        public bool Exist(string fileName)
        {
            if (AppConfig.instance.vfsOptions.vfsState == Status.On)
            {
                return dataList.Find(x => x.name == fileName) is not null;
            }

            return File.Exists(AppConfig.GetLocalFilePath(fileName));
        }

        public bool Delete(string fileName)
        {
            if (AppConfig.instance.vfsOptions.vfsState == Status.Off)
            {
                File.Delete(AppConfig.GetLocalFilePath(fileName));
            }
            else
            {
                IEnumerable<VFSData> fileData = dataList.Where(x => x.name == fileName);
                foreach (var file in fileData)
                {
                    dataList.Remove(file);
                }

                VFSData temp = fileData.First();
                IEnumerable<VFSData> list = dataList.Where(x => x.vfs == temp.vfs);
                if (list.Count() == 0)
                {
                    //todo 如果没有找到在使用这个vfs文件的时候就删除
                    File.Delete(AppConfig.GetLocalFilePath(temp.vfs));
                }

                SaveVFSData();
            }

            return Exist(fileName);
        }

        public VFSData GetFileData(string fileName)
        {
            return dataList.Find(x => x.name == fileName);
        }

        public IWriteFileExecuteHandle WriteFile(string fileName, byte[] bytes)
        {
            Delete(fileName);
            GameWriteFileExecuteHandle writeFileExecuteHandle = Engine.Reference.Dequeue<GameWriteFileExecuteHandle>();
            writeFileExecuteHandle.Execute(fileName, bytes);
            return writeFileExecuteHandle;
        }

        public IWriteFileAsyncExecuteHandle WriteFileAsync(string fileName, byte[] bytes)
        {
            Delete(fileName);
            GameWriteFileAsyncExecuteHandle writeFileExecuteHandle = Engine.Reference.Dequeue<GameWriteFileAsyncExecuteHandle>();
            writeFileExecuteHandle.Execute(fileName, bytes);
            return writeFileExecuteHandle;
        }

        public IReadFileExecuteHandle ReadFile(string fileName)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            GameReadFileExecuteHandle gameReadFileExecuteHandle = Engine.Reference.Dequeue<GameReadFileExecuteHandle>();
            gameReadFileExecuteHandle.Execute(fileName);
            return gameReadFileExecuteHandle;
        }

        public IReadFileAsyncExecuteHandle ReadFileAsync(string fileName)
        {
            if (Exist(fileName) is false)
            {
                return default;
            }

            GameReadFileAsyncExecuteHandle gameReadFileAsyncExecuteHandle = Engine.Reference.Dequeue<GameReadFileAsyncExecuteHandle>();
            gameReadFileAsyncExecuteHandle.Execute(fileName);
            return gameReadFileAsyncExecuteHandle;
        }
    }
}