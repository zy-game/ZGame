using System;
using UnityEngine.Serialization;

namespace ZEngine
{
    [Serializable]
    public class VersionOptions
    {
        public byte major;
        public byte minor;
        public byte build;
        private byte max = byte.MaxValue;

        public void Up()
        {
            if (build >= max)
            {
                build = 0;
                if (minor >= max)
                {
                    minor = 0;
                    if (major >= max)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    else
                    {
                        major++;
                    }
                }
                else
                {
                    minor++;
                }
            }
            else
            {
                build++;
            }
        }

        public void Down()
        {
            if (build == 0)
            {
                if (minor == 0)
                {
                    if (major == 0)
                    {
                        major = 0;
                        build = 0;
                        minor = 0;
                    }
                    else
                    {
                        major--;
                        minor = max;
                    }
                }
                else
                {
                    build = max;
                    minor--;
                }
            }
            else
            {
                build--;
            }
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{build}";
        }

        public static VersionOptions None { get; } = new VersionOptions() { major = 0, minor = 0, build = 0 };

        public static bool operator ==(VersionOptions l, VersionOptions r)
        {
            uint a = (uint)l.major + l.minor + l.build;
            uint b = (uint)r.major + r.minor + r.build;
            return a == b;
        }

        public static bool operator !=(VersionOptions l, VersionOptions r)
        {
            uint a = (uint)l.major + l.minor + l.build;
            uint b = (uint)r.major + r.minor + r.build;
            return a != b;
        }

        public static bool operator >(VersionOptions l, VersionOptions r)
        {
            uint a = (uint)l.major + l.minor + l.build;
            uint b = (uint)r.major + r.minor + r.build;
            return a > b;
        }

        public static bool operator <(VersionOptions l, VersionOptions r)
        {
            uint a = (uint)l.major + l.minor + l.build;
            uint b = (uint)r.major + r.minor + r.build;
            return a < b;
        }
    }
}