using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Editor.ResBuild.Config
{
    public class UploadSeting : ScriptableObject
    {
        public List<OSSOptions> optionsList;
    }

    [Serializable]
    public class OSSOptions
    {
        public bool use;
        public OSSType type;
        public string address;
        public string key;
        public string password;
    }
}