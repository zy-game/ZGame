using Cysharp.Threading.Tasks;
using ZGame;
using ZGame.Resource;
using ZGame.Window;

namespace OdinGame.Scripts
{
    public class EntityManager : Singleton<EntityManager>, IResourceLoadingHandle
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(string path)
        {
            throw new System.NotImplementedException();
        }

        public ResHandle LoadAsset(string path)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null)
        {
            throw new System.NotImplementedException();
        }

        public void Release(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}