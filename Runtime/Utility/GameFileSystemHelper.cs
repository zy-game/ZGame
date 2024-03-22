using System.IO;

namespace ZGame
{
    public static class GameFileSystemHelper
    {
        /// <summary>
        /// 尝试创建文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void TryCreateDirectory(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
        }

        /// <summary>
        /// 尝试拷贝文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void TryCopyFile(string source, string destination)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("File not found: " + source);
            }

            TryCreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);
        }

        /// <summary>
        /// 尝试拷贝文件夹
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void TryCopyDirectory(string source, string destination)
        {
            if (!Directory.Exists(source))
            {
                throw new DirectoryNotFoundException("Directory not found: " + source);
            }

            TryCreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
            {
                TryCopyFile(file, Path.Combine(destination, Path.GetFileName(file)));
            }
        }
    }
}