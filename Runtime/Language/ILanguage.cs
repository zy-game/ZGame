using UnityEngine;
using ZEngine;

namespace Runtime.Language
{
    public enum LocalizationType
    {
        Texture,
        Sprite,
        String,
    }

    class LocalizationManager : Singleton<LocalizationManager>
    {
        public ILanguageOptions Switch(int id)
        {
            return default;
        }
    }

    /// <summary>
    /// 多语言配置
    /// </summary>
    public interface ILanguageOptions : IOptions
    {
        LocalizationType type { get; }
        void SetLanguage(Transform transform);
    }
}