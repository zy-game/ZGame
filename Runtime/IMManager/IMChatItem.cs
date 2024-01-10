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
        public AudioClip clip { get; private set; }

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
            this.clip = clip;
            this.spaffCode = gesture;
        }

        public void Dispose()
        {
            clip = null;
        }
    }
}