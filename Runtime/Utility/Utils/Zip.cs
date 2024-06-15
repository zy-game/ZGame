using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame
{
    /// <summary>
    /// 压缩器
    /// </summary>
    public class Zip
    {
        public static void ComperessDirectory(string fileName, string dir)
        {
            byte[] bytes = Compress(Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories));
            File.WriteAllBytes(AppCore.GetFileOutputPath(fileName), bytes);
        }

        public static void ComperessDirectory(string fileName, string ext, string dir)
        {
            byte[] bytes = Compress(Directory.GetFiles(dir, ext, SearchOption.AllDirectories));
            File.WriteAllBytes(AppCore.GetFileOutputPath(fileName), bytes);
        }

        public static void CompressFiles(string fileName, params string[] files)
        {
            byte[] bytes = Compress(files);
            File.WriteAllBytes(AppCore.GetFileOutputPath(fileName), bytes);
        }

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="args"></param>
        public static byte[] Compress(params string[] files)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create))
                {
                    foreach (string s in files)
                    {
                        AddFileToZip(archive, s);
                    }
                }

                return ms.ToArray();
            }
        }

        private static void AddFileToZip(ZipArchive archive, string s)
        {
            if (File.Exists(s) is false)
            {
                return;
            }

            ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(s));
            using (FileStream fs = File.OpenRead(s))
            {
                using (Stream stream = entry.Open())
                {
                    fs.CopyTo(stream);
                }
            }
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="str"></param>
        public static async UniTask<string[]> Decompress(string zip, string dir)
        {
            if (zip.IsNullOrEmpty() || dir.IsNullOrEmpty())
            {
                return default;
            }

            return await Decompress(await File.ReadAllBytesAsync(zip), dir);
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="str"></param>
        public static async UniTask<string[]> Decompress(byte[] zip, string dir)
        {
            if (zip is null || zip.Length == 0 || dir.IsNullOrEmpty())
            {
                return default;
            }

            List<string> files = new List<string>();
            using (MemoryStream ms = new MemoryStream(zip))
            {
                using (ZipArchive archive = new ZipArchive(ms))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        using (StreamReader reader = new StreamReader(entry.Open()))
                        {
                            string str = Path.Combine(dir, entry.FullName);
                            string info = await reader.ReadToEndAsync();
                            await File.WriteAllTextAsync(str, info);
                            files.Add(str);
                        }
                    }
                }
            }

            return files.ToArray();
        }


        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="str"></param>
        public static async UniTask<Dictionary<string, byte[]>> Decompress(byte[] zip, CancellationToken cancellationToken = default)
        {
            if (zip is null || zip.Length == 0)
            {
                return default;
            }

            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            using (MemoryStream ms = new MemoryStream(zip))
            {
                using (ZipArchive archive = new ZipArchive(ms))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        using (Stream stream = entry.Open())
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                await stream.CopyToAsync(m, cancellationToken);
                                files.Add(entry.FullName, m.ToArray());
                                await m.FlushAsync(cancellationToken);
                            }
                        }
                    }
                }
            }

            return files;
        }
    }
}