using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class AssetObjectLoadingPipeline : IAssetLoadingPipeline
    {
        public ResHandle LoadAsset(string path)
        {
            if (path.IsNullOrEmpty())
            {
                throw new ArgumentException("path is null or empty");
            }

            if (path.StartsWith("http"))
            {
                throw new AggregateException("网络资源不能在同步中加载");
            }

            ResHandle result = default;
            ABHandle bundleHandle = default;
#if UNITY_EDITOR
            if (GameSeting.current.runtime == RuntimeMode.Editor)
            {
                bundleHandle = ABManager.instance.GetBundleHandle("EDITOR");
                if (bundleHandle is null)
                {
                    bundleHandle = ABManager.instance.Add("EDITOR");
                }


                var o = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                if (o is null)
                {
                    throw new FileNotFoundException(path);
                }

                result = new ResHandle(bundleHandle, o, path);
            }
#endif
            return default;
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            return default;
        }

        public void Release(ResHandle resObject)
        {
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}