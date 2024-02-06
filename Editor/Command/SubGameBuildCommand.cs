using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.LinkerEditor;
using ZGame.Editor.ResBuild.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.Command
{
    public class SubGameBuildCommand : ICommandExecuter
    {
        public void Dispose()
        {
        }

        public void Awake()
        {
        }

        public void Executer(params object[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            var config = args[0] as EntryConfig;
            var genericAll = args[1] as bool? ?? false;
            //todo 编译资源
            PackageSeting seting = BuilderConfig.instance.packages.Find(x => x.name == config.module);
            CompileDllCommand.CompileDllActiveBuildTarget();
            LinkerConfig.instance.Generic();
            if (config.mode is CodeMode.Hotfix)
            {
                //todo 编译DLL资源
                if (genericAll)
                {
                    PrebuildCommand.GenerateAll();
                }

                string hotfixPacageDir = GetHotfixPackagePath(seting);
                string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
                IReadOnlyList<string> aotList = AppDomain.CurrentDomain.GetStaticFieldValue<IReadOnlyList<string>>("AOTGenericReferences", "PatchedAOTAssemblyList");
                if (aotList is null || aotList.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "AOT 资源编译失败！", "OK");
                }

                byte[] bytes = Zip.Compress("*.dll", aotList.Select(x => $"{aotDir}/{x}").ToArray());
                File.WriteAllBytes($"{hotfixPacageDir}/{config.entryName.ToLower()}_aot.bytes", bytes);

                string hotfixDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget);
                bytes = Zip.Compress("*.dll", hotfixDir);
                File.WriteAllBytes($"{hotfixPacageDir}/{config.entryName.ToLower()}_hotfix.bytes", bytes);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            ZGame.CommandManager.OnExecuteCommand<BuildPackageCommand>(seting);
            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }

        private static string GetHotfixPackagePath(PackageSeting seting)
        {
            string hotfixPacageDir = string.Empty;
            RulerData rulerData = seting.items.Find(x => x.folder.name == "Hotfix");
            if (rulerData is not null)
            {
                return AssetDatabase.GetAssetPath(rulerData.folder);
            }

            rulerData = seting.items.Where(x => x.folder != null).FirstOrDefault();
            if (rulerData is null || rulerData.folder == null)
            {
                hotfixPacageDir = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "Hotfix");
            }
            else
            {
                string pacagePath = AssetDatabase.GetAssetPath(rulerData.folder);
                string parent = pacagePath.Substring(0, pacagePath.LastIndexOf("/") + 1);
                hotfixPacageDir = Path.Combine(parent, "Hotfix");
            }

            if (Directory.Exists(hotfixPacageDir) is false)
            {
                Directory.CreateDirectory(hotfixPacageDir);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            seting.items.Add(new RulerData()
            {
                folder = AssetDatabase.LoadAssetAtPath<Object>(hotfixPacageDir),
                buildType = BuildType.Once,
                selector = new Selector(".bytes")
            });
            BuilderConfig.OnSave();
            return hotfixPacageDir;
        }
    }
}