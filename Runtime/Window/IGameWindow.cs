using UnityEngine;

namespace ZGame.Window
{
    public interface IGameWindow : IEntity
    {
        string name { get; }
        GameObject gameObject { get; }
    }
}