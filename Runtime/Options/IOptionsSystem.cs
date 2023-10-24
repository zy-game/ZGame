using System;
using ZGame.Resource;

namespace ZGame.Options
{
    public interface IOptionsSystem : ISystem
    {
        public T GetOptions<T>() where T : IOptions
            => (T)GetOptions(typeof(T));

        IOptions GetOptions(Type type);
    }

    public class GlobalOptions : IInternalOptions
    {
        public string guid { get; }
        public string name { get; }
        public uint version { get; }

        public IModuleOptions startboot;

        public void Active()
        {
            throw new NotImplementedException();
        }

        public void Inactive()
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}