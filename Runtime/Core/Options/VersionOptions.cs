using System;

namespace ZEngine
{
    [Serializable]
    public class VersionOptions
    {
        public uint mainVersion;
        public uint subVersion;
        public uint buildVersion;

        public static VersionOptions None { get; } = new VersionOptions() { mainVersion = 0, subVersion = 0, buildVersion = 0 };
    }
}