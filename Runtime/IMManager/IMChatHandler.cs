using System;

namespace ZGame.IM
{
    public interface IMChatHandler : IDisposable
    {
        void OnRecvieChatHandle(IMChatItem chatItem);
    }
}