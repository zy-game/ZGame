using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ZGame.Editor.ResBuild.Config
{
    [CreateAssetMenu(menuName = "ZGame/Create ResourceBuildRulerConfig", fileName = "ResourceBuildRulerConfig", order = 0)]
    public class ResourceBuildRulerConfig : ScriptableObject
    {
        [SerializeField] public List<RulerInfoItem> rulers;

        [OnOpenAsset()]
        public static bool OnOpened(int id, int line)
        {
            UnityEngine.Object target = UnityEditor.EditorUtility.InstanceIDToObject(id);
            if (typeof(ResourceBuildRulerConfig).IsAssignableFrom(target.GetType()))
            {
                Docker.OpenScene(target);
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public class RulerInfoItem
    {
    }
}