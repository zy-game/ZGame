using System;

namespace ZGame.Config
{
    public interface IConfig : IDisposable
    {
        
    }

    public class ConfigManager : Singleton<ConfigManager>
    {
        public T GetConfig<T>() where T : IConfig
        {
            return default;
        }
    }
}