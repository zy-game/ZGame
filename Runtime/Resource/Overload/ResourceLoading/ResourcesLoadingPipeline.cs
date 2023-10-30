using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class ResourcesLoadingPipeline : IAssetLoadingPipeline
    {
        private List<AssetObjectHandle> resources = new List<AssetObjectHandle>();

        public AssetObjectHandle LoadAsset(string path)
        {
            AssetObjectHandle result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
                result = new AssetObjectHandle(Resources.Load(path.Substring(10)), path);
                resources.Add(result);
            }

            return result;
        }

        public async UniTask<AssetObjectHandle> LoadAssetAsync(string path)
        {
            AssetObjectHandle result = resources.Find(x => x.path.Equals(path));
            if (result is null)
            {
                result = new AssetObjectHandle(await Resources.LoadAsync(path.Substring(10)), path);
                resources.Add(result);
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