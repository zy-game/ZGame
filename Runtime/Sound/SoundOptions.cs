using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Sound
{
    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public sealed class SoundPlayOptions : SingleScript<SoundPlayOptions>
    {
        [Header("音效播放设置")] public List<SoundOptions> optionsList;
    }

    [Serializable]
    public class SoundOptions
    {
        [Header("是否启用")] public Switch state;
        [Header("别称")] public string title;
        [Header("音量")] public float volumen;
        [Header("是否静音")] public Switch mute;
    }
}