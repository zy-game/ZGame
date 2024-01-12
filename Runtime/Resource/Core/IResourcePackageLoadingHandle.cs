using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ZGame.Window;

namespace ZGame.Resource
{
    public interface IResourcePackageLoadingHandle : IDisposable
    {
        void LoadingPackageListSync(params ResourcePackageManifest[] manifests);
        UniTask LoadingPackageListAsync(params ResourcePackageManifest[] manifests);
    }
}