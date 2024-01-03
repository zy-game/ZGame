using System;
using System.Collections.Generic;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class StreamingApi : OSSApi
    {
        public StreamingApi(OSSOptions options) : base(options)
        {
        }

        public override bool Exist(string key)
        {
            return default;
        }

        public override void Delete(string key)
        {
        }

        public override List<OSSObject> GetObjectList()
        {
            return default;
        }

        public override void Upload(OSSObject obj, Action<float> progress)
        {
        }

        public override void Download(OSSObject obj, Action<float> progress)
        {
        }
    }
}