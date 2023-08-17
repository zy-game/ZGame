using System;

namespace ZEngine.Network
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RPCOptions : Attribute
    {
        internal Type messageType;

        public RPCOptions(Type messageType)
        {
            this.messageType = messageType;
        }
    }
}