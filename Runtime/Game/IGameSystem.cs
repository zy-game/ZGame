namespace ZGame.Game
{
    public interface IEntryGameResult : IRequest<IEntryGameResult>
    {
    }

    public interface IGameEntryOptions : IRuntimeOptions
    {
        
    }

    public interface IGameSystem : ISystem
    {
        void EntryGame(IGameEntryOptions options, IEvent<IEntryGameResult> pipeline);
    }
}