using System;

namespace ZGame.IM
{
    public interface IMChatView : IDisposable
    {
        void OnRecvieChatHandle(IMChatItem chatItem);
    }
}