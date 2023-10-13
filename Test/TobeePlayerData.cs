using System.Collections.Generic;
using UnityEngine;
using ZEngine;
using ZEngine.Game;

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
        public void Deserialized(string body)
        {
            throw new System.NotImplementedException();
        }

        public string Serialized()
        {
            throw new System.NotImplementedException();
        }

        public void Save(string path)
        {
            throw new System.NotImplementedException();
        }

        public List<ISkillOptions> skills { get; }
    }
}