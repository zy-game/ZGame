using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZGame.Game;
using ZGame.Language;
using ZGame.UI;

namespace ZGame.VFS.Command
{
    public class LoadingHotfixAssemblyCommand : ICommandHandlerAsync<Status>
    {
        public async UniTask<Status> OnExecute(params object[] args)
        {
            string dllName = args[0].ToString();
            CodeMode mode = (CodeMode)args[1];
            PackageManifestManager manifestManager = (PackageManifestManager)args[2];
            Assembly assembly = default;
            if (mode is CodeMode.Native || (ResConfig.instance.resMode == ResourceMode.Editor && Application.isEditor))
            {
                if (dllName.IsNullOrEmpty())
                {
                    throw new NullReferenceException(nameof(dllName));
                }

                CoreAPI.Logger.Log("原生代码：" + dllName);
                assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            }
            else
            {
                string aotFileName = $"{dllName.ToLower()}_aot.bytes";
                string hotfixFile = $"{dllName.ToLower()}_hotfix.bytes";
                if (manifestManager.TryGetPackageVersion(aotFileName, out uint crc) is false || manifestManager.TryGetPackageVersion(hotfixFile, out crc) is false)
                {
                    throw new FileNotFoundException();
                }

                byte[] bytes = await CoreAPI.VFS.ReadAsync(aotFileName);
                Dictionary<string, byte[]> aotZipDict = await Zip.Decompress(bytes);
                foreach (var VARIABLE in aotZipDict)
                {
                    if (RuntimeApi.LoadMetadataForAOTAssembly(VARIABLE.Value, HomologousImageMode.SuperSet) != LoadImageErrorCode.OK)
                    {
                        CoreAPI.Logger.LogError("加载AOT补元数据资源失败:" + VARIABLE.Key);
                        continue;
                    }
                }

                bytes = await CoreAPI.VFS.ReadAsync(hotfixFile);
                Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(bytes);
                if (dllZipDict.TryGetValue(dllName + ".dll", out byte[] dllBytes) is false)
                {
                    throw new NullReferenceException(dllName);
                }

                CoreAPI.Logger.Log("加载热更代码:" + dllName + ".dll");
                assembly = Assembly.Load(dllBytes);
            }

            if (assembly is null)
            {
                UIMsgBox.Show(CoreAPI.Language.Query(CommonLanguage.NotFindEntry), GameFrameworkStartup.Quit);
                return Status.Fail;
            }

            SubGameStartup subGameStartup = assembly.CreateInstance<SubGameStartup>();
            if (subGameStartup is null)
            {
                UIMsgBox.Show(CoreAPI.Language.Query(CommonLanguage.NotFindEntry), GameFrameworkStartup.Quit);
                return Status.Fail;
            }

            if (await subGameStartup.OnEntry() is not Status.Success)
            {
                UIMsgBox.Show(CoreAPI.Language.Query(CommonLanguage.LoadGameFail), GameFrameworkStartup.Quit);
                return Status.Fail;
            }

            return Status.Success;
        }

        public void Release()
        {
        }
    }
}