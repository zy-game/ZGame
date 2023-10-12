using System;

namespace ZEngine
{
    /// <summary>
    /// 配置项
    /// </summary>
    public interface IOptions : IDisposable
    {
        int id { get; set; }
        string name { get; set; }
        string describe { get; set; }

    }

    public enum Localtion
    {
        /// <summary>
        /// 内部配置选项，在打包时会将不在Resources目录下的配置拷贝至Resources中
        /// </summary>
        Internal,

        /// <summary>
        /// 项目级配置选项,在打包时这个配置不会被打进包内
        /// </summary>
        Project,

        /// <summary>
        /// 热更配置项，这个配置只存在包内，在加载时只会从Bundle包中加载
        /// </summary>
        Packaged,
    }

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