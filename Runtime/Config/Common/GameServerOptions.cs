using UnityEngine;

namespace ZGame.Config
{
    [HideInInspector]
    public class GameServerOptions : ScriptableObject
    {
        public string hosting;
        public ushort port;
    }
}