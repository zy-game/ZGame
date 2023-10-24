namespace ZGame.Localization
{
    public enum Language : byte
    {
        ZH,
        EN,
    }

    public enum LanguageType : byte
    {
        Text,
        Texture,
    }

    public interface ILanguageOptions : IOptions
    {
        string value { get; }
        LanguageType type { get; }
    }

    public interface ILocalizationSystem : ISystem
    {
        ILanguageOptions GetLanguage(int guid);
        void SwitchLanguage(Language language);
    }
}