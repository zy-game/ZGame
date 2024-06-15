using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using ZGame.Sound;


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