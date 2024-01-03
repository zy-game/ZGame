using System;
using System.Collections.Generic;
using System.Linq;
using Ai.Inworld.Studio.V1Alpha;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using UnityEngine;
using ZGame;
using Inworld;
using SpaffCode = Inworld.Grpc.EmotionEvent.Types.SpaffCode;

namespace ZGame.IM
{
    public class IMMgr : Singleton<IMMgr>
    {
        private List<IMClient> handlers = new();

        private int m_AudioRate = 16000;
        private int m_BufferSeconds = 1;
        private string m_CurrentDevice = null;
        private AudioClip m_Recording;
        public bool isRecording = false;
        private const int k_SizeofInt16 = sizeof(short);
        private int m_BufferSize;
        private float m_CDCounter;
        private int m_LastPosition;
        private float[] clipData;
        private byte[] chunk;
        private UniTaskCompletionSource<bool> _completionSource;

        public IMClient current { get; private set; }


        protected override void OnAwake()
        {
            m_BufferSize = m_BufferSeconds * m_AudioRate;
            chunk = new byte[m_BufferSize * 1 * k_SizeofInt16];
            clipData = new float[m_BufferSize * 1];
            m_CurrentDevice = Microphone.devices.FirstOrDefault();
        }

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
            OnCheckRecording();
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].OnRecvieMessage();
            }
        }

        /// <summary>
        /// 设置新会话
        /// </summary>
        /// <param name="handler"></param>
        public async UniTask<bool> Setup(string character, InworldConfig config, IMHandler handler)
        {
            IMClient client = new IMClient(handler);
            bool success = await client.Open(character, config);
            if (success)
            {
                handlers.Add(client);
            }

            return success;
        }

        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <param name="character"></param>
        /// <param name="setting"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<bool> Setup<T>(string character, InworldConfig setting) where T : IMHandler
        {
            T handle = Activator.CreateInstance<T>();
            return await Setup(character, setting, handle);
        }

        /// <summary>
        /// 关闭会话
        /// </summary>
        /// <param name="id"></param>
        public void CloseSession(string id)
        {
            IMClient handler = GetSession(id);
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
        public IMClient GetSession(string id)
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
            IMClient handler = GetSession(id);
            if (handler is null)
            {
                Debug.Log("Not Find:" + id);
                return;
            }

            handler.SendChat(content);
        }

        public void StartRecording()
        {
#if !UNITY_WEBGL
            if (Microphone.IsRecording(m_CurrentDevice))
            {
                return;
            }

            isRecording = true;
            m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
            current.OnStartAudioChat();
#endif
        }


        private void OnCheckRecording()
        {
#if!UNITY_WEBGL
            if (isRecording is false)
            {
                return;
            }

            if (!Microphone.IsRecording(m_CurrentDevice))
                StartRecording();
            if (m_CDCounter <= 0)
            {
                m_CDCounter = 0.1f;
                Collect();
            }

            m_CDCounter -= Time.deltaTime;
#endif
        }

        public virtual void StopRecording()
        {
            isRecording = false;
#if !UNITY_WEBGL
            current.OnStopAudioChat();
            Microphone.End(null);
            IMChatItem chatItem = new IMChatItem(current.robot, m_Recording, SpaffCode.Neutral);
            current.chats.Add(chatItem);
            current.OnRecvieChatHandle(chatItem);
#endif
        }


        private int GetAudioData()
        {
            int nSize = 0;
#if !UNITY_WEBGL
            int nPosition = Microphone.GetPosition(m_CurrentDevice);
            if (nPosition < m_LastPosition)
                nPosition = m_BufferSize;
            if (nPosition <= m_LastPosition)
                return -1;
            nSize = nPosition - m_LastPosition;
            if (!m_Recording.GetData(clipData, m_LastPosition))
                return -1;
            m_LastPosition = nPosition % m_BufferSize;
#endif
            return nSize;
        }

        private void Collect()
        {
            int nSize = GetAudioData();
            if (nSize < 0)
                return;
            WavUtility.ConvertAudioClipDataToInt16ByteArray(clipData, nSize * m_Recording.channels, chunk);
            ByteString audioData = ByteString.CopyFrom(chunk, 0, nSize * m_Recording.channels * k_SizeofInt16);
            current.SendAudio(audioData);
        }
    }
}