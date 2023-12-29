using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Inworld;
using Inworld.Grpc;
using Newtonsoft.Json;
using UnityEngine;
using ZGame;
using Inworld;
using AudioChunk = Inworld.Packets.AudioChunk;
using EmotionEvent = Inworld.Packets.EmotionEvent;
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;
using SpaffCode = Inworld.Grpc.EmotionEvent.Types.SpaffCode;

namespace ZGame.IM
{
    public abstract class IMHandler : IDisposable
    {
        private string _agent;
        private string robot;
        private string _character;
        private InworldConfig _setting;
        private InworldClient _client;
        private List<IMChatItem> _chats;
        private SpaffCode _code = SpaffCode.Neutral;

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

        public List<IMChatItem> Chats
        {
            get { return _chats; }
        }

        public string id { get; private set; }

        public UniTask<bool> Open(string character, InworldConfig setting)
        {
            _completionSource = new UniTaskCompletionSource<bool>();
            this.id = Guid.NewGuid().ToString();
            _setting = setting;
            _character = character;
            _chats = new List<IMChatItem>();
            _client = new InworldClient();
            m_BufferSize = m_BufferSeconds * m_AudioRate;
            chunk = new byte[m_BufferSize * 1 * k_SizeofInt16];
            clipData = new float[m_BufferSize * 1];
            _client.RuntimeEvent += OnRecvieEventHandler;
            robot = Path.GetFileNameWithoutExtension(_character);
            m_CurrentDevice = Microphone.devices.FirstOrDefault();
            _client.GetAppAuth(_setting.workspace, _setting.key, _setting.secret);
            return _completionSource.Task;
        }


        public List<IMChatItem> GetChats()
        {
            return _chats;
        }

        public void OnUpdate()
        {
            OnRecvieMessage();
            OnCheckRecording();
        }

        private void OnRecvieEventHandler(RuntimeStatus arg1, string arg2)
        {
            switch (arg1)
            {
                case RuntimeStatus.InitFailed:
                    _completionSource.TrySetResult(false);
                    break;
                case RuntimeStatus.InitSuccess:
                    LoadScene();
                    break;
            }
        }

        private async void LoadScene()
        {
            LoadSceneResponse response = await _client.LoadScene(_setting.scene);
            if (response is null)
            {
                _completionSource.TrySetResult(false);
                return;
            }

            foreach (var agent in response.Agents)
            {
                if (agent.BrainName != _character)
                {
                    continue;
                }

                _agent = agent.AgentId;
            }

            if (_agent.IsNullOrEmpty())
            {
                Debug.Log("不存在角色：" + _character);
                _completionSource.TrySetResult(false);
                return;
            }

            _client.StartSession();
            _completionSource.TrySetResult(true);
        }

        private void OnRecvieMessage()
        {
            if (_client == null || _client.SessionStarted is false)
            {
                return;
            }

            if (_client.GetIncomingEvent(out InworldPacket packet) is false)
            {
                return;
            }

            IMChatItem chat = default;
            if (packet is EmotionEvent emotionEvent)
            {
                _code = emotionEvent.SpaffCode;
                return;
            }

            if (_client.GetAudioChunk(out AudioChunk audioChunk))
            {
                AudioClip clip = WavUtility.ToAudioClip(audioChunk.Chunk.ToByteArray());
                _chats.Add(chat = new IMChatItem(robot, clip, _code));
            }
            else if (packet is TextEvent textEvent)
            {
                _chats.Add(chat = new IMChatItem(robot, textEvent.Text, _code));
            }

            if (chat is null)
            {
                return;
            }

            OnRecvieChatHandle(chat);
        }

        protected virtual void OnRecvieChatHandle(IMChatItem chatItem)
        {
        }

        public void SendChat(string text)
        {
            if (_client == null || _client.SessionStarted is false)
            {
                Debug.Log("会话已关闭：" + id);
                return;
            }

            TextEvent packet = new TextEvent();
            packet.SourceType = Inworld.Grpc.TextEvent.Types.SourceType.TypedIn;
            packet.Final = true;
            packet.Text = text;
            packet.Routing = Routing.FromPlayerToAgent(_agent);
            _client.SendEvent(new RPCMessage(packet));
            IMChatItem chat = default;
            _chats.Add(chat = new IMChatItem(String.Empty, text));
            OnRecvieChatHandle(chat);
        }

        public virtual void StartRecording()
        {
#if !UNITY_WEBGL
            if (Microphone.IsRecording(m_CurrentDevice))
            {
                return;
            }

            isRecording = true;
            m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
            _client.StartAudio(Routing.FromPlayerToAgent(_agent));
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
            _client.EndAudio(Routing.FromPlayerToAgent(_agent));
            Microphone.End(null);
            IMChatItem chatItem = new IMChatItem(robot, m_Recording, SpaffCode.Neutral);
            _chats.Add(chatItem);
            OnRecvieChatHandle(chatItem);
#endif
        }


        protected int GetAudioData()
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
            Routing routing = Routing.FromPlayerToAgent(_agent);
            _client.SendAudio(new AudioChunk(audioData, routing));
        }

        public virtual void Dispose()
        {
            StopRecording();
            _client.Destroy();
        }
    }
}