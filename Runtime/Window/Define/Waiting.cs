using UnityEngine;

namespace ZGame.Window
{
    [Linked("Resources/Prefabs/Wait", 997)]
    public class Waiting : GameWindow
    {
        public Waiting(GameObject gameObject) : base(gameObject)
        {
        }

        public static Waiting Create(float timeout = 0)
        {
            return default;
        }
    }
}