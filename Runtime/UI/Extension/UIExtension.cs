using TMPro;

namespace ZGame
{
    public partial class Extension
    {
        public static void SetText(this TMP_Text text, int language)
        {
            if (text == null)
            {
                return;
            }

            text.SetText(AppCore.Language.Query(language));
        }
    }
}