using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Sound
{
    [Config(Localtion.Packaged, "Assets/Test/SoundOptions.asset")]
    public sealed class SoundPlayOptions : SingleScript<SoundPlayOptions>
    {
        [Header("音效播放设置")] public List<SoundOptions> optionsList;
    }

    [Serializable]
    public class SoundOptions
    {
        [Header("是否启用")] public Switch state;
        [Header("别称")] public string title;

        [Range(0, 1), Header("音量")] public float volumen;
        [Header("是否静音")] public Switch mute;
        [Header("优先级"), Range(0, 100)] public int priority;
        [Header("是否循环播放")] public Switch loop;
    }
}