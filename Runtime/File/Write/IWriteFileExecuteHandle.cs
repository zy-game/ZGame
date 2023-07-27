using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    public interface IWriteFileExecuteHandle : IGameExecuteHandle<WriteFileResult>
    {
    }

    class GameWriteFileExecuteHandle : IWriteFileExecuteHandle
    {
        public float progress { get; }
        public ExecuteStatus status { get; private set; }
        public WriteFileResult result { get; private set; }


        public void Execute(params object[] args)
        {
            if (args is null || args.Length is 0)
            {
                status = ExecuteStatus.Failed;
                Engine.Console.Error("Not Find Write File Patg or fileData");
                return;
            }

            string fileName = (string)args[0];
            byte[] bytes = (byte[])args[1];
            if (AppConfig.instance.vfsOptions.vfsState == Status.Off)
            {
                //todo 如果没有开启vfs，则将所有数据写入单个文件中
                WriteToSingleFile(AppConfig.GetLocalFilePath(fileName), bytes);
            }
            else
            {
                //todo 更具vfs布局写入文件
                WriteToVFS(fileName, bytes);
            }
        }

        private void WriteToVFS(string filePath, byte[] bytes)
        {
            switch (AppConfig.instance.vfsOptions.layout)
            {
                case VFSLayout.SpacePriority:
                    //紧密布局，将每个文件系统都塞满，
                    int sl = AppConfig.instance.vfsOptions.sgementLenght;
                    int count = bytes.Length / sl;
                    count = bytes.Length > count * sl ? count + 1 : count;
                    int offset = 0;
                    int length = sl;
                    for (int i = 0; i < count; i++)
                    {
                        VFSData vfsData = VFSManager.instance.GetNotUseSgement();
                        if (vfsData is null)
                        {
                            vfsData = VFSManager.instance.GenerateVFSystem(count).FirstOrDefault();
                        }

                        WriteToVFS(vfsData, bytes, offset, length);
                        offset += length;
                    }

                    break;
                case VFSLayout.ReadWritePriority:
                    //如果当前文件大于分片，就写入单独文件中，只有小于分片的才去合并文件
                    if (bytes.Length <= AppConfig.instance.vfsOptions.sgementLenght)
                    {
                        VFSData vfsData = VFSManager.instance.GetNotUseSgement();
                        if (vfsData is null)
                        {
                            vfsData = VFSManager.instance.GenerateVFSystem(AppConfig.instance.vfsOptions.sgementCount).FirstOrDefault();
                        }

                        WriteToVFS(vfsData, bytes, 0, bytes.Length);
                        return;
                    }
                    else
                    {
                        VFSData vfsData = VFSManager.instance.GenerateVFSystem(1).FirstOrDefault();
                        WriteToVFS(vfsData, bytes, 0, bytes.Length);
                    }

                    break;
            }
        }

        private void WriteToVFS(VFSData vfsData, byte[] bytes, int byteOffset, int length)
        {
            FileStream fileStream = VFSManager.instance.GetFileStream(vfsData.vfs);
            fileStream.Seek(vfsData.offset, SeekOrigin.Begin);
            fileStream.Write(bytes, byteOffset, length);
        }

        private void WriteToSingleFile(string filePath, byte[] bytes)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllBytes(filePath, bytes);
            status = ExecuteStatus.Success;
        }

        public bool EnsureExecuteSuccessfuly()
        {
            return status == ExecuteStatus.Success;
        }

        public void Release()
        {
            Engine.Reference.Release(result);
            result = null;
            status = ExecuteStatus.None;
        }
    }
}