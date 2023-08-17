using System.Collections.Generic;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Sound
{
    public class SoundManager : Single<SoundManager>
    {
        private List<AudioPlayer> players = new List<AudioPlayer>();

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
            AudioPlayer player = Engine.Class.Loader<AudioPlayer>();
            player.SetOptions(options);
            players.Add(player);
        }

        public void PlaySound(string soundName, string optionsName = "default")
        {
            AudioPlayer player = players.Find(x => x.name == optionsName);
            if (player is null)
            {
                Engine.Console.Log("未找到音效配置");
                return;
            }

            player.Play(soundName);
        }

        public void PauseSound(string soundName)
        {
            AudioPlayer player = players.Find(x => x.clipName == soundName);
            if (player is null)
            {
                return;
            }

            player.Pause();
        }

        public void ResumeSound(string soundName)
        {
            AudioPlayer player = players.Find(x => x.clipName == soundName);
            if (player is null)
            {
                return;
            }

            player.Resume();
        }

        public void StopSound(string soundName)
        {
            AudioPlayer player = players.Find(x => x.clipName == soundName);
            if (player is null)
            {
                return;
            }

            player.Stop();
        }

        public SoundOptions GetSoundPlayOptions(string optionsName)
        {
            AudioPlayer player = players.Find(x => x.name == optionsName);
            if (player is null)
            {
                Engine.Console.Log("未找到音效配置");
                return default;
            }

            return player.options;
        }
    }

    class AudioPlayer : IReference
    {
        public string name => options.title;
        public string clipName { get; private set; }
        public SoundOptions options { get; private set; }
        private AudioSource source;

        public void SetOptions(SoundOptions options)
        {
            this.options = options;
            source = new GameObject($"Sound_{options.title}").AddComponent<AudioSource>();
            source.mute = options.mute == Switch.On;
            source.volume = this.options.volumen;
            source.priority = this.options.priority;
            source.loop = this.options.loop == Switch.On;
            GameObject.DontDestroyOnLoad(source.gameObject);
        }

        public void Release()
        {
            GameObject.DestroyImmediate(source.gameObject);
        }

        public void Play(string clipName)
        {
            IRequestAssetExecuteResult<AudioClip> requestAssetExecuteResult = Engine.Resource.LoadAsset<AudioClip>(clipName);
            if (requestAssetExecuteResult.asset == null)
            {
                Engine.Console.Log("加载音效失败:" + clipName);
                return;
            }

            this.clipName = clipName;
            source.clip = requestAssetExecuteResult.asset;
            source.Play();
        }

        public void Pause()
        {
            source.Pause();
        }

        public void Resume()
        {
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }
    }
}