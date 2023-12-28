using System;
using System.Collections.Generic;

namespace ZGame.Data
{
    public class DataManager : Singleton<DataManager>
    {
        private Dictionary<Type, object> map = new();

        public T Get<T>()
        {
            if (map.TryGetValue(typeof(T), out object data))
            {
                return (T)data;
            }

            return default;
        }

        public void Set<T>(T data)
        {
            map[typeof(T)] = data;
        }

        public void Clear()
        {
            map.Clear();
        }
    }
}