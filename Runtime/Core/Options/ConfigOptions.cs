using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigOptions : Attribute
{
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

    internal Localtion localtion;

    public ConfigOptions(Localtion localtion)
    {
        this.localtion = localtion;
    }
}