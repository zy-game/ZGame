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
    public interface IMClient : IDisposable
    {
        string id { get; }
        bool isOpen { get; }
        UniTask<bool> Open(params object[] args);
        void SendChat(string text);
        void StartAudio();
        void SendAudio(byte[] clip);
        void EndAudio();
    }
}