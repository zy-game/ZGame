using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ZGame.UI
{
    public class UIBind : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] public TextAsset language;
        [SerializeField] public string NameSpace;
        [SerializeField] public bool templetee;
        [SerializeField] public UnityEngine.Object output;
        [SerializeField] public List<string> reference = new List<string>();
        [SerializeField] public List<UIBindData> options = new List<UIBindData>();
#endif
    }

    [Serializable]
    public class UIBindData
    {
        public string name;
        public string path;
        public int language;
        public bool bindLanguage;
        public Selector selector;
        [NonSerialized] public GameObject target;
        [NonSerialized] public bool isOn;

        public bool isText
        {
            get
            {
                if (selector is null || selector.Count == 0)
                {
                    return false;
                }

                return selector.items.Exists(x => x.name.EndsWith(new[]
                {
                    typeof(TextMeshProUGUI).Name,
                    typeof(Text).Name,
                    typeof(Image).Name,
                    typeof(RawImage).Name
                }));
            }
        }
    }
}