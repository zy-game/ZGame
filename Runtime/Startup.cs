using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame;
using ZGame.Config;
using ZGame.Game;
using ZGame.Resource;
using ZGame.UI;

public class Startup : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        WorkApi.Initialized();
    }

    private void OnApplicationQuit()
    {
        WorkApi.Uninitialized();
    }
}