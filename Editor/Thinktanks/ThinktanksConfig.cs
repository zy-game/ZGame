namespace ZGame.Editor.Helpme
{
    [ResourceReference("Assets/Settings/HelpConfig.asset")]
    public class ThinktanksConfig : BaseConfig<ThinktanksConfig>
    {
        public string userName;
        public string userPassword;

        public string address;

        public override void OnAwake()
        {
        }
    }
}