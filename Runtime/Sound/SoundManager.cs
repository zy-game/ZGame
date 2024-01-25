using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame;
using ZGame.Resource;

namespace ZGame.Sound
{
    public enum PlayState
    {
        None,
        Playing,
        Paused,
        Complete,
    }

    /// <summary>
    /// 音效管理器
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        private int _rate;
        private float _volume;
        private int _position;
        private int _limit_time;
        private string _divName;
        private int sensitivity;
        private int _talkPosition;
        private int _BufferSize;
        private float _recordStartTime;
        private float[] _recordSamples;
        private float _talkingSplitTime;
        private bool _isRecording = false;
        private bool _isRealTalking = false;
        private AudioClip _recordClip = null;
        private Action<AudioClip> _talkingCallback;
        private const string BACK_MUSIC = "BackMusic";
        private Action<AudioClip, int> _recordingCallback;
        private const string EFFECT_SOUND = "EffectSound";
        private List<SoundPlayer> _handles = new List<SoundPlayer>();


        /// <summary>
        /// 录音设备名
        /// </summary>
        public string micName
        {
            get { return _divName; }
        }

        /// <summary>
        /// 最大录音时长
        /// </summary>
        public int limitTime
        {
            get { return _limit_time; }
        }

        /// <summary>
        /// 录音码率
        /// </summary>
        public int rate
        {
            get { return _rate; }
        }

        /// <summary>
        /// 录音数据位置
        /// </summary>
        public int position
        {
            get { return _position; }
        }

        /// <summary>
        /// 当前音量
        /// </summary>
        public float volume
        {
            get { return _volume; }
        }

        protected override void OnAwake()
        {
            _divName = Microphone.devices.FirstOrDefault();
            _limit_time = 60;
            _rate = 16000;
            _recordSamples = new float[128];
            BehaviourScriptable.instance.gameObject.AddComponent<AudioListener>();
            AddSoundPlayer(BACK_MUSIC, true);
            AddSoundPlayer(EFFECT_SOUND, false);
        }

        protected override void OnUpdate()
        {
            CheckSoundPlayer();
            HasRecordTimeout();
            OnUpdateTalking();
        }

        private void CheckSoundPlayer()
        {
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                _handles[i].OnUpdate();
            }
        }

        private void HasRecordTimeout()
        {
            if (_isRecording is false)
            {
                return;
            }

            if (Microphone.IsRecording(_divName) is false || _limit_time <= 0)
            {
                return;
            }

            if (Time.realtimeSinceStartup - _recordStartTime < _limit_time)
            {
                return;
            }

            StopRecordingSound(false);
        }


        /// <summary>
        /// 开始录音
        /// </summary>
        public void StartRecordingSound(int timeout, Action<AudioClip, int> callback)
        {
            _isRecording = true;
            this._limit_time = timeout;
            this._recordingCallback = callback;
            _recordStartTime = Time.realtimeSinceStartup;
            _recordClip = Microphone.Start(_divName, false, _limit_time, _rate);
            Debug.Log("Start Recording:" + _divName + " :" + Microphone.IsRecording(_divName));
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopRecordingSound(bool isCancel)
        {
            _position = Microphone.GetPosition(_divName);
            Microphone.End(_divName);
            _isRecording = false;
            if (isCancel is false)
            {
                _recordingCallback?.Invoke(GetRealTimeRecordingSound(), _position);
            }

            _recordClip = null;
            Debug.Log("End Recording:" + _divName + " :" + Microphone.IsRecording(_divName));
        }

        /// <summary>
        /// 获取录音数据
        /// </summary>
        /// <returns></returns>
        AudioClip GetRealTimeRecordingSound()
        {
            if (_recordClip == null)
            {
                return default;
            }

            _recordSamples = new float[_recordClip.samples];
            byte[] chunk = new byte[position * sizeof(short)];
            _recordClip.GetData(_recordSamples, 0);
            AudioClip clip = AudioClip.Create("mySound", _recordClip.channels * position, _recordClip.channels, _rate, false, false);
            float[] temp = new float[_recordClip.channels * position];
            Array.Copy(_recordSamples, 0, temp, 0, _recordClip.channels * position);
            clip.SetData(temp, 0);
            return clip;
        }

        /// <summary>
        /// 开始实时语音
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <param name="callback"></param>
        public void OpenRealTimeTalking(int milliseconds, Action<AudioClip> callback)
        {
            if (Microphone.IsRecording(_divName))
            {
                StopRecordingSound(true);
            }

            _isRealTalking = true;
            _BufferSize = 1 * _rate;
            _talkingCallback = callback;
            _recordSamples = new float[_BufferSize];
            _talkingSplitTime = milliseconds / 1000f;
            _recordStartTime = Time.realtimeSinceStartup;
            _recordClip = Microphone.Start(_divName, true, 1, _rate);
            _position = Microphone.GetPosition(_divName);
            Debug.Log("Start Talking");
        }

        public void CloseRealTimeTalking()
        {
            OnUpdateTalking();
            _isRealTalking = false;
            _recordClip = null;
            Debug.Log("Stop Talking");
        }

        private void OnUpdateTalking()
        {
            if (_isRealTalking is false)
            {
                return;
            }

            if (Microphone.IsRecording(_divName) is false)
            {
                return;
            }

            if (Time.realtimeSinceStartup - _recordStartTime < _talkingSplitTime)
            {
                return;
            }

            _recordStartTime = Time.realtimeSinceStartup;
            int nSize = GetAudioData();
            if (nSize < 0)
            {
                return;
            }

            AudioClip clip = AudioClip.Create("mySound", nSize * _recordClip.channels, _recordClip.channels, _rate, false, false);
            clip.SetData(_recordSamples, 0);
            _talkingCallback?.Invoke(clip);
        }

        protected int GetAudioData()
        {
            int nSize = 0;
            int nPosition = Microphone.GetPosition(_divName);
            if (nPosition < _position)
                nPosition = _BufferSize;
            if (nPosition <= _position)
                return -1;
            nSize = nPosition - _position;
            if (!_recordClip.GetData(_recordSamples, _position))
                return -1;
            _position = nPosition % _BufferSize;
            return nSize;
        }

        /// <summary>
        /// 添加播放器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public SoundPlayer AddSoundPlayer(string name, bool isLoop)
        {
            SoundPlayer player = GetSoundPlayer(name);
            if (player is not null)
            {
                return player;
            }

            _handles.Add(player = new SoundPlayer(name, isLoop));
            return player;
        }

        /// <summary>
        /// 获取指定的播放器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SoundPlayer GetSoundPlayer(string name)
        {
            return _handles.Find(x => x.name == name);
        }

        /// <summary>
        /// 移除播放器
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            SoundPlayer handle = GetSoundPlayer(name);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
            _handles.Remove(handle);
        }

        /// <summary>
        /// 特殊音效是否在播放
        /// </summary>
        /// <returns></returns>
        public bool EffectPlaying()
        {
            return IsPlaying(EFFECT_SOUND);
        }

        /// <summary>
        /// 背景音效是否正在播放
        /// </summary>
        /// <returns></returns>
        public bool BackSoundPlaying()
        {
            return IsPlaying(BACK_MUSIC);
        }

        /// <summary>
        /// 指定的音效是否正在播放
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsPlaying(string name)
        {
            return _handles.Find(x => x.name == name).isPlaying;
        }

        /// <summary>
        /// 在背景音效队列中加入音效
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="playCallback"></param>
        public void PlayBackSound(string clipName, bool isNow = false, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(BACK_MUSIC);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, true);
            }

            handle.Play(clipName, isNow, playCallback);
        }

        /// <summary>
        /// 在背景音效队列中加入音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="playCallback"></param>
        public void PlayBackSound(AudioClip clip, bool isNow = false, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(BACK_MUSIC);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, true);
            }

            handle.Play(clip, isNow, playCallback);
        }

        /// <summary>
        /// 在默认的音效播放器队列中加入音效
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="playCallback"></param>
        public void PlayEffectSound(string clipName, bool isNow = false, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, false);
            }

            handle.Play(clipName, isNow, playCallback);
        }

        /// <summary>
        /// 在默认的音效播放器队列中加入音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="playCallback"></param>
        public void PlayEffectSound(AudioClip clip, bool isNow = false, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, false);
            }

            handle.Play(clip, isNow, playCallback);
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clipName"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public void PlaySound(string playName, string clipName, bool isLoop, bool isNow = false, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(playName);
            if (handle is null)
            {
                handle = AddSoundPlayer(playName, isLoop);
            }

            handle.Play(clipName, isNow, playCallback);
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public void PlaySound(string playName, AudioClip clip, bool isLoop, bool isNow = false, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(playName);
            if (handle is null)
            {
                handle = AddSoundPlayer(playName, isLoop);
            }

            handle.Play(clip, isNow, playCallback);
        }

        /// <summary>
        /// 暂停指定的音效
        /// </summary>
        /// <param name="clipName"></param>
        public void PauseSound(string clipName)
        {
            SoundPlayer handle = _handles.Find(x => x.clipName == clipName);
            if (handle is null)
            {
                return;
            }

            handle.Pause();
        }

        /// <summary>
        /// 暂停指定的音效播放器
        /// </summary>
        /// <param name="playName"></param>
        public void Pause(string playName)
        {
            SoundPlayer handle = GetSoundPlayer(playName);
            if (handle is null)
            {
                return;
            }

            handle.Pause();
        }

        /// <summary>
        /// 停止指定的音效
        /// </summary>
        /// <param name="clipName"></param>
        public void StopSound(string clipName)
        {
            SoundPlayer handle = _handles.Find(x => x.clipName == clipName);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
        }

        /// <summary>
        /// 停止指定的播放器
        /// </summary>
        /// <param name="playName"></param>
        public void Stop(string playName)
        {
            SoundPlayer handle = GetSoundPlayer(playName);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
        }

        /// <summary>
        /// 停止所有音效的播放
        /// </summary>
        public void StopAll()
        {
            SoundPlayer handle = GetSoundPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
        }

        /// <summary>
        /// 根据音效名设置音效播放
        /// </summary>
        /// <param name="clipName"></param>
        public void ResumeSound(string clipName)
        {
            SoundPlayer handle = _handles.Find(x => x.clipName == clipName);
            if (handle is null)
            {
                return;
            }

            handle.Resume();
        }

        /// <summary>
        /// 回复指定的播放器播放
        /// </summary>
        /// <param name="playName"></param>
        public void Resume(string playName)
        {
            SoundPlayer handle = GetSoundPlayer(playName);
            if (handle is null)
            {
                return;
            }

            handle.Resume();
        }

        /// <summary>
        /// 设置自动的播放器音量
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volume"></param>
        public void SetVolume(string name, float volume)
        {
            SoundPlayer handle = GetSoundPlayer(name);
            if (handle is null)
            {
                return;
            }

            handle.SetVolume(volume);
        }
    }
}