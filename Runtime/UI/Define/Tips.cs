using UnityEngine;

namespace ZGame.Window
{
    [ResourceReference("Resources/Prefabs/Toast")]
    public class Tips : UIBase
    {
        public Tips(GameObject gameObject) : base(gameObject)
        {
        }

        public static Tips Create(string tips)
        {
            return default;
        }
    }
}