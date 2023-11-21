using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    public sealed class AssetBundleLoadingHandle : IDisposable
    {
        private float count;
        private float index;
        private HashSet<string> loaded;

        public void Dispose()
        {
            count = 0;
            index = 0;
            GC.SuppressFinalize(this);
        }


        public async UniTask LoadingPackageList(Action<float> progressCallback, params string[] args)
        {
            index = 0;
            count = (float)(args.Length);
            loaded = new HashSet<string>();
            foreach (var VARIABLE in args)
            {
                await LoadAssetBundleFromFile(VARIABLE, 0, progressCallback);
                index++;
                progressCallback?.Invoke(index / count);
            }
        }

        public void UnloadPackageList(params string[] args)
        {
        }

        private async UniTask LoadAssetBundleFromFile(string fileName, uint version, Action<float> progressCallback)
        {
            if (loaded.Contains(fileName))
            {
                return;
            }

            if (FileManager.instance.Exist(fileName, version) is false)
            {
                Clear();
                throw new FileNotFoundException(fileName);
            }

            IFileDataReader fileDataReader = await FileManager.instance.ReadAsync(fileName);
            if (fileDataReader.bytes is null || fileDataReader.bytes.Length == 0)
            {
                Clear();
                throw new FileLoadException(fileDataReader.name);
            }

            AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(fileDataReader.bytes);
            ABManager.instance.Add(assetBundle);
            fileDataReader.Dispose();
        }

        private void Clear()
        {
            count = 0;
            index = 0;
            foreach (var VARIABLE in loaded)
            {
                ABManager.instance.Remove(VARIABLE);
            }

            loaded.Clear();
        }
    }
}