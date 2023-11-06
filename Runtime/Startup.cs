using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using dnlib.DotNet;
using HybridCLR;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using ZGame;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Resource;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    [SerializeField] public List<GameSetting> GameSettings = new List<GameSetting>();


    private async void Start()
    {
        GameSetting setting = GameSettings.Find(x => x.active);
        Engine.Initialized(setting);
        Loading loading = Engine.Window.GeOrOpentWindow<Loading>();
        if (setting is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }


        await UpdateAndLoadingResourcePackageList(setting);
        await EntryGame(setting);
    }

    private static async UniTask UpdateAndLoadingResourcePackageList(GameSetting setting)
    {
        if (setting.module.IsNullOrEmpty())
        {
            throw new ResourceModuleNotFoundException();
        }

        Loading loading = Engine.Window.GeOrOpentWindow<Loading>();
        await Engine.Resource.UpdateResourcePackageList(setting.module, loading.Setup);

        await Engine.Resource.LoadingResourcePackageList(setting.module, loading.Setup);

        await EntryGame(setting);
    }

    private static async UniTask EntryGame(GameSetting settings)
    {
#if UNITY_EDITOR
        if (Engine.IsHotfix is false)
        {
            if (settings.dll.IsNullOrEmpty())
            {
                Debug.LogError(new EntryPointNotFoundException());
                return;
            }

            string dllName = Path.GetFileNameWithoutExtension(settings.dll);
            foreach (var VARIABLE in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (VARIABLE.GetName().Name.Equals(dllName) is false)
                {
                    continue;
                }

                MethodInfo methodInfo = VARIABLE.GetType("Program")?.GetMethod("Main");
                if (methodInfo is null)
                {
                    Debug.LogError("未找到入口函数:Program.Main");
                    continue;
                }

                methodInfo.Invoke(null, new object[0]);
            }

            return;
        }
#endif
        TextAsset textAsset = Engine.Resource.LoadAsset(Path.GetFileNameWithoutExtension(settings.dll) + ".bytes")?.Require<TextAsset>();
        if (textAsset == null)
        {
            //Quit();
            return;
        }

        Assembly assembly = Assembly.Load(textAsset.bytes);

        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var item in settings.aot)
        {
            textAsset = Engine.Resource.LoadAsset(Path.GetFileNameWithoutExtension(item) + ".bytes")?.Require<TextAsset>();
            if (textAsset == null)
            {
                Debug.LogError("加载AOT补元数据资源失败");
                return;
            }

            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
        }

        MethodInfo method = assembly.GetType("Program")?.GetMethod("Main");
        if (method is null)
        {
            //Quit();
            return;
        }

        method.Invoke(null, new object[0]);
    }
}


[Serializable]
public sealed class GameSetting
{
    [Header("Use")] public bool active;
    [Header("是否在编辑器使用热更模式")] public bool useHotfix;

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