using System;

namespace ZEngine
{
    [Serializable]
    public class VersionOptions
    {
        public byte mainVersion;
        public byte subVersion;
        public byte buildVersion;
        private byte max = byte.MaxValue;

        public void Up()
        {
            if (buildVersion >= max)
            {
                buildVersion = 0;
                if (subVersion >= max)
                {
                    subVersion = 0;
                    if (mainVersion >= max)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    else
                    {
                        mainVersion++;
                    }
                }
                else
                {
                    subVersion++;
                }
            }
            else
            {
                buildVersion++;
            }
        }

        public void Down()
        {
            if (buildVersion == 0)
            {
                if (subVersion == 0)
                {
                    if (mainVersion == 0)
                    {
                        mainVersion = 0;
                        buildVersion = 0;
                        subVersion = 0;
                    }
                    else
                    {
                        mainVersion--;
                        subVersion = max;
                    }
                }
                else
                {
                    buildVersion = max;
                    subVersion--;
                }
            }
            else
            {
                buildVersion--;
            }
        }

        public override string ToString()
        {
            return $"{mainVersion}.{subVersion}.{buildVersion}";
        }

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
            uint a = (uint)l.mainVersion + l.subVersion + l.buildVersion;
            uint b = (uint)r.mainVersion + r.subVersion + r.buildVersion;
            return a > b;
        }

        public static bool operator <(VersionOptions l, VersionOptions r)
        {
            uint a = (uint)l.mainVersion + l.subVersion + l.buildVersion;
            uint b = (uint)r.mainVersion + r.subVersion + r.buildVersion;
            return a < b;
        }
    }
}