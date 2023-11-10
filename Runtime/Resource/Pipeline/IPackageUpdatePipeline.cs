using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IPackageUpdatePipeline : IDisposable
    {
        UniTask StartUpdate(BuilderManifest manifest, Action<float> progressCallback);
        UniTask StartUpdate(Action<float> progressCallback, params string[] args);
    }
}