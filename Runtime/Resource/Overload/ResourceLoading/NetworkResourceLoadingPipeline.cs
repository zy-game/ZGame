using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public class NetworkResourceLoadingPipeline : IAssetLoadingPipeline
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public AssetObjectHandle LoadAsset(string path)
        {
            throw new NotImplementedException();
        }

        public UniTask<AssetObjectHandle> LoadAssetAsync(string path)
        {
            throw new NotImplementedException();
        }

        public void Release(AssetObjectHandle obj)
        {
            throw new NotImplementedException();
        }
    }
}