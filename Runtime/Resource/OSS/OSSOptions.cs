using System;
using System.IO;
using UnityEngine;

namespace ZGame.Resource.Config
{
    public enum OSSType
    {
        None,
        Aliyun,
        Tencent,
        Streaming,
        URL,
    }

    [Serializable]
    public class OSSOptions
    {
        public string title;
        public OSSType type;
        public string bucket;
        public string region;
        public string key;
        public string password;
        public bool enableAccelerate;
        [NonSerialized] public bool isOn;

        public string GetFilePath(string fileName)
        {
            switch (type)
            {
                case OSSType.Streaming:
                    return Path.Combine(Application.streamingAssetsPath, fileName.ToLower());
                case OSSType.Aliyun:
                    if (enableAccelerate)
                    {
                        return $"https://{bucket}.oss-accelerate.aliyuncs.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return $"https://{bucket}.oss-{region}.aliyuncs.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.Tencent:
                    if (enableAccelerate)
                    {
                        return $"https://{bucket}.cos.accelerate.myqcloud.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return $"https://{bucket}.cos.{region}.myqcloud.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.URL:
                    return $"{region}{BasicConfig.GetPlatformName()}/{fileName.Replace(" ", "_").ToLower()}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}