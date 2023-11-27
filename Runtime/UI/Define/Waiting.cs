using UnityEngine;

namespace ZGame.Window
{
    [ResourceReference("Resources/Prefabs/Wait", 997)]
    public class Waiting : UIBase
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