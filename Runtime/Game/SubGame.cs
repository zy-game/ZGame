using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    public abstract class SubGame : IDisposable
    {
        public Assembly assembly { get; private set; }
        public EntryConfig config { get; private set; }

        public virtual void OnEntry(params object[] args)
        {
        }

        public virtual void Dispose()
        {
        }

        public static async UniTask<SubGame> LoadGame(EntryConfig config, params object[] args)
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
            if (config.mode is CodeMode.Native || Application.isEditor)
            {
                if (config.entryName.IsNullOrEmpty())
                {
                    throw new NullReferenceException(nameof(config.entryName));
                }

                string dllName = Path.GetFileNameWithoutExtension(config.entryName);
                return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            }

            ResHandle textAsset = default;
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var item in config.references)
            {
                textAsset = await ResourceManager.instance.LoadAssetAsync(Path.GetFileNameWithoutExtension(item) + ".bytes");
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

                Debug.Log("加载补充元数据成功：" + item);
            }

            textAsset = await ResourceManager.instance.LoadAssetAsync(Path.GetFileNameWithoutExtension(config.entryName) + ".bytes");
            if (textAsset == null)
            {
                throw new NullReferenceException(config.entryName);
            }

            return Assembly.Load(textAsset.Get<TextAsset>(default).bytes);
        }
    }
}