using System;

namespace ZGame.Resource
{
    public class BakeingReference : Attribute
    {
        public Type referenceType;

        public BakeingReference(Type type)
        {
            this.referenceType = type;
        }
    }
}