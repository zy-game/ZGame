using System;

namespace ZGame
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }


    public sealed class ResourceModuleNotFoundException : NotFoundException
    {
        public ResourceModuleNotFoundException() : base("没有找到激活的资源模块")
        {
        }

        public ResourceModuleNotFoundException(string moduleName) : base($"没有找到资源模块:{moduleName}")
        {
        }
    }
}