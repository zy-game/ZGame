using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public abstract class ScriptableObjectEditorWindow<T> : SubEditorSceneWindow where T : ScriptableObject
    {
        protected T target { get; private set; }
        public override string name => target?.name;


        public ScriptableObjectEditorWindow(T data)
        {
            target = data;
        }

        public override void SetTitleName(string name)
        {
            this.target.name = name;
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(target), name);
            OnSave();
        }

        public void OnDelete()
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(target));
            AssetDatabase.Refresh();
            target = null;
            EditorWindow.GetWindow<GameEditorWindow>().Repaint();
            EditorWindow.GetWindow<GameEditorWindow>().OnEnable();
        }

        public override void OnSave()
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        protected List<T> FindAllScriptableObject<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T)}").Select(x => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(x))).ToList();
        }
    }
}