using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.Language;
using ZGame.UI;

namespace ZGame.VFS.Command
{
    public class LoadingResObjectCommand : ICommandHandler<ResObject>
    {
        public ResObject OnExecute(params object[] args)
        {
            string path = args[0].ToString();
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            if (ResObject.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, Resources.Load(path.Substring(10)), path);
            }

#if UNITY_EDITOR
            else if (ResConfig.instance.resMode == ResourceMode.Editor && path.StartsWith("Assets"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), path);
            }
#endif
            else
            {
                if (CoreAPI.VFS.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    CoreAPI.Logger.LogError("资源未找到：" + path);
                    return ResObject.DEFAULT;
                }

                if (ResPackage.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    using (LoadingResPackageCommand command = RefPooled.Spawner<LoadingResPackageCommand>())
                    {
                        command.OnExecute(manifest);
                    }

                    return OnExecute(path);
                }

                resObject = ResObject.Create(package, package.bundle.LoadAsset(path), path);
            }

            return resObject;
        }

        public void Release()
        {
        }
    }

    public class LoadingResObjectAsyncCommand : ICommandHandlerAsync<ResObject>
    {
        public async UniTask<ResObject> OnExecute(params object[] args)
        {
            string path = args[0].ToString();
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            if (ResObject.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, await Resources.LoadAsync(path.Substring(10)), path);
            }
#if UNITY_EDITOR
            else if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), path);
            }
#endif
            else
            {
                if (CoreAPI.VFS.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    CoreAPI.Logger.LogError("资源未找到：" + path);
                    return ResObject.DEFAULT;
                }

                if (ResPackage.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    using (LoadingResPackageAsyncCommand command = RefPooled.Spawner<LoadingResPackageAsyncCommand>())
                    {
                        await command.OnExecute(manifest);
                    }

                    return await OnExecute(path);
                }

                resObject = ResObject.Create(package, await package.bundle.LoadAssetAsync(path), path);
            }

            return resObject;
        }

        public void Release()
        {
        }
    }

    public class LoadingSceneCommand : ICommandHandlerAsync<ResObject>
    {
        public async UniTask<ResObject> OnExecute(params object[] args)
        {
            string path = args[0].ToString();
            IProgress<float> callback = (IProgress<float>)args[1];
            LoadSceneMode mode = (LoadSceneMode)args[2];
            PackageManifestManager manifestManager = (PackageManifestManager)args[3];
            UILoading.SetTitle(CoreAPI.Language.Query(CommonLanguage.LoadScene));
            Scene scene = default;
            ResPackage package = default;
            AsyncOperation operation = default;
            ResObject sceneObject = default;
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
#endif
            if (operation == null)
            {
                if (manifestManager.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    return sceneObject;
                }

                if (ResPackage.TryGetValue(manifest.name, out package) is false)
                {
                    using (LoadingResPackageAsyncCommand command = RefPooled.Spawner<LoadingResPackageAsyncCommand>())
                    {
                        await command.OnExecute(manifest);
                    }

                    return await OnExecute(path, callback, mode);
                }
                else
                {
                    operation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(path), parameters);
                }
            }

            await operation.ToUniTask(UILoading.Show());
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            sceneObject = ResObject.Create(package, scene, path);
            UILoading.Hide();
            return sceneObject;
        }

        public void Release()
        {
        }
    }
}