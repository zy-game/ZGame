using System;

namespace ZEngine
{
    [Serializable]
    public class VersionOptions
    {
        public int mainVersion;
        public int subVersion;
        public int buildVersion;

        public static VersionOptions None { get; } = new VersionOptions() { mainVersion = -1, subVersion = -1, buildVersion = -1 };
    }
}