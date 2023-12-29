using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using HybridCLR;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    public sealed class GameManager : Singleton<GameManager>
    {
        private Assembly assembly = default;
        private SubGame gameHandle = default;
        private List<World> worlds = new();
        public static World DefaultWorld { get; private set; }

        internal void Initialized()
        {
            DefaultWorld = CreateWorld("DEFAULT_WORLD");
        }

        protected override void OnFixedUpdate()
        {
            for (int i = worlds.Count - 1; i >= 0; i--)
            {
                worlds[i].OnFixedUpdate();
            }
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


        protected override void OnDestroy()
        {
            foreach (World world in worlds)
            {
                world.Dispose();
            }

            worlds.Clear();
            gameHandle?.Dispose();
            gameHandle = null;
        }

        public async UniTask<bool> EntryGame(EntryConfig config, params object[] args)
        {
            SubGame gameEntry = await SubGame.LoadGame(config);
            if (gameEntry is null)
            {
                return false;
            }

            gameHandle = gameEntry;
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