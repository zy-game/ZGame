using System;
using System.Collections.Generic;
using ZGame.Module;

namespace ZGame.Data
{
    /// <summary>
    /// 运行时游戏数据
    /// </summary>
    public class RuntimeDatableManager : IModule
    {
        private Dictionary<string, object> map = new();

        public void OnAwake()
        {
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasKey<T>()
        {
            return HasKey(typeof(T).FullName);
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            return map.ContainsKey(key);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return Get<T>(typeof(T).FullName);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (map.ContainsKey(key))
            {
                return (T)map[key];
            }

            return default;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public void Set(object data)
        {
            Set(data.GetType().FullName, data);
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public void Set(string key, object data)
        {
            if (map.ContainsKey(key) is false)
            {
                map.Add(key, data);
                return;
            }

            map[key] = data;
        }

        /// <summary>
        /// 清理所有数据
        /// </summary>
        public void Clear()
        {
            map.Clear();
        }

        /// <summary>
        /// 清理指定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Clear<T>()
        {
            Clear(typeof(T).FullName);
        }

        /// <summary>
        /// 清理指定数据
        /// </summary>
        /// <param name="key"></param>
        public void Clear(string key)
        {
            if (map.ContainsKey(key))
            {
                map.Remove(key);
            }
        }

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}