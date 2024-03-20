using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using ZGame.Sound;

namespace ZGame.Game.Common
{
    public class AudioSourceComponent : EntityComponent
    {
        private AudioSource _source;
        public bool IsPlay { get; private set; }


        public override void OnAwake(params object[] args)
        {
            GameObjectComponent gameObjectComponent = this.entity.GetComponent<GameObjectComponent>();
            if (gameObjectComponent is null)
            {
                return;
            }

            if (gameObjectComponent.gameObject.TryGetComponent<AudioSource>(out _source) is false)
            {
                _source = gameObjectComponent.gameObject.AddComponent<AudioSource>();
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
            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.SetCustomCurve(AudioSourceCurveType.SpatialBlend, curve);
            _source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
            _source.SetCustomCurve(AudioSourceCurveType.Spread, curve);
            _source.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, curve);
            _source.minDistance = min;
            _source.maxDistance = max;
            _source.spatialBlend = 0.8f;
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
            IsPlay = true;
            _source.clip = clip;
            _source.volume = volume;
            _source.Play();
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
            await UniTask.Delay((int)(clip.length * 1000f));
            Stop();
        }

        /// <summary>
        /// 设置混音器
        /// </summary>
        /// <param name="mixer"></param>
        public void SetMixer(AudioMixerGroup mixer)
        {
            _source.outputAudioMixerGroup = mixer;
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            _source.volume = volume;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            _source.clip = null;
            _source.Stop();
        }

        /// <summary>
        /// 暂停播放音效
        /// </summary>
        public void Pause()
        {
            _source.Pause();
        }

        /// <summary>
        /// 继续播放音效
        /// </summary>
        public void Resume()
        {
            _source.UnPause();
        }
    }
}