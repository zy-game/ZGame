using System;
using System.Collections.Generic;
using System.IO;
using COSXML;
using COSXML.Auth;
using COSXML.Common;
using COSXML.Model.Bucket;
using COSXML.Model.Tag;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class TencentApi : OSSApi
    {
        private COSXML.CosXml cosXml;
        private OSSOptions options;

        public TencentApi(OSSOptions options)
        {
            this.options = options;
            Initialize();
        }

        void Initialize()
        {
            CosXmlConfig config = new CosXmlConfig.Builder()
                .SetRegion(options.region)
                .Build();
            long durationSecond = 600; //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(options.key, options.password, durationSecond);
            this.cosXml = new CosXmlServer(config, qCloudCredentialProvider);
            try
            {
                if (Exist())
                {
                    return;
                }

                Create();
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

        private bool Create()
        {
            try
            {
                PutBucketRequest createBucketRequest = new PutBucketRequest(options.bucket);
                createBucketRequest.SetCosACL(CosACL.PublicRead);
                PutBucketResult result = cosXml.PutBucket(createBucketRequest);
                return result.IsSuccessful();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return false;
        }

        private bool Exist()
        {
            try
            {
                string bucket = $"{options.bucket}-{options.appid}";
                DoesBucketExistRequest request = new DoesBucketExistRequest(bucket);
                return cosXml.DoesBucketExist(request);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return false;
        }


        public override List<OSSObject> GetFileList()
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
                        Debug.Log(item.key);
                        files.Add(new OSSObject(item.key));
                    }
                } while (info.isTruncated);
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

            return files;
        }
    }
}