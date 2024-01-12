using System.Collections.Generic;

namespace ZGame.Resource.Config
{
    [ResourceReference("Resources/Config/OSSConfig.asset")]
    public class OSSConfig : SingletonScriptableObject<OSSConfig>
    {
        public List<OSSOptions> ossList;
        public string seletion;

        public OSSOptions current
        {
            get { return ossList.Find(x => x.title == seletion); }
        }

        public override void OnAwake()
        {
        }

        public string GetFilePath(string fileName)
        {
            if (current is not null)
            {
                return current.GetFilePath(fileName);
            }
            else
            {
                return "";
            }
        }
    }
}