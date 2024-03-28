using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using ZGame.Sound;

namespace ZGame.Game
{
    public class AudioSourceComponent : IComponent
    {
        public AudioSource source;
        public bool IsPlay => source.isPlaying;

        public override void OnAwake(params object[] args)
        {
            GameObjectComponent gameObjectComponent = this.entity.GetComponent<GameObjectComponent>();
            if (gameObjectComponent is null)
            {
                return;
            }

            if (gameObjectComponent.gameObject.TryGetComponent<AudioSource>(out source) is false)
            {
                source = gameObjectComponent.gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// 设置3D音效最大范围
        /// </summary>
        /// <param name="range"></param>
        public AudioSourceComponent SetAudioRange(float min, float max)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.2f, 1);
            curve.AddKey(1, 0);
            source.rolloffMode = AudioRolloffMode.Linear;
            source.SetCustomCurve(AudioSourceCurveType.SpatialBlend, curve);
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
            source.SetCustomCurve(AudioSourceCurveType.Spread, curve);
            source.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, curve);
            source.minDistance = min;
            source.maxDistance = max;
            source.spatialBlend = 0.8f;
            return this;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip"></param>
        public void Play(AudioClip clip)
        {
            Play(clip, 1);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        public void Play(AudioClip clip, float volume)
        {
            source.clip = clip;
            source.volume = volume;
            source.Play();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip"></param>
        public UniTask PlayAsync(AudioClip clip)
        {
            return PlayAsync(clip, 1);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        public async UniTask PlayAsync(AudioClip clip, float volume)
        {
            Play(clip, volume);
            Debug.Log("clip time:" + clip.length);
            await UniTask.WaitUntilValueChanged(this, (x) => IsPlay == false);
            Stop();
        }

        /// <summary>
        /// 设置混音器
        /// </summary>
        /// <param name="mixer"></param>
        public void SetMixer(AudioMixerGroup mixer)
        {
            source.outputAudioMixerGroup = mixer;
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            source.volume = volume;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            source.clip = null;
            source.Stop();
        }

        /// <summary>
        /// 暂停播放音效
        /// </summary>
        public void Pause()
        {
            source.Pause();
        }

        /// <summary>
        /// 继续播放音效
        /// </summary>
        public void Resume()
        {
            source.UnPause();
        }
    }
}