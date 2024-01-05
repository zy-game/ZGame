using System.Collections.Generic;

namespace ZGame.Resource.Config
{
    [ResourceReference("Resources/Config/OSSConfig.asset")]
    public class OSSConfig : SingletonScriptableObject<OSSConfig>
    {
        public List<OSSOptions> ossList;

        public override void OnAwake()
        {
        }

        public string GetFilePath(string title, string fileName)
        {
            OSSOptions options = ossList.Find(x => x.title == title);
            if (options is not null)
            {
                return options.GetFilePath(fileName);
            }
            else
            {
                return "";
            }
        }

        public OSSType GetOSSType(string title)
        {
            OSSOptions options = ossList.Find(x => x.title == title);
            if (options is not null)
            {
                return options.type;
            }
            else
            {
                return OSSType.None;
            }
        }
    }
}