using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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
        [LabelText("$GetTitleName")] [SerializeField]
        public string NameSpace;

        [LabelText("输出目录"), SerializeField, HideIf("isTemplete")]
        public UnityEngine.Object output;

        // [SerializeField] public List<string> reference = new List<string>();
        [LabelText("Bind List"), TableList, SerializeField]
        public List<UIBindData> options = new List<UIBindData>();

        public string GetTitleName()
        {
            return isTemplete() ? "模板名称" : "命名空间";
        }

        public bool isTemplete()
        {
            return this.transform.parent?.GetComponentInParent<UIBind>() != null;
        }
#endif
    }

    [Serializable]
    public class UIBindData
    {
        public GameObject target;
        [HideInInspector] public string name;
        [HideInInspector] public string path;

        [Selector("OnInitialized"), LabelText("$selectionList")]
        public List<string> selection;

        [HideInInspector] public Selector selector;
        [NonSerialized] public bool isOn;

        IEnumerable<string> OnInitialized()
        {
            if (target == null)
            {
                return Array.Empty<string>();
            }

            return target.GetComponents(typeof(Component)).Select(x => x.GetType().FullName).ToList();
        }

        string selectionList
        {
            get
            {
                if (selection is null || selection.Count == 0)
                {
                    return "None";
                }

                if (selection is not null && selection.Count == OnInitialized().Count())
                {
                    return "Everything";
                }

                return string.Join(",", selection.Select(x => x.Substring(x.LastIndexOf(".") + 1)));
            }
        }
    }
}