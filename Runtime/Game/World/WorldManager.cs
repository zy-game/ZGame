using Cinemachine;
using UnityEngine;

namespace ZGame.Game
{
    public class WorldManager : GameFrameworkModule
    {
        public CinemachineBrain brain { get; private set; }
        public GameWorld DefaultGameWorld { get; private set; }

        public override void OnAwake(params object[] args)
        {
            DefaultGameWorld = new GameWorld("SYSTEM_DEFAULT_WORLD");
            brain = new GameObject("CINEMACHINE BRAIN").AddComponent<CinemachineBrain>();
            GameObject.DontDestroyOnLoad(brain.gameObject);
        }
    }
}