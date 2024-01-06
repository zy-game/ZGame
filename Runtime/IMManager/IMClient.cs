using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Inworld;
using Inworld.Grpc;
using Newtonsoft.Json;
using UnityEngine;
using AudioChunk = Inworld.Packets.AudioChunk;
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;

namespace ZGame.IM
{
    public class IMClient : IDisposable
    {
        private string _agent;
        private string _character;
        private IMHandler handler;
        private InworldConfig _setting;
        private InworldClient _client;
        private EmotionEvent.Types.SpaffCode _code = EmotionEvent.Types.SpaffCode.Neutral;
        private UniTaskCompletionSource<bool> _completionSource;

        public string robot { get; private set; }
        public List<IMChatItem> chats { get; private set; }
        public string id { get; } = Guid.NewGuid().ToString();

        public IMClient(IMHandler handler)
        {
            this.handler = handler;
            this.handler.id = id;
        }


        public void SendChat(string text)
        {
            if (_client == null || _client.SessionStarted is false)
            {
                _client.StartSession();
            }

            TextEvent packet = new TextEvent();
            packet.SourceType = Inworld.Grpc.TextEvent.Types.SourceType.TypedIn;
            packet.Final = true;
            packet.Text = text;
            packet.Routing = Routing.FromPlayerToAgent(_agent);
            _client.SendEvent(new RPCMessage(packet));
            OnRecvieChatHandle(new IMChatItem(String.Empty, text));
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
            Debug.Log("会话已打开：" + id);
            _completionSource.TrySetResult(true);
        }

        public UniTask<bool> Open(string character, InworldConfig setting)
        {
            _setting = setting;
            _character = character;
            chats = new List<IMChatItem>();
            _client = new InworldClient();
            _client.RuntimeEvent += OnRecvieEventHandler;
            robot = Path.GetFileNameWithoutExtension(_character);
            _completionSource = new UniTaskCompletionSource<bool>();
            _client.GetAppAuth(_setting.workspace, _setting.key, _setting.secret);

            return _completionSource.Task;
        }

        public void OnRecvieMessage()
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
            if (packet is Inworld.Packets.EmotionEvent emotionEvent)
            {
                _code = emotionEvent.SpaffCode;
                return;
            }

            if (_client.GetAudioChunk(out AudioChunk audioChunk))
            {
                AudioClip clip = WavUtility.ToAudioClip(audioChunk.Chunk.ToByteArray());
                chat = new IMChatItem(robot, clip, _code);
            }
            else if (packet is TextEvent textEvent)
            {
                Debug.Log(JsonConvert.SerializeObject(textEvent));
                if (textEvent.SourceType == Inworld.Grpc.TextEvent.Types.SourceType.SpeechToText)
                {
                    return;
                }

                chat = new IMChatItem(robot, textEvent.Text, _code);
            }

            if (chat is null)
            {
                return;
            }


            OnRecvieChatHandle(chat);
        }

        public void SendAudio(ByteString audioChunk)
        {
            if (_client is null)
            {
                return;
            }

            if (_client.SessionStarted is false)
            {
                _client.StartSession();
            }

            Routing routing = Routing.FromPlayerToAgent(_agent);
            _client.SendAudio(new AudioChunk(audioChunk, routing));
        }

        public void OnStartAudioChat()
        {
            if (_client is null)
            {
                return;
            }

            if (_client.SessionStarted is false)
            {
                _client.StartSession();
            }

            _client.StartAudio(Routing.FromPlayerToAgent(_agent));
        }

        public void OnStopAudioChat()
        {
            if (_client is null)
            {
                return;
            }

            if (_client.SessionStarted is false)
            {
                _client.StartSession();
            }

            _client.EndAudio(Routing.FromPlayerToAgent(_agent));
        }

        public void OnRecvieChatHandle(IMChatItem chat)
        {
            chats.Add(chat);
            handler.OnRecvieChatHandle(chat);
        }

        public void Dispose()
        {
            _client?.Destroy();
        }
    }
}