using System.IO;

namespace ZGame
{
    public static class PathHelper
    {
        public static void TryExistsDirectory(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
        }
    }
}