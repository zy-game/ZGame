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
        public string bucket;
        public string region;
        public string key;
        public string password;
        [NonSerialized] public bool isOn;

        public string GetFilePath(string fileName)
        {
            switch (type)
            {
                case OSSType.Streaming:
                    return Path.Combine(Application.streamingAssetsPath, fileName.ToLower());
                case OSSType.Aliyun:
                    return $"https://{bucket}.oss-{region}.aliyuncs.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.Tencent:
                    return $"https://{bucket}.cos.{region}.myqcloud.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}