using UnityEngine;

namespace ZEngine
{
    /// <summary>
    /// 播放器
    /// </summary>
    public interface IPlayerable<T> where T : Object
    {
        T asset { get; }
        bool isPlaying { get; }
    }
}