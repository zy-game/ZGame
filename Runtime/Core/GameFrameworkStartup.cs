using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame;
using ZGame.Config;
using ZGame.Game;
using ZGame.Resource;
using ZGame.UI;

public class GameFrameworkStartup : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        GameFrameworkEntry.Initialized();
    }

    private void OnApplicationQuit()
    {
        GameFrameworkEntry.Uninitialized();
    }

    private void FixedUpdate()
    {
        GameFrameworkEntry.FixedUpdate();
    }

    private void Update()
    {
        GameFrameworkEntry.Update();
    }
}