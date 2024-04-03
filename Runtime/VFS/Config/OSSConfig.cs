using System;
using System.Collections.Generic;
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

    public class OSSConfig : BaseConfig<OSSConfig>
    {
        public string seletion;
        public List<OSSOptions> ossList;

        private OSSOptions _options;

        public OSSOptions current
        {
            get { return ossList.Find(x => x.title == seletion); }
        }

        public override void OnAwake()
        {
            if (ossList is null)
            {
                ossList = new List<OSSOptions>();
            }
        }

        public string GetFilePath(string fileName)
        {
            if (current is null)
            {
                return String.Empty;
            }

            return current.GetFilePath(fileName);
        }

        public void Add(OSSOptions options)
        {
            ossList.Add(options);
        }

        public void Remove(OSSOptions options)
        {
            ossList.Remove(options);
        }
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
                        return $"https://{bucket}.oss-accelerate.aliyuncs.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return $"https://{bucket}.oss-{region}.aliyuncs.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.Tencent:
                    if (enableAccelerate)
                    {
                        return $"https://{bucket}.cos.accelerate.myqcloud.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                    }

                    return $"https://{bucket}.cos.{region}.myqcloud.com/{GameFrameworkEntry.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.URL:
                    return $"{region}{GameFrameworkEntry.GetPlatformName()}/{fileName.Replace(" ", "_").ToLower()}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}