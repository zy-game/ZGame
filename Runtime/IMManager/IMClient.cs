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
        bool isOpened { get; }
        UniTask<bool> Open();
        void OnRecvieMessage();
        void SendChat(string text);
        void SendAudio(AudioClip clip, int lenght);
    }
}