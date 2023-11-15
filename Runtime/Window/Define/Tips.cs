using UnityEngine;

namespace ZGame.Window
{
    [Linked("Resources/Prefabs/Toast",998)]
    public class Tips : GameWindow
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