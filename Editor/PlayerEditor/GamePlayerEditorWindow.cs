using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZEngine.Editor.PlayerEditor
{
    public class GamePlayerEditorWindow : EngineEditorWindow
    {
        private int seleteType;
        private string[] typeList;
        private List<IPlayerOptions> options;


        protected override void Actived()
        {
            string config_path = Application.dataPath + "/../UserSettings/players.ini";
            List<string> temp = new List<string>();
            foreach (var VARIABLE in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in VARIABLE.GetTypes())
                {
                    if (typeof(IPlayerOptions).IsAssignableFrom(type) && type.IsInterface == false && type.IsAbstract == false)
                    {
                        temp.Add(type.FullName);
                    }
                }
            }

            typeList = temp.ToArray();
            options = new List<IPlayerOptions>();
            if (File.Exists(config_path) is false)
            {
                return;
            }


            List<dynamic> cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText(config_path));
            foreach (var VARIABLE in cfg)
            {
                string tyName = VARIABLE.type.ToString();
                Type t = AppDomain.CurrentDomain.FindType(tyName);
                if (t is null)
                {
                    continue;
                }

                options.Add(Newtonsoft.Json.JsonConvert.DeserializeObject(VARIABLE.data.ToString(), t));
            }
        }

        protected override void SaveChanged()
        {
            string config_path = Application.dataPath + "/../UserSettings/players.ini";
            List<dynamic> cfg = new List<dynamic>();
            foreach (var VARIABLE in options)
            {
                cfg.Add(new
                {
                    type = VARIABLE.GetType().FullName,
                    data = Newtonsoft.Json.JsonConvert.SerializeObject(VARIABLE)
                });
            }

            File.WriteAllText(config_path, Newtonsoft.Json.JsonConvert.SerializeObject(cfg));
        }

        protected override void OnDrawingToolbarMenu()
        {
            seleteType = EditorGUILayout.Popup(seleteType, typeList, EditorStyles.toolbarPopup);
        }

        protected override void CreateNewItem()
        {
            IPlayerOptions playerOptions = (IPlayerOptions)Activator.CreateInstance(AppDomain.CurrentDomain.FindType(typeList[seleteType]));
            playerOptions.id = 10000000 + options.Count;
            playerOptions.name = "未命名" + options.Count;
            options.Add(playerOptions);
            SaveChanged();
        }

        protected override MenuListItem[] GetMenuList()
        {
            MenuListItem[] items = new MenuListItem[options.Count];
            for (int i = 0; i < options.Count; i++)
            {
                items[i] = new MenuListItem();
                items[i].index = i;
                items[i].name = options[i].name;
                items[i].data = options[i];
                items[i].icon = options[i].icon;
            }

            return items;
        }

        protected override void DrawingItemDataView(object data, float width)
        {
            PropertyInfo[] propertyInfos = data.GetType().GetProperties();
            foreach (var VARIABLE in propertyInfos)
            {
                OptionsName header = VARIABLE.GetCustomAttribute<OptionsName>();
                string fileName = header == null ? VARIABLE.Name : header.name;
                object value = VARIABLE.GetValue(data);

                if (VARIABLE.PropertyType == typeof(Int32))
                {
                    int m = EditorGUILayout.IntField(fileName, (int)value);
                    if (m.Equals(value) is false)
                    {
                        VARIABLE.SetValue(data, m);
                    }
                }

                if (VARIABLE.PropertyType == typeof(float))
                {
                    float m = EditorGUILayout.FloatField(fileName, (float)VARIABLE.GetValue(data));
                    if (m.Equals(value) is false)
                    {
                        VARIABLE.SetValue(data, m);
                    }
                }

                if (VARIABLE.PropertyType == typeof(string))
                {
                    string m = EditorGUILayout.TextField(fileName, (string)VARIABLE.GetValue(data));
                    if (m?.Equals(value) is false)
                    {
                        VARIABLE.SetValue(data, m);
                    }
                }
            }
        }
    }
}