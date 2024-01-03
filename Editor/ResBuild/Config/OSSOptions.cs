using System;

namespace ZGame.Editor.ResBuild.Config
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
    }
}