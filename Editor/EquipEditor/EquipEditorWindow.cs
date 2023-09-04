using System;
using System.Collections.Generic;
using UnityEditor;
using ZEngine.Editor;

namespace Editor.EquipEditor
{
    [Config(Localtion.Packaged, "Assets/Test/equips.json")]
    public sealed class EquipList : SingleScript<EquipList>
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

    public class EquipEditorWindow : EngineCustomEditor
    {
        [MenuItem("Game/Equip Editor")]
        public static void Open()
        {
            GetWindow<EquipEditorWindow>(false, "物品编辑器", true);
        }

        protected override void Actived()
        {
            if (EquipList.instance.optionsList is null || EquipList.instance.optionsList.Count is 0)
            {
                EquipList.instance.optionsList = new List<EquipOptions>();
                return;
            }

            foreach (var VARIABLE in EquipList.instance.optionsList)
            {
                AddDataItem(VARIABLE.name, VARIABLE);
            }
        }

        protected override void CreateNewItem()
        {
            EquipOptions options = new EquipOptions() { name = "未命名" };
            EquipList.instance.optionsList.Add(options);
            AddDataItem(options.name, options);
            SaveChanged();
        }

        protected override void DrawingItemDataView(object data, float width)
        {
        }

        protected override void SaveChanged()
        {
        }
    }
}