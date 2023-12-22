using System.IO;

namespace ZGame
{
    public class PathHelper
    {
        public static string GetFolderPath(string path)
        {
            string[] paths = path.Split('/');
            string folderPath = "";
            for (int i = 0; i < paths.Length - 1; i++)
            {
                folderPath += paths[i] + "/";
            }

            return folderPath;
        }

        public static void TryCreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}