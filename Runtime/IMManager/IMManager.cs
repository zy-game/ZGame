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
        private IMClient current;
        private List<IMClient> clients = new();
        private UniTaskCompletionSource<bool> _completionSource;

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

        public async UniTask<bool> Create(IMClient client, params object[] args)
        {
            bool state = await client.Open(args);
            if (state)
            {
                clients.Add(client);
                current = client;
            }

            return state;
        }

        /// <summary>
        /// 切换聊天会话
        /// </summary>
        /// <param name="id"></param>
        public void Switch(string id)
        {
            Switch(GetSession(id));
        }

        /// <summary>
        /// 切换聊天会话
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
                Debug.Log("当前对话为空");
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
                Debug.Log("没有找到对话：" + id);
                return;
            }

            handler.SendChat(content);
        }

        /// <summary>
        /// 开始语音消息
        /// </summary>
        /// <param name="id"></param>
        public void OnStartAudioVoice()
        {
            if (current is null)
            {
                Debug.Log("当前对话为空");
                return;
            }

            current.StartAudio();
        }

        /// <summary>
        /// 开始语音消息
        /// </summary>
        /// <param name="id"></param>
        public void OnStartAudioVoice(string id)
        {
            IMClient client = GetSession(id);
            if (client is null)
            {
                Debug.Log("没有找到对话：" + id);
                return;
            }

            client.StartAudio();
        }

        /// <summary>
        /// 停止语音消息
        /// </summary>
        /// <param name="id"></param>
        public void OnStopAudioVoice(string id)
        {
            IMClient client = GetSession(id);
            if (client is null)
            {
                Debug.Log("没有找到对话：" + id);
                return;
            }

            client.EndAudio();
        }

        /// <summary>
        /// 停止语音消息
        /// </summary>
        /// <param name="id"></param>
        public void OnStopAudioVoice()
        {
            if (current is null)
            {
                Debug.Log("当前对话为空");
                return;
            }

            current.EndAudio();
        }

        /// <summary>
        /// 发送语音数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clip"></param>
        public void OnAudioVoiceClip(string id, byte[] clip)
        {
            IMClient client = GetSession(id);
            if (client is null)
            {
                Debug.Log("没有找到对话：" + id);
                return;
            }

            client.SendAudio(clip);
        }

        /// <summary>
        /// 发送语音数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clip"></param>
        public void OnAudioVoiceClip(byte[] clip)
        {
            if (current is null)
            {
                Debug.Log("当前对话为空");
                return;
            }

            current.SendAudio(clip);
        }
    }
}