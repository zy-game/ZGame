using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    public class SubGameEntry : IDisposable
    {
        public virtual void OnEntry()
        {
        }

        public virtual void Dispose()
        {
        }

        internal static async UniTask<Assembly> LoadGameAssembly(GameConfig config)
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

            string aotZipPath = WorkApi.Resource.PackageManifest.GetAssetFullPath(config.module, $"{config.entryName.ToLower()}_aot.bytes");
            string hotfixZipPath = WorkApi.Resource.PackageManifest.GetAssetFullPath(config.module, $"{config.entryName.ToLower()}_hotfix.bytes");
            Debug.Log("补元数据：" + aotZipPath);
            using (ResObject resObject = await WorkApi.Resource.LoadAssetAsync(aotZipPath))
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
            using (ResObject resObject = await WorkApi.Resource.LoadAssetAsync(hotfixZipPath))
            {
                if (resObject == null || resObject.IsSuccess() is false)
                {
                    return null;
                }

                Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(resObject.GetAsset<TextAsset>().bytes);
                byte[] dllBytes = default;
                foreach (var VARIABLE in config.references)
                {
                    if (dllZipDict.TryGetValue(VARIABLE + ".dll", out dllBytes) is false)
                    {
                        throw new NullReferenceException(config.entryName);
                    }

                    Assembly.Load(dllBytes);
                    Debug.Log("Load Reference DLL:" + VARIABLE);
                }

                if (dllZipDict.TryGetValue(config.entryName + ".dll", out dllBytes) is false)
                {
                    throw new NullReferenceException(config.entryName);
                }

                Debug.Log("Load Game Dll:" + config.entryName + ".dll Lenght:" + dllBytes.Length);
                return Assembly.Load(dllBytes);
            }
        }
    }
}