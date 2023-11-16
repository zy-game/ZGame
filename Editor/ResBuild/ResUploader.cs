using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [BindScene("版本管理", typeof(ResBuilder))]
    public class ResUploader : PageScene
    {
        public OSSType type;
        private int ossIndex = 0;
        private OSSOptions selectin;
        private List<OSSFileEntity> files;

        public override void OnEnable()
        {
            OnRefresh();
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            type = (OSSType)EditorGUILayout.EnumPopup(type);
            string[] list = BuilderConfig.instance.ossList.Where(x => x.type == type).Select(x => x.title).ToArray();
            ossIndex = EditorGUILayout.Popup(ossIndex, list);
            if (GUILayout.Button("刷新列表"))
            {
                OnRefresh();
            }

            if (GUILayout.Button("上传"))
            {
                OnUpload();
            }

            GUILayout.EndHorizontal();
            OnDrawingItem();
        }

        private void OnRefresh()
        {
        }

        private void OnUpload()
        {
        }

        private void OnDrawingItem()
        {
            if (files is null)
            {
                return;
            }
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
            }

            string key = String.Empty;
            Aliyun.OSS.PutObjectResult result = default;
            foreach (var file in manifests)
            {
                string bundleName = ($"{moduleManifest.title}_{file.name}.assetbundle").ToLower();
                key = $"{ZGame.GetPlatfrom()}/{bundleName}";
                result = ossClient.PutObject(options.bucket, key, $"{output}/{bundleName}");
                now++;
                EditorUtility.DisplayProgressBar("上传资源", "正在上传OSS...", now / count);
            }

            key = $"{ZGame.GetPlatfrom()}/{moduleManifest.title.ToLower()}.ini";
            result = ossClient.PutObject(options.bucket, key, $"{output}/{moduleManifest.title.ToLower()}.ini");
            EditorUtility.DisplayProgressBar("上传资源", "正在上传配置文件...", now / count);
        }

        private void UploadCosService(string output, OSSOptions options, ResourceModuleManifest moduleManifest, params ResourceBundleManifest[] manifests)
        {
            float count = manifests.Length;
            float now = 0;
            long durationSecond = 600;
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
            }


            foreach (var file in manifests)
            {
                string bundleName = ($"{moduleManifest.title}_{file.name}.assetbundle").ToLower();
                key = ($"{ZGame.GetPlatfrom()}/{bundleName}").ToLower();
                request = new COSXML.Model.Object.PutObjectRequest(options.bucket, key, $"{output}/{bundleName}");
                result = cosClient.PutObject(request);
                now++;
                EditorUtility.DisplayProgressBar("上传资源", "正在上传COS...", now / count);
            }

            key = ($"{ZGame.GetPlatfrom()}/{moduleManifest.title}.ini").ToLower();
            request = new COSXML.Model.Object.PutObjectRequest(options.bucket, key, $"{output}/{moduleManifest.title.ToLower()}.ini");
            result = cosClient.PutObject(request);
            EditorUtility.DisplayProgressBar("上传资源", "正在上传配置文件...", now / count);
        }
    }

    class OSSFileEntity : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class OSSApi
    {
        
    }
}