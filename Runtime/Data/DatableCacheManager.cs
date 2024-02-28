using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.FileSystem;

namespace ZGame.Data
{
    /// <summary>
    /// 本地数据缓存管理器
    /// </summary>
    public class DatableCacheManager : Singleton<DatableCacheManager>
    {
        private Dictionary<string, string> cacheDic = new Dictionary<string, string>();

        protected override void OnAwake()
        {
            byte[] bytes = VFSManager.instance.Read("dataCache.ini");
            if (bytes is null || bytes.Length == 0)
            {
                return;
            }

            string json = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log(json);
            cacheDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        /// <summary>
        /// 是否存在某个键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            return cacheDic.ContainsKey(key);
        }

        /// <summary>
        /// 获取一个整型值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetInt(string key)
        {
            if (int.TryParse(GetString(key), out int value))
            {
                return value;
            }

            return 0;
        }

        /// <summary>
        /// 获取一个字符串值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string key)
        {
            if (cacheDic.TryGetValue(key, out string value))
            {
                return value;
            }

            return String.Empty;
        }

        /// <summary>
        /// 获取一个布尔值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetBool(string key)
        {
            int value = GetInt(key);
            return value == 1;
        }

        /// <summary>
        /// 设置一个字符串值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetString(string key, string value)
        {
            if (cacheDic.ContainsKey(key) is false)
            {
                cacheDic.Add(key, value);
            }

            cacheDic[key] = value;
            Save();
        }

        /// <summary>
        /// 设置一个整型值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetInt(string key, int value)
        {
            SetString(key, value.ToString());
        }

        /// <summary>
        /// 设置一个布尔值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBool(string key, bool value)
        {
            SetString(key, value ? "1" : "0");
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        /// <param name="key"></param>
        public void Clear(string key)
        {
            cacheDic.Remove(key);
            Save();
        }

        /// <summary>
        /// 清理所有缓存
        /// </summary>
        public void Clear()
        {
            cacheDic.Clear();
            Save();
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            string json = JsonConvert.SerializeObject(cacheDic);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            VFSManager.instance.Write("dataCache.ini", bytes, 0);
            Debug.Log(json);
        }
    }
}