using System;
using System.Collections.Generic;
using System.IO;
using COSXML;
using COSXML.Auth;
using COSXML.Common;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using COSXML.Model.Tag;
using COSXML.Transfer;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class TencentCloudApi : OSSApi
    {
        private COSXML.CosXml cosXml;

        public TencentCloudApi(OSSOptions options) : base(options)
        {
            CosXmlConfig config = new CosXmlConfig.Builder()
                .SetRegion(options.region)
                .Build();
            long durationSecond = 600; //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(options.key, options.password, durationSecond);
            this.cosXml = new CosXmlServer(config, qCloudCredentialProvider);
            try
            {
                DoesBucketExistRequest bucketExistRequest = new DoesBucketExistRequest(options.BucketFullName);
                if (cosXml.DoesBucketExist(bucketExistRequest))
                {
                    return;
                }

                PutBucketRequest createBucketRequest = new PutBucketRequest(options.bucket);
                createBucketRequest.SetCosACL(CosACL.PublicRead);
                PutBucketResult result = cosXml.PutBucket(createBucketRequest);
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                Debug.Log("CosClientException: " + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                Debug.Log("CosServerException: " + serverEx.GetInfo());
            }
        }

        public override bool Exist(string key)
        {
            throw new NotImplementedException();
        }

        public override void Delete(string key)
        {
            throw new NotImplementedException();
        }

        public override List<OSSObject> GetObjectList()
        {
            List<OSSObject> files = new List<OSSObject>();
            try
            {
                ListBucket info = null;
                string bucket = $"{options.bucket}-{options.appid}";
                string nextMarker = String.Empty;
                do
                {
                    GetBucketRequest request = new GetBucketRequest(bucket);
                    if (nextMarker.IsNullOrEmpty() is false)
                    {
                        request.SetMarker(nextMarker);
                    }

                    GetBucketResult result = cosXml.GetBucket(request);
                    info = result.listBucket;
                    nextMarker = info.nextMarker;

                    foreach (var item in info.contentsList)
                    {
                        files.Add(new OSSObject(item.key));
                    }
                } while (info.isTruncated);
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                Debug.Log("CosClientException: " + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                Debug.Log("CosServerException: " + serverEx.GetInfo());
            }

            return files;
        }

        public override async void Upload(OSSObject obj, Action<float> progress)
        {
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();
            // 手动设置开始分块上传的大小阈值为10MB，默认值为5MB
            transferConfig.DivisionForUpload = 10 * 1024 * 1024;
            // 手动设置分块上传中每个分块的大小为2MB，默认值为1MB
            transferConfig.SliceSizeForUpload = 2 * 1024 * 1024;
            TransferManager transferManager = new TransferManager(cosXml, transferConfig);
            COSXMLUploadTask uploadTask = new COSXMLUploadTask($"{options.bucket}-{options.appid}", obj.fullPath);
            uploadTask.SetSrcPath(obj.localPath);
            uploadTask.progressCallback = delegate(long completed, long total) { progress.Invoke((float)completed / (float)total); };
            try
            {
                COSXML.Transfer.COSXMLUploadTask.UploadTaskResult result = await transferManager.UploadAsync(uploadTask);
                Debug.Log(result.GetResultInfo());
            }
            catch (Exception e)
            {
                Debug.Log("CosException: " + e);
            }
        }


        public override async void Download(OSSObject obj, Action<float> progress)
        {
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();
            transferConfig.DivisionForDownload = 20 * 1024 * 1024;
            transferConfig.SliceSizeForDownload = 10 * 1024 * 1024;
            TransferManager transferManager = new TransferManager(cosXml, transferConfig);
            String bucket = $"{options.bucket}-{options.appid}"; //存储桶，格式：BucketName-APPID
            String cosPath = obj.fullPath; //对象在存储桶中的位置标识符，即称对象键
            string localDir = Path.GetDirectoryName(obj.localPath); //本地文件夹
            string localFileName = obj.name; //指定本地保存的文件名
            COSXMLDownloadTask downloadTask = new COSXMLDownloadTask(bucket, cosPath, localDir, localFileName);
            try
            {
                COSXML.Transfer.COSXMLDownloadTask.DownloadTaskResult result = await transferManager.DownloadAsync(downloadTask);
                progress?.Invoke(1);
                Debug.Log(obj.localPath + "\n" + result.GetResultInfo());
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                Console.WriteLine("CosClientException: " + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                Console.WriteLine("CosServerException: " + serverEx.GetInfo());
            }
        }
    }
}