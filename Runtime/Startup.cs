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
using ZGame.State;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    [SerializeField] public List<GameSeting> GameSettings = new List<GameSeting>();

    private StateMachine _stateMachine;


    class InitGameCamera : StateHandle
    {
        public override void OnEntry()
        {
            LayerManager.instance.NewCamera("UICamera", 999, "UI");
            Loading loading = UIManager.instance.GeOrOpentWindow<Loading>();
            if (GameSeting.current is null)
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
            if (GameSeting.current is null)
            {
                Debug.LogError(new EntryPointNotFoundException());
                return;
            }

            loading.TextMeshProUGUI_TextTMP.Setup("正在获取配置信息...");
            if (GameSeting.current.module.IsNullOrEmpty())
            {
                throw new ArgumentNullException("module");
            }

            await ResourceManager.instance.CheckUpdateResourcePackageList(loading.SetupProgress, GameSeting.current.module);
            await ResourceManager.instance.LoadingResourcePackageList(loading.SetupProgress, GameSeting.current.module);
            owner.Switch<EntryGameStateHandle>();
        }
    }

    class EntryGameStateHandle : StateHandle
    {
        public async override void OnEntry()
        {
            Assembly assembly = default;
#if UNITY_EDITOR
            if (GameSeting.current.dll.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(GameSeting.current.dll));
            }

            string dllName = Path.GetFileNameWithoutExtension(GameSeting.current.dll);
            assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            CallGameEntryMethod();
            return;
#endif
            TextAsset textAsset = ResourceManager.instance.LoadAsset(Path.GetFileNameWithoutExtension(GameSeting.current.dll) + ".bytes")?.Require<TextAsset>();
            if (textAsset == null)
            {
                throw new NullReferenceException(GameSeting.current.dll);
            }

            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var item in GameSeting.current.aot)
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

    private async void Start()
    {
        GameSeting.current = GameSettings.Find(x => x.active);
        _stateMachine = StateMachineManager.instance.Create("STARTUP");
        _stateMachine.AddState<InitGameCamera>();
        _stateMachine.AddState<UpdateGameStateHandle>();
        _stateMachine.AddState<EntryGameStateHandle>();
        _stateMachine.Switch<InitGameCamera>();
    }
}