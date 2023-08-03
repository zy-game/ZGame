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

        public static bool operator ==(VersionOptions l, VersionOptions r)
        {
            return l.mainVersion == r.mainVersion && l.subVersion == r.subVersion && l.buildVersion == r.buildVersion;
        }

        public static bool operator !=(VersionOptions l, VersionOptions r)
        {
            return l.mainVersion != r.mainVersion || l.subVersion != r.subVersion || l.buildVersion != r.buildVersion;
        }

        public static bool operator >(VersionOptions l, VersionOptions r)
        {
            uint a = l.mainVersion + l.subVersion + l.buildVersion;
            uint b = r.mainVersion + r.subVersion + r.buildVersion;
            return a > b;
        }

        public static bool operator <(VersionOptions l, VersionOptions r)
        {
            uint a = l.mainVersion + l.subVersion + l.buildVersion;
            uint b = r.mainVersion + r.subVersion + r.buildVersion;
            return a < b;
        }
    }
}