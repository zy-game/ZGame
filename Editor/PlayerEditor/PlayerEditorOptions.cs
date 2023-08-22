using System;
using System.Collections.Generic;

namespace ZEngine.Editor.PlayerEditor
{
    [Config(Localtion.Project)]
    public class PlayerEditorOptions : SingleScript<PlayerEditorOptions>
    {
        public List<PlayerOptions> players;
    }

    [Serializable]
    public class PlayerOptions
    {
        public string name;
    }
}