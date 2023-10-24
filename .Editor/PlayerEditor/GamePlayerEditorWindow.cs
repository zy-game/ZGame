using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZEngine.Game;

namespace ZEngine.Editor.PlayerEditor
{
    public class GamePlayerEditorWindow : EngineEditorWindow
    {
        private int seleteType;
        private string[] typeList;
        private List<IPlayerOptions> options;


        protected override void Actived()
        {
            options = IOptions.DeserializeFileData<List<IPlayerOptions>>(Application.dataPath + "/../UserSettings/players.ini"); // new List<IPlayerOptions>();
            // string config_path = Application.dataPath + "/../UserSettings/players.ini";
            // if (File.Exists(config_path) is false)
            // {
            //     return;
            // }
            //
            // options = IOptions.Deserialize<List<IPlayerOptions>>(File.ReadAllText(config_path));
        }

        private List<Type> GetAllPlayerTypeName()
        {
            List<Type> temp = new List<Type>();
            foreach (var VARIABLE in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in VARIABLE.GetTypes())
                {
                    if (typeof(IPlayerOptions).IsAssignableFrom(type) && type.IsInterface == false && type.IsAbstract == false)
                    {
                        temp.Add(type);
                    }
                }
            }

            return temp;
        }

        protected override void SaveChanged()
        {
            // string config_path = Application.dataPath + "/../UserSettings/players.ini";
            // File.WriteAllText(config_path, IOptions.Serialize(options));
            IOptions.SerializeToFile(options, Application.dataPath + "/../UserSettings/players.ini");
        }

        protected override void OnDrawingToolbarMenu()
        {
            // seleteType = EditorGUILayout.Popup(seleteType, typeList, EditorStyles.toolbarPopup);
            if (GUILayout.Button("+", EditorStyles.toolbarPopup))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var optionsType in GetAllPlayerTypeName())
                {
                    menu.AddItem(new GUIContent(optionsType.Name), false, () =>
                    {
                        MethodInfo methodInfo = optionsType.GetMethod("Create", BindingFlags.Static);
                        IPlayerOptions playerOptions = default;
                        if (methodInfo is not null)
                        {
                            playerOptions = (IPlayerOptions)methodInfo.Invoke(null, new object[] { });
                        }
                        else
                        {
                            playerOptions = (IPlayerOptions)Activator.CreateInstance(optionsType);
                        }

                        options.Add(playerOptions);
                        SaveChanged();
                    });
                }

                menu.ShowAsContext();
            }
        }

        protected override MenuListItem[] GetMenuList()
        {
            MenuListItem[] items = new MenuListItem[options.Count];
            for (int i = 0; i < options.Count; i++)
            {
                items[i] = new MenuListItem();
                items[i].name = options[i].name;
                items[i].data = options[i];
                items[i].icon = options[i].icon;
            }

            return items;
        }

        // protected override void DrawingItemDataView(object data, float width)
        // {
        //     DrawingProperties(data);
        //     PropertyInfo[] propertyInfos = data.GetType().GetProperties();
        //     foreach (var VARIABLE in propertyInfos)
        //     {
        //         OptionsName header = VARIABLE.GetCustomAttribute<OptionsName>();
        //         string fileName = header == null ? VARIABLE.Name : header.name;
        //         object value = VARIABLE.GetValue(data);
        //     
        //         if (VARIABLE.PropertyType == typeof(Int32))
        //         {
        //             int m = EditorGUILayout.IntField(fileName, (int)value);
        //             if (m.Equals(value) is false)
        //             {
        //                 VARIABLE.SetValue(data, m);
        //             }
        //         }
        //     
        //         if (VARIABLE.PropertyType == typeof(float))
        //         {
        //             float m = EditorGUILayout.FloatField(fileName, (float)VARIABLE.GetValue(data));
        //             if (m.Equals(value) is false)
        //             {
        //                 VARIABLE.SetValue(data, m);
        //             }
        //         }
        //     
        //         if (VARIABLE.PropertyType == typeof(string))
        //         {
        //             string m = EditorGUILayout.TextField(fileName, (string)VARIABLE.GetValue(data));
        //             if (m?.Equals(value) is false)
        //             {
        //                 VARIABLE.SetValue(data, m);
        //             }
        //         }
        //     }
        // }
    }
}