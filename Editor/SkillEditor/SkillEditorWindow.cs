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
        [MenuItem("Game/ZJson")]
        public static void TestZJson()
        {
            Engine.Json.ToJson(SkillDataList.instance.optionsList);
        }

        [MenuItem("Game/SkillEditor")]
        public static void Open()
        {
            GetWindow<SkillEditorWindow>();
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
            GUILayout.Label(options.name);
        }

        protected override void SaveChanged()
        {
            SkillDataList.instance.Saved();
        }
    }
}