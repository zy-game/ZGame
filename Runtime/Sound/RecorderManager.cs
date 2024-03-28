using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Microphone =
#if !UNITY_EDITOR && UNITY_WEBGL
    UnityEngine.MicrophoneWebGl;
#else
    UnityEngine.Microphone;
#endif


namespace ZGame.Sound
{
    public class RecorderManager : GameFrameworkModule
    {
        private int _rate;
        private float _volume;
        private int _position;
        private int _limit_time;
        private string _divName;
        private float _recordStartTime;
        private bool _isRecording = false;
        private Action<byte[]> records;
        private AudioClip _recordClip = null;
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

        public override void OnAwake(params object[] args)
        {
            _rate = 16000;
            _limit_time = 60;
            m_BufferSize = m_BufferSeconds * _rate;
            m_FloatBuffer = new float[m_BufferSize * 1];

            _divName = Microphone.devices.FirstOrDefault();

            m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt16];
        }

        public override void Update()
        {
            HasRecordTimeout();
            OnSplitAudioClip();
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
            int nPosition = Microphone.GetPosition(_divName);
            if (nPosition < _position)
                nPosition = m_BufferSize;
            if (nPosition <= _position)
                return -1;
            nSize = nPosition - _position;
            if (!_recordClip.GetData(m_FloatBuffer, _position))
                return -1;
            _position = nPosition % m_BufferSize;
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
                records?.Invoke(dst);
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

                        records?.Invoke(buffer);
                    }
                }
            }
        }

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
            this.records = callback;
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
                records.Invoke(WavUtility.FromAudioClip(WavUtility.SplitAudioClip(_recordClip, position, _rate)));
            }

            Microphone.End(_divName);
            _recordClip = null;
            Debug.Log("End Recording:" + _divName + " :" + Microphone.IsRecording(_divName) + " isCancel:" + isCancel);
        }

        public override void Release()
        {
            StopRecordingSound(true);
        }
    }
}