using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZGame;
using Inworld;

namespace ZGame.IM
{
    public class IMMgr : Singleton<IMMgr>
    {
        private List<IMHandler> handlers = new();

        protected override void OnDestroy()
        {
            foreach (var VARIABLE in handlers)
            {
                VARIABLE.Dispose();
            }

            handlers.Clear();
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].OnUpdate();
            }
        }

        /// <summary>
        /// 设置新会话
        /// </summary>
        /// <param name="handler"></param>
        public void NewSession(IMHandler handler)
        {
            handlers.Add(handler);
        }

        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <param name="character"></param>
        /// <param name="setting"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> NewSession<T>(string character, InworldConfig setting) where T : IMHandler
        {
            T handle = Activator.CreateInstance<T>();
            bool success = await handle.Open(character, setting);
            if (success is false)
            {
                return default;
            }

            handlers.Add(handle);
            return handle;
        }

        /// <summary>
        /// 关闭会话
        /// </summary>
        /// <param name="id"></param>
        public void CloseSession(string id)
        {
            IMHandler handler = GetSession(id);
            if (handler is null)
            {
                return;
            }

            handler.Dispose();
            handlers.Remove(handler);
        }

        /// <summary>
        /// 获取会话
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMHandler GetSession(string id)
        {
            return handlers.Find(x => x.id == id);
        }

        /// <summary>
        /// 发送聊天信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        public void SendChat(string id, string content)
        {
            IMHandler handler = GetSession(id);
            if (handler is null)
            {
                Debug.Log("Not Find:" + id);
                return;
            }

            handler.SendChat(content);
        }
    }
}