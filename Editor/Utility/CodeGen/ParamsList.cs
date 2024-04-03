using System.Collections.Generic;
using NPBehave;

namespace ZGame.Editor.CodeGen
{
    public enum ACL
    {
        Public,
        Private,
        Internal,
        Prottcted,
        Constructor,
    }

    public class Params
    {
        public string type;
        public string name;

        public Params(string type, string name)
        {
            this.name = name;
            this.type = type;
        }
    }

    public class ParamsList
    {
        public static ParamsList None = new ParamsList();
        private List<Params> map = new();

        public ParamsList(params Params[] args)
        {
            map.AddRange(args);
        }

        public void SetParams(string type, string argsName)
        {
            if (map.Exists(x => x.name == argsName))
            {
                throw new Exception($"params name is already exist:{argsName}");
            }

            map.Add(new Params(type, argsName));
        }

        public override string ToString()
        {
            string temp = string.Empty;
            for (int i = 0; i < map.Count; i++)
            {
                temp += $"{map[i].type} {map[i].name}";
                if (i < map.Count - 1)
                {
                    temp += ", ";
                }
            }

            return temp;
        }
    }
}