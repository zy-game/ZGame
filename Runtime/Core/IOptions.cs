using System;

namespace ZGame
{
    /// <summary>
    /// 配置项
    /// </summary>
    public interface IOptions : IEntity
    {
        string name { get; }
        uint version { get; }
        void Active();
        void Inactive();

        public static T Requery<T>() where T : IOptions
        {
            return default;
        }
    }

    /// <summary>
    /// 运行时配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RuntimeOptions : Attribute
    {
    }

    /// <summary>
    /// 项目配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProjectOptions : Attribute
    {
    }

    /// <summary>
    /// 内部配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InternalOptions : Attribute
    {
    }
}