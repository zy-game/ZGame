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
            ResHandle handle = await ResourceManager.instance.LoadAssetAsync("Assets/OdinGame/ArtRes/Scene/HomeScene.unity", UIManager.instance.GetWindow<Loading>());

            UIManager.instance.Close<Loading>();
        }

        public override void Dispose()
        {
            Debug.Log("Quit Game");
        }
    }
}