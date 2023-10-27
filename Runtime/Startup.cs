using System;
using UnityEngine;
using ZGame;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Resource;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    /// <summary>
    /// 默认资源模块
    /// </summary>
    public string module;

    public string dataSystem;
    public string gameSystem;
    public string networkSystem;
    public string objectSystem;
    public string resourceSystem;
    public string windowSystem;
    public string localizetionSystem;
    public string executer;

    private async void Start()
    {
        CoreApi.Initialized(this);
    }
}