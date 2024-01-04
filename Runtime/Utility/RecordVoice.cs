using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame
{
    /// <summary>收录声音</summary>
    public class RecordVoice : Singleton<RecordVoice>
    {
        private float m_startTime;
        private bool m_isTiming = false;
        private int m_samplesLength;
        private AudioClip m_resultClip = null;

        /// <summary>设备名麦克风</summary>
        public string DeviceNameMIC { get; set; }

        /// <summary>录音产生的AudioClip的长度</summary>
        public int LengthSec { get; set; }

        /// <summary>由录音产生的AudioClip的采样率</summary>
        public int Frequency { get; set; }

        protected override void OnAwake()
        {
            m_samplesLength = 128;
            DeviceNameMIC = Microphone.devices.FirstOrDefault(); //获取设备麦克风："Built-in Microphone"
            LengthSec = 60; //ASR最长60秒
            Frequency = 16000;
        }

        protected override void OnUpdate()
        {
            if (m_isTiming)
            {
                if (Time.realtimeSinceStartup - m_startTime >= LengthSec)
                {
                    End();
                }
            }
        }

        public void Start()
        {
            m_resultClip = Microphone.Start(DeviceNameMIC, false, LengthSec, Frequency);
            m_isTiming = true;
            m_startTime = Time.realtimeSinceStartup;
        }

        public AudioClip End()
        {
            Microphone.End(DeviceNameMIC);
            m_isTiming = false;
            return m_resultClip;
        }

        public float GetVolume()
        {
            float levelMax = 0;
            if (Microphone.IsRecording(DeviceNameMIC))
            {
                float[] samples = new float[m_samplesLength];
                int startPosition = Microphone.GetPosition(DeviceNameMIC) - (m_samplesLength + 1);
                if (startPosition >= 0)
                {
                    //当麦克风还未正式启动时，该值会为负值，AudioClip.GetData函数会报错
                    m_resultClip.GetData(samples, startPosition);
                    for (int i = 0; i < m_samplesLength; i++)
                    {
                        float wavePeak = samples[i];
                        if (levelMax < wavePeak)
                        {
                            levelMax = wavePeak;
                        }
                    }

                    levelMax = levelMax * 99;
                    Debug.Log("麦克风音量：" + levelMax);
                }
            }

            return levelMax;
        }

        public float GetTime()
        {
            return Time.realtimeSinceStartup - m_startTime;
        }

        public AudioClip GetResult()
        {
            return m_resultClip;
        }
    }
}