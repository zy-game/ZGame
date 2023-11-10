using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame;
using ZGame.Game;
using ZGame.Resource;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    [SerializeField] public List<GameSeting> GameSettings = new List<GameSeting>();


    private async void Start()
    {
        GameSeting setting = GameSettings.Find(x => x.active);
        Engine.Initialized(setting);
        Engine.Cameras.NewCamera("UICamera", 999, "UI");
        Loading loading = Engine.Window.GeOrOpentWindow<Loading>();
        if (setting is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }


        await UpdateAndLoadingResourcePackageList(setting);
        await EntryGame(setting);
    }

    private static async UniTask UpdateAndLoadingResourcePackageList(GameSeting setting)
    {
        if (setting.module.IsNullOrEmpty())
        {
            throw new ResourceModuleNotFoundException();
        }

        Loading loading = Engine.Window.GeOrOpentWindow<Loading>();
        await Engine.Resource.UpdateResourcePackageList(setting.module, loading.Setup);
        await Engine.Resource.LoadingResourcePackageList(setting.module, loading.Setup);
    }

    private static async UniTask EntryGame(GameSeting settings)
    {
        Assembly assembly = default;
#if UNITY_EDITOR
        if (Engine.IsHotfix is false)
        {
            if (settings.dll.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(settings.dll));
            }

            string dllName = Path.GetFileNameWithoutExtension(settings.dll);
            assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            CallGameEntryMethod();
            return;
        }
#endif
        TextAsset textAsset = Engine.Resource.LoadAsset(Path.GetFileNameWithoutExtension(settings.dll) + ".bytes")?.Require<TextAsset>();
        if (textAsset == null)
        {
            throw new NullReferenceException(settings.dll);
        }

        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var item in settings.aot)
        {
            textAsset = Engine.Resource.LoadAsset(Path.GetFileNameWithoutExtension(item) + ".bytes")?.Require<TextAsset>();
            if (textAsset == null)
            {
                throw new Exception("加载AOT补元数据资源失败:" + item);
            }

            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, mode);
        }

        CallGameEntryMethod();

        void CallGameEntryMethod()
        {
            if (assembly is null)
            {
                throw new NullReferenceException(nameof(assembly));
            }

            MethodInfo methodInfo = assembly.GetType("Program")?.GetMethod("Main");
            if (methodInfo is null)
            {
                throw new EntryPointNotFoundException("Program.Main");
            }

            methodInfo.Invoke(null, new object[0]);
        }
    }
}