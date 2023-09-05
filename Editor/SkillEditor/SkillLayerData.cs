using System;
using ZEngine;

namespace Editor.SkillEditor
{
    [Serializable]
    public sealed class SkillLayerData
    {
        public int index;
        public string name;
        public Switch state;
        public int startFrameIndex;
        public int endFrameIndex;
    }
}