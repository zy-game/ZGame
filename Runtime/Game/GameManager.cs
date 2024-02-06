using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using HybridCLR;
using UnityEngine;
using ZGame.Config;
using ZGame.Resource;
using ZGame.UI;

namespace ZGame.Game
{
    public sealed class GameManager : Singleton<GameManager>
    {
        private Assembly assembly = default;
        private SubGame _currentGame = default;
        private List<World> worlds = new();
        private static World _defaultWorld = default;
        public SubGame CurrentGame => _currentGame;

        public static World DefaultWorld
        {
            get
            {
                if (_defaultWorld is null)
                {
                    _defaultWorld = new World("DEFAULT_WORLD");
                }

                return _defaultWorld;
            }
        }

        protected override void OnAwake()
        {
            BehaviourScriptable.instance.SetupUpdate(OnUpdate);
            BehaviourScriptable.instance.SetupFixedUpdate(OnFixedUpdate);
        }

        private void OnFixedUpdate()
        {
            for (int i = worlds.Count - 1; i >= 0; i--)
            {
                worlds[i].OnFixedUpdate();
            }
        }

        private void OnUpdate()
        {
            for (int i = worlds.Count - 1; i >= 0; i--)
            {
                worlds[i].OnUpdate();
            }
        }

        public override void Dispose()
        {
            foreach (World world in worlds)
            {
                world.Dispose();
            }

            worlds.Clear();
            _currentGame?.Dispose();
            _currentGame = null;
        }

        public World CreateWorld(string name)
        {
            World world = GetWorld(name);
            if (world is not null)
            {
                return world;
            }

            world = new(name);
            worlds.Add(world);
            return world;
        }

        public World GetWorld(string name)
        {
            return worlds.Find(x => x.name == name);
        }

        public void RemoveWorld(string name)
        {
            if (name == "DEFAULT_WORLD")
            {
                return;
            }

            World world = GetWorld(name);
            if (world is null)
            {
                return;
            }

            world.Dispose();
            worlds.Remove(world);
        }


        public async UniTask<bool> EntryGame(EntryConfig config)
        {
            SubGame gameEntry = await SubGame.LoadGame(config);
            if (gameEntry is null)
            {
                return false;
            }

            _currentGame = gameEntry;
            _currentGame.OnEntry();
            return true;
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}