using System;
using System.Collections.Generic;
using System.IO;
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

    public interface IRecordAudioHandle : IDisposable
    {
        void StartRecord();
        void StopRecord();
        void Recording(byte[] bytes);


        public static IRecordAudioHandle OnCreate(Action<byte[]> callback)
        {
            return new Default(callback);
        }

        class Default : IRecordAudioHandle
        {
            private Action<byte[]> callback;

            public Default(Action<byte[]> callback)
            {
                this.callback = callback;
            }

            public void Dispose()
            {
                callback = null;
            }

            public void StartRecord()
            {
            }

            public void StopRecord()
            {
            }

            public void Recording(byte[] bytes)
            {
                if (callback is null)
                {
                    return;
                }

                callback(bytes);
            }
        }
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
        private float _recordStartTime;
        private bool _isRecording = false;
        private IRecordAudioHandle records;
        private AudioClip _recordClip = null;
        private const string BACK_MUSIC = "BackMusic";
        private const string EFFECT_SOUND = "EffectSound";
        private List<SoundPlayer> _handles = new List<SoundPlayer>();


        const int k_SizeofInt16 = sizeof(short);
        private float m_CDCounter;
        private int m_BufferSeconds = 1;
        private bool isHeader = false;
        private int m_BufferSize;
        private byte[] m_ByteBuffer;
        private float[] m_FloatBuffer;
        private int BUFFER_SIZE_LIMIT = 6400;

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

        public bool Recording
        {
            get { return _isRecording; }
        }

        protected override void OnAwake()
        {
            _rate = 16000;
            _limit_time = 60;
            m_BufferSize = m_BufferSeconds * _rate;
            m_FloatBuffer = new float[m_BufferSize * 1];
            AddSoundPlayer(BACK_MUSIC, true);
            _divName = Microphone.devices.FirstOrDefault();
            AddSoundPlayer(EFFECT_SOUND, false);
            m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt16];
            records = IRecordAudioHandle.OnCreate(new Action<byte[]>(_ => { }));
            BehaviourScriptable.instance.gameObject.AddComponent<AudioListener>();
        }

        protected override void OnUpdate()
        {
            CheckSoundPlayer();
            HasRecordTimeout();
            OnSplitAudioClip();
        }

        private void CheckSoundPlayer()
        {
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                _handles[i].OnUpdate();
            }
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
        public void PlayBackSound(string clipName, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(BACK_MUSIC);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, true);
            }

            handle.Play(clipName, playCallback);
        }

        /// <summary>
        /// 在背景音效队列中加入音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="playCallback"></param>
        public void PlayBackSound(AudioClip clip, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(BACK_MUSIC);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, true);
            }

            handle.Play(clip, playCallback);
        }

        /// <summary>
        /// 在默认的音效播放器队列中加入音效
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="playCallback"></param>
        public void PlayEffectSound(string clipName, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, false);
            }

            handle.Play(clipName, playCallback);
        }

        /// <summary>
        /// 在默认的音效播放器队列中加入音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="playCallback"></param>
        public void PlayEffectSound(AudioClip clip, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                handle = AddSoundPlayer(EFFECT_SOUND, false);
            }

            handle.Play(clip, playCallback);
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clipName"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public void PlaySound(string playName, string clipName, bool isLoop, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(playName);
            if (handle is null)
            {
                handle = AddSoundPlayer(playName, isLoop);
            }

            handle.Play(clipName, playCallback);
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public void PlaySound(string playName, AudioClip clip, bool isLoop, Action<PlayState> playCallback = null)
        {
            SoundPlayer handle = GetSoundPlayer(playName);
            if (handle is null)
            {
                handle = AddSoundPlayer(playName, isLoop);
            }

            handle.Play(clip, playCallback);
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

        private void HasRecordTimeout()
        {
            if (_isRecording is false)
            {
                return;
            }

            if (_limit_time <= 0 || Microphone.IsRecording(_divName) is false)
            {
                return;
            }

            if (Time.realtimeSinceStartup - _recordStartTime < _limit_time)
            {
                return;
            }

            StopRecordingSound(false);
        }

        private void OnSplitAudioClip()
        {
            if (_isRecording is false || _limit_time > 0)
            {
                return;
            }

            if (m_CDCounter > 0)
            {
                m_CDCounter -= Time.deltaTime;
                return;
            }

            m_CDCounter = 0.2f;
            Collect();
        }

        protected int GetAudioData()
        {
            int nSize = 0;
#if !UNITY_WEBGL
            int nPosition = Microphone.GetPosition(_divName);
            if (nPosition < _position)
                nPosition = m_BufferSize;
            if (nPosition <= _position)
                return -1;
            nSize = nPosition - _position;
            if (!_recordClip.GetData(m_FloatBuffer, _position))
                return -1;
            _position = nPosition % m_BufferSize;
#endif
            return nSize;
        }

        protected virtual void Collect()
        {
            int nSize = GetAudioData(); // YAN: Out to m_FloatBuffer.
            if (nSize < 0)
            {
                return;
            }

            int lenghtSamples = _recordClip.channels * position;
            byte[] bytes = default;
            AudioClip clip = default;

            if (isHeader)
            {
                isHeader = false;
                float[] temp = new float[_recordClip.channels * position];
                Array.Copy(m_FloatBuffer, 0, temp, 0, lenghtSamples);
                clip = AudioClip.Create("mySound", lenghtSamples, _recordClip.channels, _rate, false, false);
                clip.SetData(temp, 0);
                bytes = WavUtility.FromAudioClip(_recordClip);
            }
            else
            {
                // bytes = new byte[nSize * _recordClip.channels * k_SizeofInt16];
                bytes = WavUtility.ConvertAudioClipDataToInt16ByteArray(m_FloatBuffer, nSize * _recordClip.channels);
            }

            if (BUFFER_SIZE_LIMIT <= 0)
            {
                int count = nSize * _recordClip.channels * k_SizeofInt16;
                byte[] dst = new byte[count];
                Extension.Copy(bytes, 0, dst, 0, count);
                records.Recording(dst);
            }
            else
            {
                using (MemoryStream bf = new MemoryStream(bytes))
                {
                    var buffer = new byte[BUFFER_SIZE_LIMIT];
                    while (true)
                    {
                        int len = bf.Read(buffer, 0, buffer.Length);
                        if (len <= 0)
                        {
                            break;
                        }

                        records.Recording(buffer);
                    }
                }
            }
        }


        // /// <summary>
        // /// 开始录音
        // /// </summary>
        // public void StartRecordingSound(int timeout, Action<AudioClip> callback)
        // {
        //     this.isHeader = true;
        //     this._isRecording = true;
        //     this._limit_time = timeout;
        //     this._recordStartTime = Time.realtimeSinceStartup;
        //     this.records = IRecordAudioHandle.OnCreate(callback);
        //     this._recordClip = Microphone.Start(_divName, timeout <= 0, Math.Max(1, _limit_time), _rate);
        //     this._position = Microphone.GetPosition(_divName);
        //     Debug.Log("Start Recording:" + _divName + " :" + Microphone.IsRecording(_divName));
        // }

        /// <summary>
        /// 开始录音
        /// </summary>
        public void StartRecordingSound(int timeout, Action<byte[]> callback, bool isHeader = true, int bufferSize = 6400)
        {
            this.isHeader = isHeader;
            this._isRecording = true;
            this._limit_time = timeout;
            this.BUFFER_SIZE_LIMIT = bufferSize;
            this._recordStartTime = Time.realtimeSinceStartup;
            this.records = IRecordAudioHandle.OnCreate(callback);
            this._recordClip = Microphone.Start(_divName, timeout <= 0, Math.Max(1, _limit_time), _rate);
            this._position = Microphone.GetPosition(_divName);
            Debug.Log("Start Recording:" + _divName + " :" + Microphone.IsRecording(_divName) + " time:" + timeout);
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopRecordingSound(bool isCancel)
        {
            _isRecording = false;
            if (isCancel is false)
            {
                _position = Microphone.GetPosition(_divName);
                records.Recording(WavUtility.FromAudioClip(WavUtility.SplitAudioClip(_recordClip, position, _rate)));
            }

            Microphone.End(_divName);
            _recordClip = null;
            Debug.Log("End Recording:" + _divName + " :" + Microphone.IsRecording(_divName) + " isCancel:" + isCancel);
        }
    }
}