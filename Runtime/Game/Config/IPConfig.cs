using System;
using System.Collections.Generic;

namespace ZGame
{
    public class IPConfig : BaseConfig<IPConfig>
    {
        public string selected;
        public List<IPOptions> ipList;

        public IPOptions current
        {
            get { return ipList.Find(o => o.title == selected); }
        }

        public override void OnAwake()
        {
            if (ipList is null)
            {
                ipList = new List<IPOptions>();
            }
        }

        public void Add(IPOptions options)
        {
            ipList.Add(options);
        }

        public void Remove(IPOptions options)
        {
            ipList.Remove(options);
        }

        public string GetUrl(string path)
        {
            return current?.GetUrl(path);
        }
    }

    [Serializable]
    public class IPOptions
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title;

        /// <summary>
        /// 地址
        /// </summary>
        public string address;

        /// <summary>
        /// 端口
        /// </summary>
        public int port;


        [NonSerialized] public bool isOn;

        public string GetUrl(string path)
        {
            if (port == 0)
            {
                return $"{address}{path}";
            }

            return $"{address}:{port}{path}";
        }
    }
}