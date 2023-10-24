using COSXML.Common;
using COSXML.Model.Tag;
using COSXML.Network;
using COSXML.CosException;
using System;
using System.Collections.Generic;
using System.Text;

namespace COSXML.Model.Bucket
{
    public sealed class PutBucketLoggingRequest : BucketRequest
    {
        private BucketLoggingStatus bucketLoggingStatus;

        public PutBucketLoggingRequest(string bucket) : base(bucket)
        {
            this.method = CosRequestMethod.PUT;
            this.queryParameters.Add("logging", null);
            this.bucketLoggingStatus = new BucketLoggingStatus();
        }

        public void SetTarget(string targetBucket, string targetPrefix = "")
        {

            if (targetBucket == null || targetBucket == "")
            {
                throw new CosClientException((int)CosClientError.InvalidArgument,
                  "targetBucket is null or empty");
            }

            // 可选参数，传 null 填入空白
            if (targetPrefix == null)
            {
                targetPrefix = "";
            }

            if (bucketLoggingStatus.loggingEnabled == null)
            {
                bucketLoggingStatus.loggingEnabled = new BucketLoggingStatus.LoggingEnabled();
            }

            bucketLoggingStatus.loggingEnabled.targetBucket = targetBucket;

            bucketLoggingStatus.loggingEnabled.targetPrefix = targetPrefix;
        }

        public override Network.RequestBody GetRequestBody()
        {
            return GetXmlRequestBody(bucketLoggingStatus);
        }
    }
}
