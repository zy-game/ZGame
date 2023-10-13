using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Runtime.Language;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Window
{
    /// <summary>
    /// UI界面本地化配置
    /// </summary>
    public interface IUIWindowOptions : IOptions
    {
        void Initialize(UIWindow window);
        void SetOptions(string path, int id);
        void RemoveOptions(string path);
        void SetBindPipeline(string path, UIBindType type);
        void RemoveBindPipeline(string path);


        public static IUIWindowOptions Create(string optionsPath)
        {
            IRequestAssetObjectResult requestAssetObjectResult = ZGame.Resource.LoadAsset(optionsPath);
            if (requestAssetObjectResult.status is not Status.Success)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<UIWindowOptions>(requestAssetObjectResult.GetObject<TextAsset>().text);
        }

        public static void Save(IUIWindowOptions options, string savePath)
        {
#if UNITY_EDITOR
            File.WriteAllText(savePath, JsonConvert.SerializeObject(options));
#endif
        }

        class UIWindowOptions : IUIWindowOptions
        {
            public int id { get; set; }
            public string name { get; set; }
            public string describe { get; set; }
            public Dictionary<string, int> info;
            public Dictionary<string, UIBindType> binds;

            public void Dispose()
            {
                info.Clear();
                binds.Clear();
                GC.SuppressFinalize(this);
            }

            public void Initialize(UIWindow window)
            {
                foreach (var VARIABLE in info)
                {
                    Transform transform = window.gameObject.transform.Find(VARIABLE.Key);
                    if (transform == null)
                    {
                        continue;
                    }

                    ILanguageOptions languageOptions = ZGame.Localization.GetLocalizationOptions(VARIABLE.Value);
                    if (languageOptions is null)
                    {
                        continue;
                    }

                    languageOptions.SetLanguage(transform);
                }

                foreach (var VARIABLE in binds)
                {
                    window.SetBindPipeline(VARIABLE.Key, IUIBindPipeline.Create(window, VARIABLE.Key, VARIABLE.Value));
                }
            }

            public void SetOptions(string path, int id)
            {
                if (info.ContainsKey(path) is false)
                {
                    info.Add(path, id);
                    return;
                }

                info[path] = id;
            }

            public void RemoveOptions(string path)
            {
                if (info.ContainsKey(path) is false)
                {
                    return;
                }

                info.Remove(path);
            }

            public void SetBindPipeline(string path, UIBindType type)
            {
                throw new NotImplementedException();
            }

            public void RemoveBindPipeline(string path)
            {
                throw new NotImplementedException();
            }
        }
    }
}