using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource;
using ZGame.Resource.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild
{
    [SubPageSetting("资源", null, false, typeof(BuilderConfig))]
    public class ResBuilder : SubPage
    {
        private const string key = "__build config__";

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();


            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Package List", EditorStyles.boldLabel);
            GUILayout.Space(5);
            foreach (var VARIABLE in BuilderConfig.instance.packages)
            {
                if (VARIABLE.items == null || (search.IsNullOrEmpty() is false && VARIABLE.name.StartsWith(search) is false))
                {
                    continue;
                }

                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                VARIABLE.selection = GUILayout.Toggle(VARIABLE.selection, VARIABLE.name, GUILayout.Width(300));
                GUILayout.Space(40);
                GUILayout.Label(VARIABLE.describe);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    EditorManager.SwitchScene<ResPackageSetting>();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    OnBuildBundle(new BuilderOptions[1]
                    {
                        new BuilderOptions(VARIABLE)
                    });
                }

                GUILayout.Space(2);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            if (GUILayout.Button("构建资源"))
            {
                OnBuild();
            }

            if (EditorGUI.EndChangeCheck())
            {
                BuilderConfig.OnSave();
            }
        }

        private void OnBuild()
        {
            List<BuilderOptions> builds = new List<BuilderOptions>();
            foreach (var VARIABLE in BuilderConfig.instance.packages)
            {
                if (VARIABLE.selection is false)
                {
                    continue;
                }

                builds.Add(new BuilderOptions(VARIABLE));
            }

            OnBuildBundle(builds.ToArray());
            EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
        }

        public static string OnBuildBundle(params BuilderOptions[] builds)
        {
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            foreach (var VARIABLE in builds)
            {
                list.AddRange(VARIABLE.builds);
            }

            BuildTarget target = BuilderConfig.instance.useActiveTarget ? EditorUserBuildSettings.activeBuildTarget : BuilderConfig.instance.target;
            string output = BasicConfig.GetPlatformOutputPath(BuilderConfig.output);
            if (Directory.Exists(output) is false)
            {
                Directory.CreateDirectory(output);
            }

            var manifest = BuildPipeline.BuildAssetBundles(output, list.ToArray(), BuildAssetBundleOptions.None, target);
            OnUploadResourcePackageList(output, CreatePackageManifest(output, manifest, builds), builds);
            return output;
        }

        private static List<ResourcePackageListManifest> CreatePackageManifest(string output, AssetBundleManifest manifest, params BuilderOptions[] builds)
        {
            BuildPipeline.GetCRCForAssetBundle(new DirectoryInfo(output).Name, out uint crc);
            List<ResourcePackageListManifest> packageListManifests = new List<ResourcePackageListManifest>();
            foreach (var VARIABLE in builds)
            {
                ResourcePackageListManifest packageListManifest = new ResourcePackageListManifest();
                packageListManifest.name = VARIABLE.seting.name;
                packageListManifest.packages = new ResourcePackageManifest[VARIABLE.builds.Length];
                packageListManifest.version = Crc32.GetCRC32Str(DateTime.Now.ToString("g"));
                for (int i = 0; i < VARIABLE.builds.Length; i++)
                {
                    string[] dependencies = manifest.GetAllDependencies(VARIABLE.builds[i].assetBundleName);
                    BuildPipeline.GetCRCForAssetBundle(output + "/" + VARIABLE.builds[i].assetBundleName, out crc);
                    packageListManifest.packages[i] = new ResourcePackageManifest()
                    {
                        name = VARIABLE.builds[i].assetBundleName.ToLower(),
                        version = crc,
                        owner = packageListManifest.name,
                        files = VARIABLE.builds[i].assetNames.Select(x => x.Replace("\\", "/")).ToArray(),
                        dependencies = dependencies,
                    };
                }

                packageListManifests.Add(packageListManifest);
            }

            foreach (var VARIABLE in packageListManifests)
            {
                File.WriteAllText($"{output}/{VARIABLE.name.ToLower()}.ini", JsonConvert.SerializeObject(VARIABLE));
            }

            return packageListManifests;
        }

        private static void OnUploadResourcePackageList(string output, List<ResourcePackageListManifest> manifests, params BuilderOptions[] builds)
        {
            int allCount = builds.Sum(x => x.builds.Length * x.seting.service.Selected.Length);
            int successCount = 0;
            foreach (var options in builds)
            {
                foreach (var VARIABLE in options.seting.service.Selected)
                {
                    OSSOptions oss = OSSConfig.instance.ossList.Find(x => x.title == VARIABLE);
                    if (oss is null)
                    {
                        continue;
                    }

                    foreach (var bundle in options.builds)
                    {
                        Upload(oss, output + "/" + bundle.assetBundleName);
                        successCount++;
                        EditorUtility.DisplayProgressBar("上传进度", successCount + "/" + allCount, successCount / (float)allCount);
                    }

                    foreach (ResourcePackageListManifest manifest in manifests)
                    {
                        Upload(oss, output + "/" + manifest.name + ".ini");
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Aliyun.OSS.OssClient client;
        private static COSXML.CosXmlServer server;

        public static void Upload(OSSOptions options, string filePath, bool isEncrypt = false)
        {
            if (File.Exists(filePath) is false)
            {
                throw new FileNotFoundException(filePath);
            }

            switch (options.type)
            {
                case OSSType.Streaming:
                    StreamingUpload(options, filePath);
                    break;
                case OSSType.Aliyun:
                    OSSUpload(options, filePath);
                    break;
                case OSSType.Tencent:
                    COSUpload(options, filePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Log("upload:" + filePath);
        }

        private static void StreamingUpload(OSSOptions options, string filePath)
        {
            string path = options.GetFilePath(Path.GetFileName(filePath));
            if (Directory.Exists(Application.streamingAssetsPath) is false)
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Copy(filePath, path, true);
        }

        private static void OSSUpload(OSSOptions options, string filePath)
        {
            string putName = $"{BasicConfig.GetPlatformName()}/{Path.GetFileName(filePath)}".ToLower();
            if (client is null)
            {
                var _configuration = new Aliyun.OSS.Common.ClientConfiguration();
                _configuration.ConnectionTimeout = 10000;
                string endpoint = $"oss-cn-hangzhou.aliyuncs.com";
                client = new Aliyun.OSS.OssClient(endpoint, options.key, options.password, _configuration);
            }

            if (client.DoesBucketExist(options.bucket) is false)
            {
                var request = new Aliyun.OSS.CreateBucketRequest(options.bucket);
                request.ACL = Aliyun.OSS.CannedAccessControlList.PublicRead;
                request.DataRedundancyType = Aliyun.OSS.DataRedundancyType.ZRS;
                client.CreateBucket(request);
            }

            try
            {
                Aliyun.OSS.PutObjectResult putObjectResult = client.PutObject(options.bucket, putName, filePath);
                if (putObjectResult.HttpStatusCode != HttpStatusCode.OK)
                {
                    Debug.Log("上传文件错误：" + filePath);
                }
                else
                {
                    Debug.Log(putObjectResult.ResponseMetadata);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static async void COSUpload(OSSOptions options, string filePath)
        {
            try
            {
                string putName = $"{BasicConfig.GetPlatformName()}/{Path.GetFileName(filePath)}".ToLower();
                if (server is null)
                {
                    COSXML.CosXmlConfig config = new COSXML.CosXmlConfig.Builder().SetRegion(options.region).Build();
                    string secretId = options.key;
                    string secretKey = options.password;
                    long durationSecond = 600;
                    COSXML.Auth.QCloudCredentialProvider qCloudCredentialProvider = new COSXML.Auth.DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
                    server = new COSXML.CosXmlServer(config, qCloudCredentialProvider);
                }

                if (server.DoesBucketExist(new COSXML.Model.Bucket.DoesBucketExistRequest(options.bucket)) is false)
                {
                    server.PutBucket(new COSXML.Model.Bucket.PutBucketRequest(options.bucket));
                }

                COSXML.Transfer.TransferConfig transferConfig = new COSXML.Transfer.TransferConfig();
                transferConfig.DivisionForUpload = 10 * 1024 * 1024;
                transferConfig.SliceSizeForUpload = 2 * 1024 * 1024;
                COSXML.Transfer.TransferManager transferManager = new COSXML.Transfer.TransferManager(server, transferConfig);
                COSXML.Transfer.COSXMLUploadTask uploadTask = new COSXML.Transfer.COSXMLUploadTask(options.bucket, putName);
                uploadTask.SetSrcPath(filePath);
                uploadTask.progressCallback = delegate(long completed, long total) { Console.WriteLine(String.Format("progress = {0:##.##}%", completed * 100.0 / total)); };
                COSXML.Transfer.COSXMLUploadTask.UploadTaskResult result = await transferManager.UploadAsync(uploadTask);
                Debug.Log(result.GetResultInfo());
                string eTag = result.eTag;
            }
            catch (Exception e)
            {
                Debug.Log("CosException: " + e);
            }
        }
    }
}