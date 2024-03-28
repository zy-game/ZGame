using UnityEngine;

namespace ZGame.Game
{
    public class GameObjectComponent : IComponent
    {
        public GameObject gameObject { get; private set; }
        public Transform transform => gameObject.transform;

        public override void OnAwake(params object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return;
            }

            string assetPath = args[0] as string;
            gameObject = GameFrameworkEntry.Resource.LoadGameObjectSync(assetPath);
        }

        public override void Release()
        {
            if (gameObject == null)
            {
                return;
            }

            GameObject.DestroyImmediate(gameObject);
        }
    }
}