using System;
using System.Collections.Generic;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public abstract class OSSApi
    {
        public readonly OSSOptions options;

        public OSSApi(OSSOptions options)
        {
            this.options = options;
        }

        public abstract bool Exist(string key);
        public abstract void Delete(string key);

        public abstract List<OSSObject> GetObjectList();
        public abstract void Upload(OSSObject obj,Action<float> progress);
        public abstract void Download(OSSObject obj,Action<float> progress);
    }
}