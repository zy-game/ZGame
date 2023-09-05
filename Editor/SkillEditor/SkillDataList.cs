using System.Collections.Generic;

namespace Editor.SkillEditor
{
    [Config(Localtion.Packaged, "Assets/Test/skill.asset")]
    public sealed class SkillDataList : SingleScript<SkillDataList>
    {
        public List<SkillOptions> optionsList;
    }
}