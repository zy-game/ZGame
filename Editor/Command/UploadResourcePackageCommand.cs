using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using ZGame.Resource.Config;

namespace ZGame.Editor.Command
{
    public class UploadResourcePackageCommand
    {
        private static Aliyun.OSS.OssClient client;
        private static COSXML.CosXmlServer server;


        public static void Executer(params object[] args)
        {
            if (args is null || args.Length < 2)
            {
                return;
            }

            var options = args[0] as OSSOptions;
            var filePath = args[1] as string;
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
            string putName = $"{GameFrameworkEntry.GetPlatformName()}/{Path.GetFileName(filePath)}".ToLower();
            if (client is null)
            {
                var accessKeyId = options.key;
                var accessKeySecret = options.password;
                string endpoint = "https://oss-cn-beijing.aliyuncs.com";
                client = new Aliyun.OSS.OssClient(endpoint, accessKeyId, accessKeySecret);
            }

            var buckets = client.ListBuckets().ToList();
            if (buckets.Exists(x => x.Name.Equals(options.bucket)) is false)
            {
                try
                {
                    var request = new Aliyun.OSS.CreateBucketRequest(options.bucket);
                    //设置读写权限ACL为公共读PublicRead，默认为私有权限。
                    request.ACL = Aliyun.OSS.CannedAccessControlList.PublicRead;
                    //设置数据容灾类型为同城冗余存储。
                    request.DataRedundancyType = Aliyun.OSS.DataRedundancyType.ZRS;
                    client.CreateBucket(request);
                    Debug.Log("Create bucket succeeded");
                }
                catch (Exception ex)
                {
                    Debug.LogError(string.Format("Create bucket failed. {0}", ex.Message));
                }
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

        private static async void COSUpload(OSSOptions options, string filePath)
        {
            try
            {
                string putName = $"{GameFrameworkEntry.GetPlatformName()}/{Path.GetFileName(filePath)}".ToLower();
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