﻿using System;
using System.IO;

namespace Downloader
{
    internal static class FileHelper
    {
        public static Stream CreateFile(string filename)
        {
            string directory = Path.GetDirectoryName(filename);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return Stream.Null;
            }

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            return new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        }
        public static string GetTempFile()
        {
            return GetTempFile(Path.GetTempPath(), string.Empty);
        }
        public static string GetTempFile(string baseDirectory, string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                baseDirectory = Path.GetTempPath();
            }

            string filename = Path.Combine(baseDirectory, Guid.NewGuid().ToString("N") + fileExtension);
            CreateFile(filename).Dispose();

            return filename;
        }

        public static long GetAvailableFreeSpaceOnDisk(string directory)
        {
            try
            {
                var drive = new DriveInfo(directory);
                if (drive.IsReady)
                {
                    return drive.AvailableFreeSpace;
                }

                return 0L;
            }
            catch (ArgumentException)
            {
                // null or use UNC (\\server\share) paths not supported.
                return 0L;
            }
        }

        public static void ThrowIfNotEnoughSpace(long actualNeededSize, params string[] directories)
        {
            if (directories != null)
            {
                foreach (string directory in directories)
                {
                    var availableFreeSpace = GetAvailableFreeSpaceOnDisk(directory);
                    if (availableFreeSpace > 0 && availableFreeSpace < actualNeededSize)
                    {
                        throw new IOException($"There is not enough space on the disk `{directory}` with {availableFreeSpace} bytes");
                    }
                }
            }
        }
    }
}