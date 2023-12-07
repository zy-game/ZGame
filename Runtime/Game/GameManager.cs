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
        private GameHandle gameHandle = default;
        private HashSet<string> aotList = new();

        public async UniTask EntryGame(EntryConfig options, params object[] args)
        {
            await LoadAOT(options);
            await LoadDLL(options);
            OnEntryGame(options, args);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private async UniTask LoadDLL(EntryConfig options)
        {
#if UNITY_EDITOR
            if (options.dll.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(options.dll));
            }

            string dllName = Path.GetFileNameWithoutExtension(options.dll);
            assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            return;
#endif
            ResHandle textAsset = await ResourceManager.instance.LoadAssetAsync(Path.GetFileNameWithoutExtension(options.dll) + ".bytes");
            if (textAsset == null)
            {
                throw new NullReferenceException(options.dll);
            }

            assembly = Assembly.Load(textAsset.Require<TextAsset>().bytes);
        }

        private async UniTask LoadAOT(EntryConfig options)
        {
#if UNITY_EDITOR
            return;
#endif
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var item in options.aot)
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

                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.Require<TextAsset>().bytes, mode);
                if (err != LoadImageErrorCode.OK)
                {
                    Debug.LogError("加载AOT补元数据资源失败:" + item);
                    continue;
                }

                aotList.Add(item);
                Debug.Log("加载补充元数据成功：" + item);
            }
        }

        private void OnEntryGame(EntryConfig options, params object[] args)
        {
            if (assembly is null)
            {
                throw new NullReferenceException(nameof(assembly));
            }

            Type entryType = assembly.GetAllSubClasses<GameHandle>().FirstOrDefault();
            if (entryType is null)
            {
                throw new EntryPointNotFoundException();
            }

            if (gameHandle is not null)
            {
                gameHandle.Dispose();
            }

            gameHandle = Activator.CreateInstance(entryType) as GameHandle;
            gameHandle.OnEntry(args);
        }


        public void Dispose()
        {
        }
    }
}