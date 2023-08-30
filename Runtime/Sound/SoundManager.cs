using System.Collections.Generic;

namespace ZEngine.Sound
{
     class SoundManager : Single<SoundManager>
    {
        private List<AudioPlayerHandle> players = new List<AudioPlayerHandle>();

        public SoundManager()
        {
            if (SoundPlayOptions.instance.optionsList is null || SoundPlayOptions.instance.optionsList.Count is 0)
            {
                return;
            }

            for (int i = 0; i < SoundPlayOptions.instance.optionsList.Count; i++)
            {
                SetPlayOptions(SoundPlayOptions.instance.optionsList[i]);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            players.ForEach(Engine.Class.Release);
            Engine.Console.Log("关闭音效管理器");
        }

        public void SetPlayOptions(SoundOptions options)
        {
            AudioPlayerHandle player = Engine.Class.Loader<AudioPlayerHandle>();
            player.SetOptions(options);
            players.Add(player);
        }

        public void PlaySound(string soundName, string optionsName = "default")
        {
            AudioPlayerHandle player = players.Find(x => x.name == optionsName);
            if (player is null)
            {
                Engine.Console.Log("未找到音效配置");
                return;
            }

            player.Play(soundName);
        }

        public void PauseSound(string soundName)
        {
            AudioPlayerHandle player = players.Find(x => x.clipName == soundName);
            if (player is null)
            {
                return;
            }

            player.Pause();
        }

        public void ResumeSound(string soundName)
        {
            AudioPlayerHandle player = players.Find(x => x.clipName == soundName);
            if (player is null)
            {
                return;
            }

            player.Resume();
        }

        public void StopSound(string soundName)
        {
            AudioPlayerHandle player = players.Find(x => x.clipName == soundName);
            if (player is null)
            {
                return;
            }

            player.Stop();
        }

        public SoundOptions GetSoundPlayOptions(string optionsName)
        {
            AudioPlayerHandle player = players.Find(x => x.name == optionsName);
            if (player is null)
            {
                Engine.Console.Log("未找到音效配置");
                return default;
            }

            return player.options;
        }
    }
}