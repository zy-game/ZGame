using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Resource;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    [SerializeField] public List<GameSetting> GameSettings = new List<GameSetting>();

    [Header("是否在编辑器使用热更模式")] public bool useHotfix;

    private async void Start()
    {
        CoreApi.Initialized(this);
    }
}


[Serializable]
public sealed class GameSetting
{
    [Header("Use")] public bool active;

    /// <summary>
    /// 游戏模块名
    /// </summary>
    [Header("Module Name")] public string name;

    /// <summary>
    /// 默认语言
    /// </summary>
    [Header("Default Language")] public Language Language;

    /// <summary>
    /// 资源服务器地址
    /// </summary>
    [Header("Resource Address")] public string resUrl;

    /// <summary>
    /// 服务器地址
    /// </summary>
    [Header("Server Address")] public string serverUrl;

    /// <summary>
    /// 默认资源模块
    /// </summary>
    [Header("Default Resource Module")] public string module;

    /// <summary>
    /// DLL 名称
    /// </summary>
    [Header("Dll Name")] public string dll;

    /// <summary>
    /// 补元数据列表
    /// </summary>
    [Header("AOT List")] public List<string> aot;
}