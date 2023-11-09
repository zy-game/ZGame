using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class ResRuleSeting : SubPageScene
    {
        public override string name { get; } = "规则管理";
        private RuleSeting _config;

        public ResRuleSeting(Docker window) : base(window)
        {
        }

        public override PageScene OpenAssetObject(Object obj)
        {
            if (obj is RuleSeting rulerConfig)
            {
                _config = rulerConfig;
                return this;
            }

            return base.OpenAssetObject(obj);
        }

        public override void OnEnable()
        {
            if (_config is null)
            {
                _config = ((ResBuilder)parent).config?.ruleSeting;
            }
        }

        public override void OnDisable()
        {
            EditorUtility.SetDirty(_config);
        }

        public override void OnGUI(string search, Rect rect)
        {
            if (_config is null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
            {
                _config.rulers.Add(new RulerInfoItem());
            }

            GUILayout.EndHorizontal();
            for (int i = _config.rulers.Count - 1; i >= 0; i--)
            {
                if (search.IsNullOrEmpty() is false && _config.rulers[i].name.Contains(search) is false)
                {
                    continue;
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawingRuleInfo(_config.rulers[i]);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }
        }

        private void DrawingRuleInfo(RulerInfoItem ruler)
        {
            GUILayout.BeginHorizontal();
            ruler.use = EditorGUILayout.Toggle("是否激活规则", ruler.use);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
            {
                _config.rulers.Remove(ruler);
            }

            GUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(!ruler.use);
            ruler.name = EditorGUILayout.TextField("规则名称", ruler.name);
            ruler.folder = EditorGUILayout.ObjectField("资源目录", ruler.folder, typeof(DefaultAsset), false);
            ruler.ignore = EditorGUILayout.TextField("忽略文件后缀", ruler.ignore);
            ruler.spiltPackageType = (SpiltPackageType)EditorGUILayout.EnumPopup("分包规则", ruler.spiltPackageType);
            ruler.describe = EditorGUILayout.TextField("描述", ruler.describe);

            EditorGUI.EndDisabledGroup();
        }
    }
}