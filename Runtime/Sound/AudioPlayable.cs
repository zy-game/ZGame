using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{


    public class AudioPlayable : Playable
    {
        public AudioClip clip;
        public ResHandle _resHandle;

        public override void Dispose()
        {
            base.Dispose();
            clip = null;
            _resHandle?.Release();
            _resHandle = null;
        }
    }
}