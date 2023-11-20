using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = System.Object;

namespace ZGame.Resource
{
    public class AssetDatabaseLoadingPipeline : IAssetLoadingPipeline
    {
        private List<AssetObjectHandle> resources = new List<AssetObjectHandle>();

        public AssetObjectHandle LoadAsset(string path)
        {
            AssetObjectHandle result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
#if UNITY_EDITOR
                result = new AssetObjectHandle(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Object)), path);
                resources.Add(result);
#endif
            }

            return result;
        }

        public async UniTask<AssetObjectHandle> LoadAssetAsync(string path)
        {
            AssetObjectHandle result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
#if UNITY_EDITOR
                result = new AssetObjectHandle(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), path);
                await UniTask.Delay(1);
                resources.Add(result);
#endif
            }

            return result;
        }

        public void Release(AssetObjectHandle resObject)
        {
            AssetObjectHandle _resObject = resources.Find(x => x.Equals(resObject));
            if (_resObject is null)
            {
                return;
            }

            _resObject.Release();
            if (_resObject.refCount > 0)
            {
                return;
            }

            _resObject.Dispose();
            resources.Remove(_resObject);
        }

        public void Dispose()
        {
            foreach (var VARIABLE in resources)
            {
                VARIABLE.Dispose();
            }

            resources.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}