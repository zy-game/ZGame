using System;
using System.IO;
using UnityEngine;

namespace ZGame.Resource.Config
{
    [Serializable]
    public class OSSOptions
    {
        public string title;
        public OSSType type;
        public string appid;
        public string bucket;
        public string region;
        public string key;
        public string password;

        public string BucketFullName => $"{bucket}-{appid}";

        [NonSerialized] public bool isOn;


        public void Upload(string filePath)
        {
            if (File.Exists(filePath) is false)
            {
                throw new FileNotFoundException(filePath);
            }

            switch (type)
            {
                case OSSType.Streaming:
                    string path = Path.Combine(Application.streamingAssetsPath, Path.GetFileName(filePath).ToLower());
                    if (Directory.Exists(Application.streamingAssetsPath) is false)
                    {
                        Directory.CreateDirectory(Application.streamingAssetsPath);
                    }

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    File.Copy(filePath, path, true);
                    break;
                case OSSType.Aliyun:
                    break;
                case OSSType.Tencent:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetFilePath(string fileName)
        {
            switch (type)
            {
                case OSSType.Streaming:
                    return Path.Combine(Application.streamingAssetsPath, fileName);
                case OSSType.Aliyun:
                    return $"https://{bucket}.oss-{region}.aliyuncs.com/{BasicConfig.GetPlatformName()}/{fileName}";
                case OSSType.Tencent:
                    return $"https://{bucket}.cos.{region}.myqcloud.com/{BasicConfig.GetPlatformName()}/{fileName}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}