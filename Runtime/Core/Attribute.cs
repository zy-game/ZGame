using System;

namespace ZEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigAttribute : Attribute
    {
        internal string path;
        internal Localtion localtion;

        public ConfigAttribute(Localtion localtion, string path = "")
        {
            this.path = path;
            this.localtion = localtion;
        }
    }

    public class InternalConfigAttribute : ConfigAttribute
    {
        public InternalConfigAttribute() : base(Localtion.Internal)
        {
        }
    }

    public class ProjectConfigAttribute : ConfigAttribute
    {
        public ProjectConfigAttribute() : base(Localtion.Project)
        {
        }
    }

    public class PackageConfigAttribute : ConfigAttribute
    {
        public PackageConfigAttribute(string path) : base(Localtion.Packaged, path)
        {
        }
    }

    public class OptionsName : Attribute
    {
        public string name;

        public OptionsName(string name)
        {
            this.name = name;
        }
    }
}