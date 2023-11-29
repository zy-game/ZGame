using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using CodiceApp.EventTracking;
using NUnit.Framework;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class AliyunApi : OSSApi
    {
        private OssClient client;

        public AliyunApi(OSSOptions options) : base(options)
        {
            string url = $"oss-{options.region}.aliyuncs.com"; //$"oss-{options.region}.aliyuncs.com";
            client = new OssClient(url, options.key, options.password);
            try
            {
                if (client.DoesBucketExist(options.bucket))
                {
                    Debug.Log("bucket already exist:" + base.options.bucket);
                    return;
                }

                var request = new CreateBucketRequest(options.bucket);
                //设置读写权限ACL为公共读PublicRead，默认为私有权限。
                request.ACL = CannedAccessControlList.PublicRead;
                client.CreateBucket(request);
                Debug.Log("Create bucket succeeded");
            }
            catch (Exception ex)
            {
                Debug.Log($"Check object Exist failed. {ex}");
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
                ObjectListing result = null;

                string nextMarker = string.Empty;
                do
                {
                    var listObjectsRequest = new ListObjectsRequest(options.bucket)
                    {
                        MaxKeys = 100
                    };
                    if (nextMarker.IsNullOrEmpty() is false)
                    {
                        listObjectsRequest.Marker = nextMarker;
                    }

                    result = client.ListObjects(listObjectsRequest);
                    foreach (var summary in result.ObjectSummaries)
                    {
                        files.Add(new OSSObject(summary.Key));
                    }

                    nextMarker = result.NextMarker;
                } while (result.IsTruncated);
            }
            catch (Exception ex)
            {
                Console.WriteLine("List object failed. {0}", ex.Message);
            }

            return files;
        }

        public override void Upload(OSSObject obj, Action<float> progress)
        {
            try
            {
                using (var fs = File.Open(obj.localPath, FileMode.Open))
                {
                    var putObjectRequest = new PutObjectRequest(options.bucket, obj.fullPath, fs);
                    putObjectRequest.StreamTransferProgress += (EventSender, args) => { progress.Invoke((float)args.TransferredBytes / (float)args.TotalBytes); };
                    client.PutObject(putObjectRequest);
                }

                Debug.LogFormat("Put object:{0} succeeded", obj.localPath);
            }

            catch (OssException ex)
            {
                Debug.LogFormat("Failed with error code: {0}; Error info: {1}. \nRequestID: {2}\tHostID: {3}", ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
            }
            catch (Exception ex)
            {
                Debug.LogFormat("Failed with error info: {0}", ex.Message);
            }

        }

        public override async void Download(OSSObject obj, Action<float> progress)
        {
            try
            {
                var getObjectRequest = new GetObjectRequest(options.bucket, obj.fullPath);
                getObjectRequest.StreamTransferProgress += (s, args) => { progress?.Invoke((float)args.TransferredBytes / (float)args.TotalBytes); };
                // 下载文件。
                var ossObject = client.GetObject(getObjectRequest);
                using (var requestStream = ossObject.Content)
                {
                    byte[] buf = new byte[1024];
                    var fs = File.Open(obj.localPath, FileMode.OpenOrCreate);
                    var len = 0;
                    // 通过输入流将文件的内容读取到文件或者内存中。
                    while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                    {
                        await fs.WriteAsync(buf, 0, len);
                    }

                    fs.Close();
                    progress?.Invoke(1);
                }

                Debug.LogFormat("Get object:{0} succeeded", obj.localPath);
            }
            catch (OssException ex)
            {
                Debug.LogFormat("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}", ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
            }
            catch (Exception ex)
            {
                Debug.LogFormat("Failed with error info: {0}", ex.Message);
            }
        }
    }
}