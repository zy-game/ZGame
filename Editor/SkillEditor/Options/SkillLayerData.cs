using System;
using ZEngine;

namespace Editor.SkillEditor
{
    [Serializable]
    public class SkillLayerData
    {
        public int index;
        public string name;
        public Switch state;
        public LayerType type;
        public int startFrameIndex;
        public int endFrameIndex;
    }

    public enum LayerType : byte
    {
        
    }
}