using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZEngine.Resource;
using Object = UnityEngine.Object;

namespace ZEngine.Editor.ResourceBuilder
{
    public class GUIStyleViewer : EditorWindow
    {
        private Vector2 scrollVector2 = Vector2.zero;
        private string search = "";

        [MenuItem("UFramework/GUIStyle查看器")]
        public static void InitWindow()
        {
            EditorWindow.GetWindow(typeof(GUIStyleViewer));
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal("HelpBox");
            GUILayout.Space(30);
            search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(position.x / 3));
            GUILayout.Label("", "SearchCancelButtonEmpty");
            GUILayout.EndHorizontal();
            scrollVector2 = GUILayout.BeginScrollView(scrollVector2);
            foreach (GUIStyle style in GUI.skin.customStyles)
            {
                if (style.name.ToLower().Contains(search.ToLower()))
                {
                    DrawStyleItem(style);
                }
            }

            GUILayout.EndScrollView();
        }

        void DrawStyleItem(GUIStyle style)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(40);
            EditorGUILayout.SelectableLabel(style.name);
            GUILayout.FlexibleSpace();
            EditorGUILayout.SelectableLabel(style.name, style);
            GUILayout.Space(40);
            EditorGUILayout.SelectableLabel("", style, GUILayout.Height(40), GUILayout.Width(40));
            GUILayout.Space(50);
            if (GUILayout.Button("复制到剪贴板"))
            {
                TextEditor textEditor = new TextEditor();
                textEditor.text = style.name;
                textEditor.OnFocus();
                textEditor.Copy();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
    }

    public class GameResourceBuilder : EditorWindow
    {
        [MenuItem("Game/Build Package")]
        public static void Open()
        {
            GetWindow<GameResourceBuilder>(false, "Package Builder", true);
        }

        private string search;
        private SerializedObject options;
        private Switch isOptions = Switch.Off;
        private ResourceModuleManifest selection;
        private Vector2 listScroll = Vector2.zero;
        private Vector2 optionsScroll = Vector2.zero;
        private Vector2 manifestScroll = Vector2.zero;
        private Color inColor = new Color(1f, 0.92f, 0.01f, .8f);
        private Color outColor = new Color(0, 0, 0, 0.2f);

        public void OnEnable()
        {
            options = new SerializedObject(ResourceModuleOptions.instance);
            if (ResourceModuleOptions.instance.modules is null || ResourceModuleOptions.instance.modules.Count is 0)
            {
                return;
            }

            selection = null;
            foreach (var module in ResourceModuleOptions.instance.modules)
            {
                if (module.folder == null)
                {
                    continue;
                }

                foreach (var bundle in module.bundles)
                {
                    if (bundle.folder == null)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(bundle.folder);
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    bundle.files = new List<Object>();
                    foreach (var VARIABLE in files)
                    {
                        if (VARIABLE.EndsWith(".meta"))
                        {
                            continue;
                        }

                        bundle.files.Add(AssetDatabase.LoadAssetAtPath<Object>(VARIABLE));
                    }
                }
            }
        }

        public void OnGUI()
        {
            Toolbar();
            switch (isOptions)
            {
                case Switch.On:
                    DrawingOptions();
                    break;
                case Switch.Off:
                    DrawingResourceModule();
                    break;
            }
        }

        void Toolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.FlexibleSpace();
                search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(300));
                if (GUILayout.Button("Options", EditorStyles.toolbarButton))
                {
                    isOptions = isOptions == Switch.Off ? Switch.On : Switch.Off;
                }

                if (GUILayout.Button("Build", EditorStyles.toolbarButton))
                {
                    if (ResourceModuleOptions.instance.modules is null || ResourceModuleOptions.instance.modules.Count is 0)
                    {
                        return;
                    }

                    List<ResourceBundleManifest> manifests = new List<ResourceBundleManifest>();
                    foreach (var VARIABLE in ResourceModuleOptions.instance.modules)
                    {
                        manifests.AddRange(VARIABLE.bundles);
                    }

                    OnBuild(manifests.ToArray());
                }

                GUILayout.EndHorizontal();
            }
        }

        void DrawingOptions()
        {
            optionsScroll = GUILayout.BeginScrollView(optionsScroll);
            {
                EditorGUI.BeginChangeCheck();
                {
                    GUILayout.BeginVertical("Options", EditorStyles.helpBox);
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(options.FindProperty("options"), true);
                        GUILayout.EndVertical();
                    }
                    GUILayout.BeginVertical("Module Options", EditorStyles.helpBox);
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(options.FindProperty("modules"), true);
                        GUILayout.EndVertical();
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        options.ApplyModifiedProperties();
                        ResourceModuleOptions.instance.Saved();
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        void DrawingResourceModule()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(300), GUILayout.Height(position.height - 30));
                    {
                        listScroll = GUILayout.BeginScrollView(listScroll);
                        {
                            DrawingMoudleList();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }


                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(position.width - 310), GUILayout.Height(position.height - 30));
                    {
                        manifestScroll = GUILayout.BeginScrollView(manifestScroll, false, true);
                        {
                            DrawingModuleManifest();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }


        private void DrawingMoudleList()
        {
            if (ResourceModuleOptions.instance.modules is null || ResourceModuleOptions.instance.modules.Count is 0)
            {
                return;
            }


            for (int i = 0; i < ResourceModuleOptions.instance.modules.Count; i++)
            {
                ResourceModuleManifest moduleManifest = ResourceModuleOptions.instance.modules[i];
                if (search.IsNullOrEmpty() is false && moduleManifest.Search(search) is false)
                {
                    continue;
                }

                Rect contains = EditorGUILayout.BeginVertical();
                {
                    this.BeginColor(moduleManifest == selection ? Color.cyan : GUI.color);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label($"{moduleManifest.title}", "LargeBoldLabel");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label($"v:{moduleManifest.version}");
                            GUILayout.EndHorizontal();
                        }
                        this.EndColor();
                    }

                    GUILayout.Space(5);
                    this.BeginColor(moduleManifest == selection ? inColor : outColor);
                    {
                        GUILayout.Box("", "WhiteBackground", GUILayout.Width(300), GUILayout.Height(1));
                        this.EndColor();
                    }
                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        selection = moduleManifest;
                        this.Repaint();
                    }

                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Build"), false, () => { OnBuild(moduleManifest.bundles.ToArray()); });
                        menu.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            ResourceModuleOptions.instance.modules.Remove(moduleManifest);
                            ResourceModuleOptions.instance.Saved();
                            this.Repaint();
                        });
                        menu.ShowAsContext();
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }
        }

        private void DrawingModuleManifest()
        {
            if (selection == null)
            {
                return;
            }

            if (selection.folder == null || selection.bundles == null || selection.bundles.Count == 0)
            {
                return;
            }

            for (int i = 0; i < selection.bundles.Count; i++)
            {
                ResourceBundleManifest manifest = selection.bundles[i];

                if (manifest.folder == null)
                {
                    continue;
                }

                Rect contains = EditorGUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Space(5);
                            manifest.isOn = GUILayout.Toggle(manifest.isOn, "");
                            GUILayout.EndVertical();
                            string name = (manifest.name.IsNullOrEmpty() ? $"Empty" : manifest.name) + $"({AssetDatabase.GetAssetPath(manifest.folder)})";
                            GUILayout.Label(name, "LargeBoldLabel");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label($"v:{manifest.version.ToString()}");
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Space(5);
                    this.BeginColor(new Color(0, 0, 0, .2f));
                    {
                        GUILayout.Box("", "WhiteBackground", GUILayout.Height(1));
                        this.EndColor();
                    }
                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        manifest.foldout = !manifest.foldout;
                        this.Repaint();
                    }

                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Build"), false, () => { OnBuild(manifest); });
                        menu.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            selection.bundles.Remove(manifest);
                            ResourceModuleOptions.instance.Saved();
                        });
                        menu.ShowAsContext();
                    }

                    GUILayout.EndVertical();
                }
                GUI.enabled = false;
                if (manifest.foldout && manifest.files is not null && manifest.files.Count > 0)
                {
                    foreach (var VARIABLE in manifest.files)
                    {
                        if (search.IsNullOrEmpty() is false && VARIABLE.name.Contains(search) is false)
                        {
                            continue;
                        }

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(23);
                            EditorGUILayout.ObjectField(VARIABLE, typeof(Object));
                            GUILayout.EndHorizontal();
                        }
                    }
                }

                GUI.enabled = true;
                GUILayout.Space(5);
            }
        }

        private void OnBuild(params ResourceBundleManifest[] manifests)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            for (int i = 0; i < manifests.Length; i++)
            {
                if (manifests[i].folder == null || manifests[i].files is null || manifests[i].files.Count is 0)
                {
                    continue;
                }

                ResourceModuleManifest resourceModuleManifest = ResourceModuleOptions.instance.modules.Find(x => x.bundles.Contains(manifests[i]));
                builds.Add(new AssetBundleBuild()
                {
                    assetBundleName = $"{resourceModuleManifest.title}_{manifests[i].name}.assetbundle",
                    assetNames = manifests[i].files.Select(x => AssetDatabase.GetAssetPath(x)).ToArray()
                });
            }

            string output = Application.dataPath + "/../output/assets/" + Engine.Custom.GetPlatfrom();
            if (Directory.Exists(output) is false)
            {
                Directory.CreateDirectory(output);
            }

            try
            {
                AssetBundleManifest bundleManifest = BuildPipeline.BuildAssetBundles(output, builds.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

                Dictionary<ResourceModuleManifest, List<ResourceBundleManifest>> map = new Dictionary<ResourceModuleManifest, List<ResourceBundleManifest>>();
                foreach (var VARIABLE in manifests)
                {
                    ResourceModuleManifest resourceModuleManifest = ResourceModuleOptions.instance.modules.Find(x => x.bundles.Contains(VARIABLE));
                    if (map.TryGetValue(resourceModuleManifest, out List<ResourceBundleManifest> list) is false)
                    {
                        map.Add(resourceModuleManifest, list = new List<ResourceBundleManifest>());
                    }

                    list.Add(VARIABLE);
                }

                RefershResourceConfig(bundleManifest, output, map);
                ResourceModuleOptions.instance.Saved();
                UploadAsset(output, map);
                this.Repaint();
            }
            catch (Exception e)
            {
                Engine.Console.Error(e);
            }
        }

        private void RefershResourceConfig(AssetBundleManifest bundleManifest, string output, Dictionary<ResourceModuleManifest, List<ResourceBundleManifest>> map)
        {
            foreach (var VARIABLE in map)
            {
                RuntimeModuleManifest runtimeModuleManifest = default;
                if (File.Exists(output + $"/{VARIABLE.Key.title}.ini"))
                {
                    runtimeModuleManifest = Engine.Json.Parse<RuntimeModuleManifest>(File.ReadAllText(output + $"/{VARIABLE.Key.title.ToLower()}.ini"));
                }

                if (runtimeModuleManifest is null)
                {
                    runtimeModuleManifest = new RuntimeModuleManifest();
                    runtimeModuleManifest.name = VARIABLE.Key.title.ToLower();
                    runtimeModuleManifest.bundleList = new List<RuntimeBundleManifest>();
                }

                runtimeModuleManifest.version = VARIABLE.Key.version;
                foreach (var manifest in VARIABLE.Value)
                {
                    string bundleName = ($"{VARIABLE.Key.title}_{manifest.name}.assetbundle").ToLower();
                    manifest.version.Up();
                    RuntimeBundleManifest runtimeBundleManifest = runtimeModuleManifest.bundleList.Find(x => x.name == bundleName);
                    if (runtimeBundleManifest is null)
                    {
                        runtimeBundleManifest = new RuntimeBundleManifest();
                        runtimeBundleManifest.files = new List<RuntimeAssetManifest>();
                        runtimeBundleManifest.dependencies = new List<string>();
                        runtimeModuleManifest.bundleList.Add(runtimeBundleManifest);
                        runtimeBundleManifest.owner = runtimeModuleManifest.name;
                        runtimeBundleManifest.name = bundleName;
                    }

                    runtimeBundleManifest.version = manifest.version;
                    runtimeBundleManifest.files.Clear();
                    runtimeBundleManifest.dependencies.Clear();
                    runtimeBundleManifest.length = (int)new FileInfo(output + "/" + runtimeBundleManifest.name).Length;
                    runtimeBundleManifest.dependencies.AddRange(bundleManifest.GetAllDependencies(runtimeBundleManifest.name));
                    runtimeBundleManifest.hash = bundleManifest.GetAssetBundleHash(runtimeBundleManifest.name).ToString();
                    BuildPipeline.GetCRCForAssetBundle(output + "/" + runtimeBundleManifest.name, out uint crc);
                    runtimeBundleManifest.crc = crc;
                    foreach (var file in manifest.files)
                    {
                        RuntimeAssetManifest runtimeAssetManifest = new RuntimeAssetManifest();
                        runtimeAssetManifest.name = file.name;
                        runtimeAssetManifest.path = AssetDatabase.GetAssetPath(file);
                        runtimeAssetManifest.guid = AssetDatabase.AssetPathToGUID(runtimeAssetManifest.path);
                        runtimeBundleManifest.files.Add(runtimeAssetManifest);
                    }
                }

                File.WriteAllText(output + $"/{VARIABLE.Key.title.ToLower()}.ini", JsonConvert.SerializeObject(runtimeModuleManifest));
            }
        }

        private void UploadAsset(string output, Dictionary<ResourceModuleManifest, List<ResourceBundleManifest>> map)
        {
            IEnumerable<OSSOptions> ossList = ResourceModuleOptions.instance.options.Where(x => x.isOn == Switch.On);
            try
            {
                foreach (var upload in map)
                {
                    foreach (var options in ossList)
                    {
                        switch (options.service)
                        {
                            case OSSService.OSS:
                                UploadOSSService(output, options, upload.Key, upload.Value.ToArray());
                                break;
                            case OSSService.COS:
                                UploadCosService(output, options, upload.Key, upload.Value.ToArray());
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Engine.Console.Error(e);
            }

            EditorUtility.ClearProgressBar();
        }

        private void UploadOSSService(string output, OSSOptions options, ResourceModuleManifest moduleManifest, params ResourceBundleManifest[] manifests)
        {
            float count = manifests.Length;
            float now = 0;
            var _configuration = new Aliyun.OSS.Common.ClientConfiguration();
            _configuration.ConnectionTimeout = 10000;
            Aliyun.OSS.OssClient ossClient = new Aliyun.OSS.OssClient(options.url, options.keyID, options.key, _configuration);
            if (ossClient.DoesBucketExist(options.bucket) is false)
            {
                Aliyun.OSS.Bucket bucket = ossClient.CreateBucket(options.bucket);
                Engine.Console.Log(bucket.ToString());
            }

            string key = String.Empty;
            Aliyun.OSS.PutObjectResult result = default;
            foreach (var file in manifests)
            {
                string bundleName = ($"{moduleManifest.title}_{file.name}.assetbundle").ToLower();
                key = $"{Engine.Custom.GetPlatfrom()}/{bundleName}";
                result = ossClient.PutObject(options.bucket, key, $"{output}/{bundleName}");
                Engine.Console.Log(result.ETag);
                now++;
                EditorUtility.DisplayProgressBar("上传资源", "正在上传OSS...", now / count);
            }

            key = $"{Engine.Custom.GetPlatfrom()}/{moduleManifest.title.ToLower()}.ini";
            result = ossClient.PutObject(options.bucket, key, $"{output}/{moduleManifest.title.ToLower()}.ini");
            EditorUtility.DisplayProgressBar("上传资源", "正在上传配置文件...", now / count);
        }

        private void UploadCosService(string output, OSSOptions options, ResourceModuleManifest moduleManifest, params ResourceBundleManifest[] manifests)
        {
            float count = manifests.Length;
            float now = 0;
            long durationSecond = 600; //每次请求签名有效时长，单位为秒
            COSXML.Auth.QCloudCredentialProvider cosCredentialProvider = new COSXML.Auth.DefaultQCloudCredentialProvider(options.keyID, options.key, durationSecond);
            COSXML.CosXmlConfig config = new COSXML.CosXmlConfig.Builder().IsHttps(true).SetRegion(options.url).SetDebugLog(true).Build();
            COSXML.CosXmlServer cosClient = new COSXML.CosXmlServer(config, cosCredentialProvider);
            COSXML.Model.Bucket.DoesBucketExistRequest bucketExistRequest = new COSXML.Model.Bucket.DoesBucketExistRequest(options.bucket);
            string key = String.Empty;
            COSXML.Model.Object.PutObjectResult result = default;
            COSXML.Model.Object.PutObjectRequest request = default;
            if (cosClient.DoesBucketExist(bucketExistRequest) is false)
            {
                COSXML.Model.Bucket.PutBucketRequest bucketRequestrequest = new COSXML.Model.Bucket.PutBucketRequest(options.bucket);
                COSXML.Model.Bucket.PutBucketResult bucketResult = cosClient.PutBucket(bucketRequestrequest);
                Engine.Console.Log(result.GetResultInfo());
            }


            foreach (var file in manifests)
            {
                string bundleName = ($"{moduleManifest.title}_{file.name}.assetbundle").ToLower();
                key = ($"{Engine.Custom.GetPlatfrom()}/{bundleName}").ToLower();
                request = new COSXML.Model.Object.PutObjectRequest(options.bucket, key, $"{output}/{bundleName}");
                result = cosClient.PutObject(request);
                Engine.Console.Log(result.eTag);
                now++;
                EditorUtility.DisplayProgressBar("上传资源", "正在上传COS...", now / count);
            }

            key = ($"{Engine.Custom.GetPlatfrom()}/{moduleManifest.title}.ini").ToLower();
            request = new COSXML.Model.Object.PutObjectRequest(options.bucket, key, $"{output}/{moduleManifest.title.ToLower()}.ini");
            result = cosClient.PutObject(request);
            EditorUtility.DisplayProgressBar("上传资源", "正在上传配置文件...", now / count);
        }
    }
}