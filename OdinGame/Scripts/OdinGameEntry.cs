using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame;
using ZGame.Game;
using ZGame.Resource;
using ZGame.Window;

namespace OdinGame.Scripts
{
    public class OdinGameEntry : SubGameEntry
    {
        public override async void OnEntry(params object[] args)
        {
            base.OnEntry(args);
            Debug.Log("OdinGameEntry OnEntry");
            Loading loading = UIManager.instance.TryOpen<Loading>();
            ResHandle handle = await ResourceManager.instance.LoadAssetAsync("Assets/OdinGame/ArtRes/Scene/Game.unity", loading);
            await handle.OpenSceneAsync(loading);
            UIManager.instance.Close<Loading>();
            World.DefaultGameObjectInjectionWorld.CreateSystem<MoveSystem>();
        }

        public override void Dispose()
        {
            Debug.Log("Quit Game");
        }
    }
}