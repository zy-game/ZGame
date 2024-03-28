using System;
using System.Collections.Generic;

namespace ZGame.Data
{
    /// <summary>
    /// 运行时游戏数据
    /// </summary>
    public class RuntimeDatableManager : GameFrameworkModule
    {
        private List<IRuntimeDatable> map = new();


        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains<T>() where T : IRuntimeDatable
        {
            return Contains(typeof(T));
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Contains(Type type)
        {
            if (typeof(IRuntimeDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IRuntimeDatable));
            }

            return map.Exists(x => x.GetType() == type);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetDatable<T>() where T : IRuntimeDatable
        {
            return (T)map.Find(x => x.GetType() == typeof(T));
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IRuntimeDatable GetDatable(Type type)
        {
            if (typeof(IRuntimeDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IRuntimeDatable));
            }

            return map.Find(x => x.GetType() == type);
        }

        /// <summary>
        /// 尝试获取指定的数据
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetValue<T>(out T data) where T : IRuntimeDatable
        {
            data = (T)map.Find(x => x.GetType() == typeof(T));
            return data != null;
        }

        public bool TryGetValue(Type type, out IRuntimeDatable data)
        {
            if (typeof(IRuntimeDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IRuntimeDatable));
            }

            data = map.Find(x => x.GetType() == type);
            return data != null;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public void SetDatable(IRuntimeDatable data)
        {
            if (Contains(data.GetType()))
            {
                throw new Exception($"{data}已存在");
            }

            map.Add(data);
        }

        /// <summary>
        /// 清理指定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Clear<T>() where T : IRuntimeDatable
        {
            Clear(typeof(T));
        }

        public void Clear(Type type)
        {
            if (typeof(IRuntimeDatable).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(nameof(IRuntimeDatable));
            }

            IRuntimeDatable data = GetDatable(type);
            if (data is null)
            {
                return;
            }

            data.Dispose();
            map.Remove(data);
        }

        public override void Release()
        {
            foreach (var VARIABLE in map)
            {
                VARIABLE.Dispose();
            }

            map.Clear();
            GC.SuppressFinalize(this);
        }
    }
}