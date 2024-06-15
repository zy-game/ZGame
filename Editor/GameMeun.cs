using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.Editor.ExcelExprot;
using ZGame.Editor.LinkerEditor;
using ZGame.Editor.UIBind;
using ZGame.Resource;
using ZGame.UI;

namespace ZGame.Editor
{
    public class GameMeun
    {
        [MenuItem("Tools/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Tools/Clear EditorPlayerPrefs")]
        public static void ClearEditorPlayerPrefs()
        {
            EditorPrefs.DeleteAll();
        }


        [MenuItem("Assets/ZGame/Generic UIDocment")]
        public static void GenericUIBind()
        {
            if (Selection.objects is null || Selection.objects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "请选择Prefab获取Prefab所在的文件夹", "OK");
                return;
            }

            bool isUICode = EditorUtility.DisplayDialog("提示", "是否覆盖UICode?", "是", "否");

            foreach (var VARIABLE in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(VARIABLE);
                if (path.EndsWith(".prefab"))
                {
                    Generic(AssetDatabase.LoadAssetAtPath<GameObject>(path), isUICode);
                    continue;
                }

                string[] guidList = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
                if (guidList is null || guidList.Length == 0)
                {
                    continue;
                }

                for (int i = 0; i < guidList.Length; i++)
                {
                    Generic(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guidList[i])), isUICode);
                }
            }
        }

        private static void Generic(GameObject prefab, bool isUICode)
        {
            UIDocment docment = prefab.GetComponent<UIDocment>();
            using (UICodeGenerator generator = new UICodeGenerator())
            {
                UIGeneratorWriter writer = new UIGeneratorWriter();
                generator.Execute(docment, writer);
                string output = $"{AssetDatabase.GetAssetPath(docment.output)}/Base/UIBind_{docment.name}.cs";
                if (Directory.Exists(Path.GetDirectoryName(output)) is false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(output));
                }

                File.WriteAllText(output, writer.ToString());
                if (isUICode)
                {
                    output = $"{AssetDatabase.GetAssetPath(docment.output)}/Overload/UICode_{docment.name}.cs";
                    if (Directory.Exists(Path.GetDirectoryName(output)) is false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(output));
                    }

                    File.WriteAllText(output, writer.GetOverlapCode(AssetDatabase.GetAssetPath(prefab), docment));
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(docment.name + " Generid UICode Finishing");
        }

        [MenuItem("Assets/ZGame/Disable RayCast Target")]
        public static void DisableCompoRaycast()
        {
            List<GameObject> selectedObjects = Selection.gameObjects.ToList();
            if (selectedObjects.Count <= 0)
            {
                EditorUtility.DisplayDialog("错误", "请选择需要去除 RayCast Target 的物体。", "确定");
            }
            else
            {
                Debug.Log("设置 Raycast Target 操作开始！");
                List<Graphic> graphicObjs = new List<Graphic>();
                foreach (var go in selectedObjects)
                {
                    graphicObjs.Clear();
                    graphicObjs = go.GetComponentsInChildren<Graphic>(true).ToList();

                    if (graphicObjs.Count <= 0)
                    {
                        continue;
                    }

                    foreach (Graphic item in graphicObjs)
                    {
                        if (item.GetComponent<Button>() != null
                            || item.TryGetComponent<InputField>(out _)
                            || item.TryGetComponent<Button>(out _)
                            || item.TryGetComponent<Toggle>(out _)
                            || item.TryGetComponent<Slider>(out _)
                            || item.TryGetComponent<Dropdown>(out _)
                            || item.TryGetComponent<Scrollbar>(out _)
                            || item.TryGetComponent<ScrollRect>(out _)
                            || item.TryGetComponent<TMP_Dropdown>(out _)
                            || item.TryGetComponent<TMP_InputField>(out _))
                        {
                            continue;
                        }

                        item.raycastTarget = false;
                        Debug.Log($"{item.name} 对象身上的 Raycast Target 已经取消勾选...");
                    }

                    EditorUtility.SetDirty(go);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                Debug.Log("设置 Raycast Target 操作结束！");
            }
        }
    }
}