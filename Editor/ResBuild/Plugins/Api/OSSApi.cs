using System.Collections.Generic;

namespace ZGame.Editor.ResBuild
{
    public abstract class OSSApi
    {
        public abstract List<OSSObject> GetFileList();
    }
}