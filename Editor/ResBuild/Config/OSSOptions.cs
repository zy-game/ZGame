using System;

namespace ZGame.Editor.ResBuild.Config
{
    public enum OSSType
    {
        Aliyun,
        Tencent,
    }
    [Serializable]
    public class OSSOptions
    {
        public string title;
        public bool use;
        public OSSType type;
        public string address;
        public string key;
        public string password;
    }
}