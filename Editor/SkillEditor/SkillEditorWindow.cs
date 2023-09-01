using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZEngine.Editor;
using ZEngine.Game;

namespace Editor.SkillEditor
{
    [Config(Localtion.Packaged, "Assets/Test/skill.json")]
    public sealed class SkillDataList : SingleScript<SkillDataList>
    {
        public List<SkillOptions> optionsList;
    }


    public class SkillEditorWindow : EngineCustomEditor
    {
        [MenuItem("Game/SkillEditor")]
        public static void Open()
        {
            GetWindow<SkillEditorWindow>(false, "技能编辑器", true);
        }

        protected override void Actived()
        {
            if (SkillDataList.instance.optionsList is null || SkillDataList.instance.optionsList.Count is 0)
            {
                SkillDataList.instance.optionsList = new List<SkillOptions>();
                return;
            }

            foreach (var VARIABLE in SkillDataList.instance.optionsList)
            {
                AddDataItem(VARIABLE.name, VARIABLE);
            }
        }

        protected override void CreateNewItem()
        {
            SkillOptions options = new SkillOptions() { name = "未命名" };
            SkillDataList.instance.optionsList.Add(options);
            AddDataItem(options.name, options);
            SaveChanged();
        }

        protected override void DrawingItemDataView(object data)
        {
            SkillOptions options = (SkillOptions)data;
            options.id = EditorGUILayout.IntField("技能编号", options.id);
            options.name = EditorGUILayout.TextField("技能名称", options.name);
            if (options._icon == null && options.icon.IsNullOrEmpty() is false)
            {
                options._icon = AssetDatabase.LoadAssetAtPath<Texture2D>(options.icon);
            }

            options._icon = (Texture2D)EditorGUILayout.ObjectField("角色头像", options._icon, typeof(Texture2D), false);
            if (options._icon != null)
            {
                options.icon = AssetDatabase.GetAssetPath(options._icon);
            }
        }

        protected override void SaveChanged()
        {
            SkillDataList.instance.Saved();
        }
    }
}