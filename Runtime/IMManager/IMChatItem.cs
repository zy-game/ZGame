using System;
using UnityEngine;

namespace ZGame.IM
{
    public enum SpaffCode : byte
    {
        Neutral,
        Disgust,
        Contempt,
        Belligerence,
        Domineering,
        Criticism,
        Anger,
        Tension,
        TenseHumor,
        Defensiveness,
        Whining,
        Sadness,
        Stonewalling,
        Interest,
        Validation,
        Affection,
        Humor,
        Surprise,
        Joy,
    }

    public class IMChatItem : IDisposable
    {
        public string text { get; }
        public string nick { get; }
        public SpaffCode spaffCode { get; }
        public AudioClip clip { get; private set; }

        public IMChatItem(string nick)
        {
            this.nick = nick;
        }

        public IMChatItem(string nick, string content, SpaffCode gesture = SpaffCode.Neutral) : this(nick)
        {
            this.text = content;
            this.spaffCode = gesture;
        }

        public IMChatItem(string nick, AudioClip clip, SpaffCode gesture = SpaffCode.Neutral) : this(nick)
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