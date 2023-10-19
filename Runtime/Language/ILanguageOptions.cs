using UnityEngine;
using ZEngine;

namespace Runtime.Language
{
    /// <summary>
    /// 多语言配置
    /// </summary>
    public interface ILanguageOptions : IOptions
    {
        LanguageType type { get; }
        void SetLanguage(Transform transform);

        public static ILanguageOptions Create(int id, string text, LanguageType type)
        {
            return default;
        }
    }
}