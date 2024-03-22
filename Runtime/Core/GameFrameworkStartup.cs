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
    private void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
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

    private void LateUpdate()
    {
        GameFrameworkEntry.LateUpdate();
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}