using System.Collections.Generic;
using UnityEngine;
using ZEngine;

namespace Test
{
    public class TobeePlayerData : IPlayerOptions
    {
        public void Dispose()
        {
            
        }

        public int id { get; set; }
        [OptionsName("配置名")]
        public string name { get; set; }
        public string icon { get; set; }
        public string prefab { get; set; }
        public string describe { get; set; }
        
        public List<ISkillOptions> skills { get; }
    }
}