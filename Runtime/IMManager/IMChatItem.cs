using System;
using Inworld.Grpc;
using UnityEngine;

namespace ZGame.IM
{
    public class IMChatItem : IDisposable
    {
        public string text { get; }
        public string nick { get; }
        public EmotionEvent.Types.SpaffCode spaffCode { get; }
        public AudioClip audio { get; private set; }

        public IMChatItem(string nick)
        {
            this.nick = nick;
        }

        public IMChatItem(string nick, string content, EmotionEvent.Types.SpaffCode gesture = EmotionEvent.Types.SpaffCode.Neutral) : this(nick)
        {
            this.text = content;
            this.spaffCode = gesture;
        }

        public IMChatItem(string nick, AudioClip clip, EmotionEvent.Types.SpaffCode gesture = EmotionEvent.Types.SpaffCode.Neutral) : this(nick)
        {
            this.audio = clip;
            this.spaffCode = gesture;
        }

        public void Dispose()
        {
            audio = null;
        }
    }
}