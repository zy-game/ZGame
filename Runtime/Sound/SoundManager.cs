using UnityEngine.Internal;

namespace ZEngine.Sound
{
    public class SoundManager:Single<SoundManager>
    {
        public void SetPlayOptions(ISoundPlayOptions options)
        {
            
        }

        public ISoundPlayHandle PlaySound(string soundName, [DefaultValue("default")] string optionsName)
        {
            return default;
        }

        public ISoundPlayHandle PlayEffectSound(string soundName, [DefaultValue("default")] string optionsName)
        {
            return default;
        }

        public ISoundPlayOptions GetSoundPlayOptions(string optionsName)
        {
            return default;
        }
    }
}