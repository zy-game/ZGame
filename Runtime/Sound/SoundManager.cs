namespace ZEngine.Sound
{
    public class SoundManager : Single<SoundManager>
    {
        public void SetPlayOptions(SoundOptions options)
        {
        }

        public IAudioPlayExecuteHandle PlaySound(string soundName, string optionsName = "default")
        {
            return default;
        }

        public IAudioPlayExecuteHandle PlayEffectSound(string soundName, string optionsName = "default")
        {
            return default;
        }

        public SoundOptions GetSoundPlayOptions(string optionsName)
        {
            return default;
        }
    }
}