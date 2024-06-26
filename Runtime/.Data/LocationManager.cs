using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Data
{
    /// <summary>
    /// 本地数据缓存管理器
    /// </summary>
    public class LocationManager : GameManager
    {
        private const string DATA_FILE_NAME = "local.data";
        private Dictionary<string, string> cacheDic = new Dictionary<string, string>();

        public override async void OnAwake(params object[] args)
        {
            string bytes = string.Empty;
#if UNITY_WEBGL
            bytes = PlayerPrefs.GetString(DATA_FILE_NAME);
#else
            bytes = UTF8Encoding.UTF8.GetString(AppCore.File.Read(DATA_FILE_NAME));
#endif
            AppCore.Logger.Log("LOCAL:" + bytes);
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }


            cacheDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(bytes);
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
            AppCore.Logger.Log("SAVE LOCAL DATA :" + json);
#if UNITY_WEBGL
            PlayerPrefs.SetString(DATA_FILE_NAME, json);
#else
            AppCore.File.Write(DATA_FILE_NAME, UTF8Encoding.UTF8.GetBytes(json), 0);
#endif
        }

        public override void Release()
        {
            Save();
            cacheDic.Clear();
        }
    }
}