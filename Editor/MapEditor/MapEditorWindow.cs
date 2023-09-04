using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace ZEngine.Editor.MapEditor
{
    public class MapEditorWindow : EngineCustomEditor
    {
        [MenuItem("Game/Map Create")]
        public static void Open()
        {
            GetWindow<MapEditorWindow>(false, "Map Create", true);
        }

        protected override void Actived()
        {
            if (MapOptions.instance.sceneList is null || MapOptions.instance.sceneList.Count is 0)
            {
                MapOptions.instance.sceneList = new List<SceneOptions>();
                return;
            }

            foreach (var VARIABLE in MapOptions.instance.sceneList)
            {
                AddDataItem(VARIABLE.name, VARIABLE);
            }
        }

        protected override void CreateNewItem()
        {
            SceneOptions options = new SceneOptions() { name = "未命名" };
            MapOptions.instance.sceneList.Add(options);
            AddDataItem(options.name, options);
            SaveChanged();
        }

        protected override void DrawingItemDataView(object data, float width)
        {
            SceneOptions options = (SceneOptions)data;
            GUILayout.Label(options.name);
        }

        protected override void SaveChanged()
        {
            MapOptions.instance.Saved();
        }
    }
}