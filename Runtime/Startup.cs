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
using ZGame.Runnable;
using ZGame.State;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    private async void Start()
    {
        RunnableManager.instance.Run<StartupRunnableHandle>();
    }
}

class StartupRunnableHandle : LinkedRunnableHandle<CheckupResourceUpdateRunnableHandle>
{
    public override bool IsCompletion()
    {
        return false;
    }
}

class CheckupResourceUpdateRunnableHandle : LinkedRunnableHandle<LoadingResourcePackageRunnableHandle>
{
    public override bool IsCompletion()
    {
        return false;
    }
}

class LoadingResourcePackageRunnableHandle : LinkedRunnableHandle<EntryGameRunnableHandle>
{
    public override bool IsCompletion()
    {
        return false;
    }
}

class EntryGameRunnableHandle : RunnableHandle
{
    public override bool IsCompletion()
    {
        return false;
    }
}

class InitGameCamera : StateHandle
{
    public override void OnEntry()
    {
        LayerManager.instance.NewCamera("UICamera", 999, "UI");
        Loading loading = UIManager.instance.GeOrOpentWindow<Loading>();
        if (GlobalConfig.current is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        loading.TextMeshProUGUI_TextTMP.Setup("正在获取配置信息...");
        owner.Switch<UpdateGameStateHandle>();
    }
}

class UpdateGameStateHandle : StateHandle
{
    public async override void OnEntry()
    {
        Loading loading = UIManager.instance.GeOrOpentWindow<Loading>();
        if (GlobalConfig.current is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        loading.TextMeshProUGUI_TextTMP.Setup("正在获取配置信息...");
        if (GlobalConfig.current.module.IsNullOrEmpty())
        {
            throw new ArgumentNullException("module");
        }

        await ResourceManager.instance.CheckUpdateResourcePackageList(loading.SetupProgress, GlobalConfig.current.module);
        await ResourceManager.instance.LoadingResourcePackageList(loading.SetupProgress, GlobalConfig.current.module);
        owner.Switch<EntryGameStateHandle>();
    }
}

class EntryGameStateHandle : StateHandle
{
    public async override void OnEntry()
    {
        Assembly assembly = default;
#if UNITY_EDITOR
        if (GlobalConfig.current.dll.IsNullOrEmpty())
        {
            throw new NullReferenceException(nameof(GlobalConfig.current.dll));
        }

        string dllName = Path.GetFileNameWithoutExtension(GlobalConfig.current.dll);
        assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
        CallGameEntryMethod();
        return;
#endif
        TextAsset textAsset = ResourceManager.instance.LoadAsset(Path.GetFileNameWithoutExtension(GlobalConfig.current.dll) + ".bytes")?.Require<TextAsset>();
        if (textAsset == null)
        {
            throw new NullReferenceException(GlobalConfig.current.dll);
        }

        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var item in GlobalConfig.current.aot)
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