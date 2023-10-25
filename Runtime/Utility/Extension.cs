using System;

namespace ZGame
{
    public static class Extension
    {
        public static Type GetTypeForThat(this AppDomain domain, string name)
        {
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                Type type = VARIABLE.GetType(name);
                if (type is null)
                {
                    continue;
                }

                return type;
            }

            return default;
        }
        
    }
}