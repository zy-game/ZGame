using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Inworld;
using UnityEngine;

namespace ZGame
{
    /// <summary>收录声音</summary>
    public class RecordVoice : Singleton<RecordVoice>
    {
        private int _rate;
        private int _position;
        private int _limit_time;
        private string _divName;
        private Action _timeout;
        private float m_startTime;
        private bool m_isTiming = false;
        private AudioClip m_resultClip = null;

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

        protected override void OnAwake()
        {
            _divName = Microphone.devices.FirstOrDefault();
            _limit_time = 60;
            _rate = 16000;
        }


        public void SetLimitTime(int time, Action timeout)
        {
            _limit_time = time;
        }

        public void SetRecordRate(int rate)
        {
            _rate = rate;
        }

        public void SetMicName(string name)
        {
            _divName = name;
        }

        protected override void OnUpdate()
        {
            if (m_isTiming)
            {
                if (Time.realtimeSinceStartup - m_startTime >= _limit_time)
                {
                    End();
                    _timeout?.Invoke();
                }
            }
        }

        public void Start()
        {
            m_isTiming = true;
            m_resultClip = Microphone.Start(_divName, false, _limit_time, _rate);
            m_startTime = Time.realtimeSinceStartup;
        }

        public void End()
        {
            _position = Microphone.GetPosition(_divName);
            Microphone.End(_divName);
            m_isTiming = false;
        }

        public void Clear()
        {
            m_resultClip = null;
        }

        public AudioClip GetVoiceAudioClip()
        {
            float[] simples = new float[m_resultClip.samples];
            byte[] chunk = new byte[position * sizeof(short)];
            m_resultClip.GetData(simples, 0);
            AudioClip clip = AudioClip.Create("mySound", m_resultClip.channels * position, m_resultClip.channels, _rate, false, false);
            float[] temp = new float[m_resultClip.channels * position];
            Array.Copy(simples, 0, temp, 0, m_resultClip.channels * position);
            clip.SetData(temp, 0);
            return clip;
        }

        public string GetVoiceBase64String()
        {
            return GetVoiceAudioClip().ToBase64String();
        }
    }
}