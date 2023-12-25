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
        private SubGameEntry gameHandle = default;
        private HashSet<string> aotList = new();

        protected override void OnDestroy()
        {
            gameHandle?.Dispose();
            gameHandle = null;
        }

        public async UniTask EntryGame(GameConfig config, params object[] args)
        {
            await LoadAOT(config);
            await LoadDLL(config);
            OnEntryGame(config, args);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private async UniTask LoadDLL(GameConfig config)
        {
#if UNITY_EDITOR
            if (config.dll.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(config.dll));
            }

            string dllName = Path.GetFileNameWithoutExtension(config.dll);
            assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            return;
#endif
            ResHandle textAsset = await ResourceManager.instance.LoadAssetAsync(Path.GetFileNameWithoutExtension(config.dll) + ".bytes");
            if (textAsset == null)
            {
                throw new NullReferenceException(config.dll);
            }

            assembly = Assembly.Load(textAsset.Get<TextAsset>(default).bytes);
        }

        private async UniTask LoadAOT(GameConfig config)
        {
#if UNITY_EDITOR
            return;
#endif
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var item in config.aot)
            {
                if (aotList.Contains(item))
                {
                    continue;
                }

                ResHandle textAsset = await ResourceManager.instance.LoadAssetAsync(Path.GetFileNameWithoutExtension(item) + ".bytes");
                if (textAsset == null)
                {
                    throw new Exception("加载AOT补元数据资源失败:" + item);
                }

                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.Get<TextAsset>(default).bytes, mode);
                if (err != LoadImageErrorCode.OK)
                {
                    Debug.LogError("加载AOT补元数据资源失败:" + item);
                    continue;
                }

                aotList.Add(item);
                Debug.Log("加载补充元数据成功：" + item);
            }
        }

        private void OnEntryGame(GameConfig config, params object[] args)
        {
            if (assembly is null)
            {
                throw new NullReferenceException(nameof(assembly));
            }

            Type entryType = assembly.GetAllSubClasses<SubGameEntry>().FirstOrDefault();
            if (entryType is null)
            {
                throw new EntryPointNotFoundException();
            }

            if (gameHandle is not null)
            {
                gameHandle.Dispose();
            }

            gameHandle = Activator.CreateInstance(entryType) as SubGameEntry;
            gameHandle.OnEntry(args);
        }
    }
}