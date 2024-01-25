using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame;

namespace ZGame.IM
{
    public class IMManager : Singleton<IMManager>
    {
        private List<IMClient> clients = new();
        private UniTaskCompletionSource<bool> _completionSource;
        public IMClient current { get; private set; }

        protected override void OnAwake()
        {
        }

        protected override void OnDestroy()
        {
            foreach (var VARIABLE in clients)
            {
                VARIABLE.Dispose();
            }

            clients.Clear();
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].OnRecvieMessage();
            }
        }

        /// <summary>
        /// 切换会话
        /// </summary>
        /// <param name="id"></param>
        public void Switch(string id)
        {
            Switch(GetSession(id));
        }

        /// <summary>
        /// 切换会话
        /// </summary>
        /// <param name="client"></param>
        public void Switch(IMClient client)
        {
            if (client is null)
            {
                return;
            }

            if (clients.Exists(x => x.id == client.id) is false)
            {
                clients.Add(client);
            }

            current = client;
        }

        /// <summary>
        /// 关闭会话
        /// </summary>
        /// <param name="id"></param>
        public void Close(string id)
        {
            if (id.IsNullOrEmpty())
            {
                return;
            }

            IMClient handler = GetSession(id);
            if (handler is null)
            {
                return;
            }

            handler.Dispose();
            clients.Remove(handler);
        }

        /// <summary>
        /// 获取会话
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMClient GetSession(string id)
        {
            if (id.IsNullOrEmpty())
            {
                return default;
            }

            return clients.Find(x => x.id == id);
        }

        /// <summary>
        /// 发送聊天信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        public void SendChat(string content)
        {
            if (current is null)
            {
                return;
            }

            SendChat(current.id, content);
        }


        /// <summary>
        /// 发送聊天信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        public void SendChat(string id, string content)
        {
            IMClient handler = GetSession(id);
            if (handler is null)
            {
                Debug.Log("Not Find:" + id);
                return;
            }

            handler.SendChat(content);
        }

        public void SendAudio(AudioClip clip, int lenght)
        {
            if (current is null)
            {
                return;
            }

            SendAudio(current.id, clip, lenght);
        }

        public void SendAudio(string id, AudioClip clip, int lenght)
        {
            IMClient handler = GetSession(id);
            if (handler is null)
            {
                Debug.Log("Not Find:" + id);
                return;
            }

            handler.SendAudio(clip, lenght);
        }
    }
}