using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using ZGame.Sound;
using NotImplementedException = System.NotImplementedException;

namespace ZGame.Game
{
    public class AudioSourceComponent : IComponent
    {
        public AudioSource source;

        public void Release()
        {
            source = null;
        }
    }
}