namespace ZGame.Editor.ResBuild
{
    public class ResUploader : SubPageScene
    {
        public override string name { get; } = "版本管理";

        public ResUploader(Docker window) : base(window)
        {
        }
    }
}