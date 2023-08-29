using System;

namespace ZEngine.Network
{
    public sealed class RPCHandle : Attribute
    {
        internal Type msgType;

        public RPCHandle(Type type)
        {
            this.msgType = type;
        }
    }
}