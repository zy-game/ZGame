using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame.FileSystem;
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
            if (config.mode is CodeMode.Native || (BasicConfig.instance.resMode == ResourceMode.Editor && Application.isEditor))
            {
                Debug.Log("原生代码：" + config.entryName);
                if (config.entryName.IsNullOrEmpty())
                {
                    throw new NullReferenceException(nameof(config.entryName));
                }

                string dllName = Path.GetFileNameWithoutExtension(config.entryName);
                return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            }

            string aotZipPath = PackageManifestManager.instance.GetAssetFullPath(config.module, $"{config.entryName.ToLower()}_aot.bytes");
            string hotfixZipPath = PackageManifestManager.instance.GetAssetFullPath(config.module, $"{config.entryName.ToLower()}_hotfix.bytes");
            Debug.Log("补元数据：" + aotZipPath);
            using (ResObject resObject = await ResourceManager.instance.LoadAssetAsync(aotZipPath))
            {
                if (resObject == null || resObject.IsSuccess() is false)
                {
                    return null;
                }

                Dictionary<string, byte[]> aotZipDict = await Zip.Decompress(resObject.GetAsset<TextAsset>().bytes);
                foreach (var VARIABLE in aotZipDict)
                {
                    if (RuntimeApi.LoadMetadataForAOTAssembly(VARIABLE.Value, HomologousImageMode.SuperSet) != LoadImageErrorCode.OK)
                    {
                        Debug.LogError("加载AOT补元数据资源失败:" + VARIABLE.Key);
                        continue;
                    }

                    Debug.Log("加载补充元数据成功：" + VARIABLE.Key);
                }
            }

            Debug.Log("热更代码：" + hotfixZipPath);
            using (ResObject resObject = await ResourceManager.instance.LoadAssetAsync(hotfixZipPath))
            {
                if (resObject == null || resObject.IsSuccess() is false)
                {
                    return null;
                }

                Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(resObject.GetAsset<TextAsset>().bytes);
                if (dllZipDict.TryGetValue(config.entryName + ".dll", out byte[] dllBytes) is false)
                {
                    throw new NullReferenceException(config.entryName);
                }

                Debug.Log("Load Game Dll:" + config.entryName + ".dll Lenght:" + dllBytes.Length);
                return Assembly.Load(dllBytes);
            }
        }
    }
}