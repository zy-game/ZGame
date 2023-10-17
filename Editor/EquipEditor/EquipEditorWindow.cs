using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZEngine.Editor;

namespace ZEngine.Editor.EquipEditor
{
    [Config(Localtion.Packaged, "Assets/Test/equips.json")]
    public sealed class EquipList : ConfigScriptableObject<EquipList>
    {
        public List<EquipOptions> optionsList;
    }

    [Serializable]
    public class EquipOptions
    {
        public int id;
        public string name;
        public string icon;
    }

    public class EquipEditorWindow : EngineEditorWindow
    {
        // [MenuItem("工具/编辑器/物品编辑器")]
        public static void Open()
        {
            GetWindow<EquipEditorWindow>(false, "物品编辑器", true);
        }

        protected override void OnDrawingToolbarMenu()
        {
            if (GUILayout.Button("+", EditorStyles.toolbarButton))
            {
                EquipOptions options = new EquipOptions() { name = "未命名" };
                EquipList.instance.optionsList.Add(options);
                SaveChanged();
            }
        }

        protected override MenuListItem[] GetMenuList()
        {
            return default;
        }

        protected override void Actived()
        {
            if (EquipList.instance.optionsList is null || EquipList.instance.optionsList.Count is 0)
            {
                EquipList.instance.optionsList = new List<EquipOptions>();
                return;
            }
        }

        protected override void DrawingItemDataView(object data, float width)
        {
        }

        protected override void SaveChanged()
        {
        }
    }
}