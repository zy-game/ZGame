using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using COSXML.Model.Tag;
using COSXML.Transfer;

namespace COSXML.Model.Object
{
    /// <summary>
    /// 简单上传对象返回的结果
    /// <see href="https://cloud.tencent.com/document/product/436/7749"/>
    /// </summary>
    public sealed class PutObjectResult : CosDataResult<PicOperationUploadResult>
    {
        /// <summary>
        /// 对象的eTag
        /// </summary>
        public string eTag;

        /// <summary>
        /// 对象的 crc64
        /// </summary>
        public string crc64ecma;

        public PicOperationUploadResult uploadResult { 
            get {return _data; } 
            }

        internal override void InternalParseResponseHeaders()
        {
            List<string> values;

            this.responseHeaders.TryGetValue("ETag", out values);

            if (values != null && values.Count > 0)
            {
                eTag = values[0];
            }

            this.responseHeaders.TryGetValue("x-cos-hash-crc64ecma", out values);

            if (values != null && values.Count > 0)
            {
                crc64ecma = values[0];
            }
        }
    }
}
