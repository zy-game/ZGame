﻿using System.Collections.Generic;

namespace ZEngine.Editor.SkillEditor
{
    [Config(Localtion.Packaged, "Assets/Test/skill.asset")]
    public sealed class SkillDataList : ConfigScriptableObject<SkillDataList>
    {
        public List<SkillOptions> optionsList;
    }
}