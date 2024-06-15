using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DotNetty.Transport.Channels;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Resource;
using Object = UnityEngine.Object;

namespace ZGame.Networking
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetworkManager : GameManager
    {
        private List<ISocketClient> channels = new();

        public override void OnAwake(params object[] args)
        {
        }

        protected override void Update()
        {
            for (int i = channels.Count - 1; i >= 0; i--)
            {
                channels[i].DoUpdate();
            }
        }

        public override void Release()
        {
            channels.ForEach(RefPooled.Free);
            channels.Clear();
        }

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async UniTask<int> Connect<T>(string address, ushort port) where T : ISocketClient
        {
            string ip = $"{address}:{port}";
            ISocketClient channel = channels.Find(x => x.address == ip);
            if (channel is null)
            {
                channel = (ISocketClient)RefPooled.Alloc<T>();
                channels.Add(channel);
                await channel.Connect(address, port);
                await UniTask.SwitchToMainThread();
            }

            return channel.cid;
        }


        /// <summary>
        /// 写入消息
        /// </summary>
        /// <param name="address"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public void Write(int cid, byte[] bytes)
        {
            ISocketClient channel = channels.Find(x => x.cid == cid);
            if (channel is null)
            {
                return;
            }

            channel.Write(bytes);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void Close(int cid)
        {
            ISocketClient channel = channels.Find(x => x.cid == cid);
            if (channel is null)
            {
                return;
            }

            RefPooled.Free(channel);
        }

        /// <summary>
        /// 发起一个POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">消息数据</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public UniTask<string> PostData(string url, object data)
        {
            return PostData(url, data, null);
        }

        /// <summary>
        /// 发起一个POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">消息数据</param>
        /// <param name="headers">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public async UniTask<string> PostData(string url, object data, Dictionary<string, object> headers)
        {
            return await AppCore.Procedure.Execute<string, HttpPost>(this, url, data, headers);
        }

        /// <summary>
        /// 提交一个表单数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="map">消息数据</param>
        /// <param name="headers">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public async UniTask<string> PostDataForm(string url, WWWForm form, Dictionary<string, object> headers)
        {
            return await AppCore.Procedure.Execute<string, HttpPost>(this, url, form, headers);
        }

        /// <summary>
        /// 发起一个GET请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public async UniTask<string> GetData(string url)
        {
            return await AppCore.Procedure.Execute<string, HttpGet>(this, url);
        }

        /// <summary>
        /// 获取响应头
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headName"></param>
        /// <returns></returns>
        public async UniTask<string> GetHead(string url, string headName)
        {
            return await AppCore.Procedure.Execute<string, HttpHead>(this, url, headName);
        }
    }
}