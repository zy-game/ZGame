using System;

namespace ZGame
{
    public class SelectorAttribute : Attribute
    {
        public string ValuesGetter;

        public SelectorAttribute(string getter)
        {
            ValuesGetter = getter;
        }
    }
}