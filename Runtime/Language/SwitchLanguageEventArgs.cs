using ZGame.Events;

namespace ZGame.Language
{
    public sealed class SwitchLanguageEventArgs : IGameEventArgs
    {
        public LanguageDefinition language;

        public void Release()
        {
        }
    }
}