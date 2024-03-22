namespace ZGame.Game
{
    public class WorldManager : GameFrameworkModule
    {
        public GameWorld DefaultGameWorld { get; private set; }

        public override void OnAwake()
        {
            DefaultGameWorld = new GameWorld("SYSTEM_DEFAULT_WORLD");
        }
        
        
    }
}