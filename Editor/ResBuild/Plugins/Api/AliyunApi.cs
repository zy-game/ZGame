using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using NUnit.Framework;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class AliyunApi : OSSApi
    {
        private OssClient client;
        private OSSOptions options;

        public AliyunApi(OSSOptions options)
        {
            this.options = options;
            Initialize();
        }

        void Initialize()
        {
            string url = $"oss-{options.region}.aliyuncs.com";
            Debug.Log("url:" + url);
            client = new OssClient(url, options.key, options.password);

            try
            {
                if (Exist(options.bucket))
                {
                    Debug.Log("已存在存储桶：" + options.bucket);
                    return;
                }

                CreateBucket(options.bucket);
            }
            catch (Exception ex)
            {
                Debug.Log($"Check object Exist failed. {ex}");
            }
        }

        public void CreateBucket(string bucketName)
        {
            try
            {
                Debug.Log("creating bucket :" + bucketName);
                var request = new CreateBucketRequest(bucketName);
                //设置读写权限ACL为公共读PublicRead，默认为私有权限。
                request.ACL = CannedAccessControlList.PublicRead;
                //设置数据容灾类型为同城冗余存储。
                request.DataRedundancyType = DataRedundancyType.ZRS;
                client.CreateBucket(request);
                Console.WriteLine("Create bucket succeeded");
            }
            catch (OssException ex)
            {
                Debug.LogFormat("Failed with error info: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}", ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
            }
        }

        private bool Exist(string bucketName)
        {
            try
            {
                return client.DoesBucketExist(bucketName);
            }
            catch (OssException ex)
            {
                Debug.LogFormat("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}", ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
            }
            catch (Exception ex)
            {
                Debug.LogFormat("Failed with error info: {0}", ex.Message);
            }

            return false;
        }

        public override List<OSSObject> GetFileList()
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
    }
}