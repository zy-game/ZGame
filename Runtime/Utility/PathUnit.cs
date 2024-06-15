using System.IO;

namespace ZGame
{
    public static class PathUnit
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
                throw new FileNotFoundException("Vfs not found: " + source);
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

        public static bool HasExtension(string path)
        {
            return Path.GetExtension(path).IsNullOrEmpty() is false;
        }

        public static string GetFileName(string path, bool isHaveExtension)
        {
            return isHaveExtension ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
        }

        public static bool ComparessFileName(string source, string des)
        {
            if (Path.GetExtension(source).IsNullOrEmpty())
            {
                return Path.GetFileNameWithoutExtension(source).Equals(Path.GetFileNameWithoutExtension(des));
            }
            else
            {
                return Path.GetFileName(source).Equals(Path.GetFileName(des));
            }
        }
    }
}