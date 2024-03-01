using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using ZGame.Config;
using ZGame.UI;

namespace ZGame.Game
{
    public sealed class GameManager : Singleton<GameManager>
    {
        private SubGameEntry _currentGame;
        private List<World> worlds = new();
        private static World _defaultWorld = default;

        public static World DefaultWorld
        {
            get
            {
                if (_defaultWorld is null)
                {
                    instance.worlds.Add(_defaultWorld = new World("DEFAULT"));
                }

                return _defaultWorld;
            }
        }


        public override void Dispose()
        {
            foreach (World world in worlds)
            {
                world.Dispose();
            }

            worlds.Clear();
            Debug.Log("Dispose SubGame:" + _currentGame);
            _currentGame?.Dispose();
            _currentGame = null;
        }

        public async UniTask EntrySubGame(EntryConfig config)
        {
            Assembly assembly = await SubGameEntry.LoadGameAssembly(config);
            if (assembly is null)
            {
                throw new NullReferenceException(nameof(assembly));
            }

            Type entryType = assembly.GetAllSubClasses<SubGameEntry>().FirstOrDefault();
            if (entryType is null)
            {
                throw new EntryPointNotFoundException();
            }

            _currentGame = Activator.CreateInstance(entryType) as SubGameEntry;
            if (_currentGame is null)
            {
                Debug.LogError("加载入口失败");
                return;
            }

            Debug.Log("Entry SubGame:" + _currentGame);
            _currentGame.OnEntry();
            return;
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