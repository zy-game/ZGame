using System;
using System.IO;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using UnityEngine;

namespace ZGame.Resource.Config
{
    [Serializable]
    public class OSSOptions
    {
        public string title;
        public OSSType type;
        public string appid;
        public string bucket;
        public string region;
        public string key;
        public string password;

        public string BucketFullName => $"{bucket}-{appid}";

        [NonSerialized] public bool isOn;


        public void Upload(string filePath, bool isEncrypt = false)
        {
            if (File.Exists(filePath) is false)
            {
                throw new FileNotFoundException(filePath);
            }

            switch (type)
            {
                case OSSType.Streaming:
                    string path = GetFilePath(Path.GetFileName(filePath));
                    if (Directory.Exists(Application.streamingAssetsPath) is false)
                    {
                        Directory.CreateDirectory(Application.streamingAssetsPath);
                    }

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    File.Copy(filePath, path, true);
                    break;
                case OSSType.Aliyun:
                    // 填写Bucket所在地域对应的Endpoint。以华东1（杭州）为例，Endpoint填写为https://oss-cn-hangzhou.aliyuncs.com。
                    var endpoint = "https://oss-cn-hangzhou.aliyuncs.com";
// 从环境变量中获取访问凭证。运行本代码示例之前，请确保已设置环境变量OSS_ACCESS_KEY_ID和OSS_ACCESS_KEY_SECRET。
                    var accessKeyId = Environment.GetEnvironmentVariable("OSS_ACCESS_KEY_ID");
                    var accessKeySecret = Environment.GetEnvironmentVariable("OSS_ACCESS_KEY_SECRET");
// 填写Bucket名称，例如examplebucket。
                    var bucketName = "examplebucket";
// 填写Object完整路径，完整路径中不包含Bucket名称，例如exampledir/exampleobject.txt。
                    var objectName = "exampledir/exampleobject.txt";
                    var objectContent = "More than just cloud.";
// 创建OSSClient实例。
                    var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
                    try
                    {
                        // 生成签名URL。
                        var generatePresignedUriRequest = new GeneratePresignedUriRequest(bucketName, objectName, SignHttpMethod.Put)
                        {
                            // 设置签名URL过期时间，默认值为3600秒。
                            Expiration = DateTime.Now.AddHours(1),
                        };
                        var signedUrl = client.GeneratePresignedUri(generatePresignedUriRequest);
                    }
                    catch (OssException ex)
                    {
                        Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                            ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed with error info: {0}", ex.Message);
                    }

                    break;
                case OSSType.Tencent:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Log("upload:" + filePath);
        }

        public string GetFilePath(string fileName)
        {
            switch (type)
            {
                case OSSType.Streaming:
                    return Path.Combine(Application.streamingAssetsPath, fileName.ToLower());
                case OSSType.Aliyun:
                    return $"https://{bucket}.oss-{region}.aliyuncs.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                case OSSType.Tencent:
                    return $"https://{bucket}.cos.{region}.myqcloud.com/{BasicConfig.GetPlatformName()}/{fileName.ToLower()}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}