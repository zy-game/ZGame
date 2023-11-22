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
        GameSeting.current = GameSettings.Find(x => x.active);
        LayerManager.instance.NewCamera("UICamera", 999, "UI");
        Loading loading = UIManager.instance.GeOrOpentWindow<Loading>();
        if (GameSeting.current is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }


        await UpdateAndLoadingResourcePackageList(GameSeting.current);
        await EntryGame(GameSeting.current);
    }

    private static async UniTask UpdateAndLoadingResourcePackageList(GameSeting setting)
    {
        if (setting.module.IsNullOrEmpty())
        {
            throw new ArgumentNullException("module");
        }

        Loading loading = UIManager.instance.GeOrOpentWindow<Loading>();
        await ResourceManager.instance.CheckUpdateResourcePackageList(loading.SetupProgress, setting.module);
        await ResourceManager.instance.LoadingResourcePackageList(loading.SetupProgress, setting.module);
    }

    private static async UniTask EntryGame(GameSeting settings)
    {
        Assembly assembly = default;
#if UNITY_EDITOR
        if (settings.dll.IsNullOrEmpty())
        {
            throw new NullReferenceException(nameof(settings.dll));
        }

        string dllName = Path.GetFileNameWithoutExtension(settings.dll);
        Debug.Log(dllName);
        assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
        CallGameEntryMethod();
        return;
#endif
        TextAsset textAsset = ResourceManager.instance.LoadAsset(Path.GetFileNameWithoutExtension(settings.dll) + ".bytes")?.Require<TextAsset>();
        if (textAsset == null)
        {
            throw new NullReferenceException(settings.dll);
        }

        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var item in settings.aot)
        {
            textAsset = ResourceManager.instance.LoadAsset(Path.GetFileNameWithoutExtension(item) + ".bytes")?.Require<TextAsset>();
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

            List<Type> types = AppDomain.CurrentDomain.GetCustomAttributesWithoutType<GameEntry>();
            if (types.Count == 0)
            {
                throw new EntryPointNotFoundException();
            }


            MethodInfo methodInfo = types.FirstOrDefault()?.GetMethod("Main");
            if (methodInfo is null)
            {
                throw new EntryPointNotFoundException("Method Main");
            }

            methodInfo.Invoke(null, new object[1] { new string[0] });
        }
    }
}