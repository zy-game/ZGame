using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame.Networking;
using ZGame.Resource;

namespace ZGame.Game
{
    public abstract class SubGame : IDisposable
    {
        public Assembly assembly { get; private set; }
        public EntryConfig config { get; private set; }

        public virtual void OnEntry(params string[] args)
        {
        }

        public virtual void Dispose()
        {
        }

        public static async UniTask<SubGame> LoadGame(EntryConfig config, params string[] args)
        {
            Assembly assembly = await LoadGameAssembly(config);
            if (assembly is null)
            {
                throw new NullReferenceException(nameof(assembly));
            }

            Type entryType = assembly.GetAllSubClasses<SubGame>().FirstOrDefault();
            if (entryType is null)
            {
                throw new EntryPointNotFoundException();
            }

            SubGame subGameEntry = Activator.CreateInstance(entryType) as SubGame;
            subGameEntry.assembly = assembly;
            subGameEntry.config = config;
            subGameEntry.OnEntry(args);
            return subGameEntry;
        }

        private static async UniTask<Assembly> LoadGameAssembly(EntryConfig config)
        {
            if (config.mode is CodeMode.Native || BasicConfig.instance.resMode == ResourceMode.Editor)
            {
                if (config.entryName.IsNullOrEmpty())
                {
                    throw new NullReferenceException(nameof(config.entryName));
                }

                string dllName = Path.GetFileNameWithoutExtension(config.entryName);
                return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            }


            byte[] zipBytes =
#if UNITY_EDITOR
                await File.ReadAllBytesAsync(Path.Combine(Application.streamingAssetsPath, "aot.bytes"));
#else
                await Request.GetStreamingAsset(Path.Combine(Application.streamingAssetsPath, "aot.bytes"));
#endif
            Dictionary<string, byte[]> aotZipDict = await Zip.Decompress(zipBytes);
            foreach (var VARIABLE in aotZipDict)
            {
                if (RuntimeApi.LoadMetadataForAOTAssembly(VARIABLE.Value, HomologousImageMode.SuperSet) != LoadImageErrorCode.OK)
                {
                    Debug.LogError("加载AOT补元数据资源失败:" + VARIABLE.Key);
                    continue;
                }

                Debug.Log("加载补充元数据成功：" + VARIABLE.Key);
            }

            zipBytes =
#if UNITY_EDITOR
                await File.ReadAllBytesAsync(Path.Combine(Application.streamingAssetsPath, "hotfix.bytes"));
#else
                await Request.GetStreamingAsset(Path.Combine(Application.streamingAssetsPath, "hotfix.bytes"));
#endif
            Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(zipBytes);
            if (dllZipDict.TryGetValue(config.entryName + ".dll", out byte[] dllBytes) is false)
            {
                throw new NullReferenceException(config.entryName);
            }

            Debug.Log("Load Game Dll:" + config.entryName + ".dll Lenght:" + dllBytes.Length);
            return Assembly.Load(dllBytes);
        }
    }
}