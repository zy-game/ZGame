using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using ZEngine.Resource;

namespace ZEngine
{
    public static partial class Extension
    {
        public static GameObject Instantiate(this IRequestResourceObjectResult requestResourceObjectResult)
        {
            return requestResourceObjectResult.Instantiate(null, Vector3.zero, Vector3.zero, Vector3.zero);
        }

        public static GameObject Instantiate(this IRequestResourceObjectResult requestResourceObjectResult, GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (requestResourceObjectResult == null || requestResourceObjectResult.result == null || typeof(GameObject) != requestResourceObjectResult.result.GetType())
            {
                return default;
            }

            GameObject gameObject = GameObject.Instantiate(requestResourceObjectResult.result) as GameObject;
            if (gameObject == null)
            {
                return default;
            }

            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }

            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
            Object temp = requestResourceObjectResult.result;
            gameObject.OnDestroyEvent(() => { ZGame.Resource.Release(temp); });
            return gameObject;
        }

        public static T GetObject<T>(this IRequestResourceObjectResult requestResourceObjectResult) where T : Object
        {
            if (requestResourceObjectResult == null || requestResourceObjectResult.result == null || typeof(T) != requestResourceObjectResult.result.GetType())
            {
                return default;
            }

            return (T)requestResourceObjectResult.result;
        }

        public static void SetAssetObject<T>(this IRequestResourceObjectResult requestResourceObjectResult, GameObject gameObject) where T : Component
        {
            if (requestResourceObjectResult == null || requestResourceObjectResult.result || requestResourceObjectResult.status is not Status.Success)
            {
                return;
            }

            T component = gameObject.GetComponent<T>();
            switch (component)
            {
                case AudioSource audioSource:
                    audioSource.clip = requestResourceObjectResult.GetObject<AudioClip>();
                    break;
                case Image spriteImage:
                    spriteImage.sprite = requestResourceObjectResult.GetObject<Sprite>();
                    break;
                case RawImage rawImage:
                    rawImage.texture = requestResourceObjectResult.GetObject<Texture2D>();
                    break;
                case VideoPlayer videoPlayer:
                    videoPlayer.clip = requestResourceObjectResult.GetObject<VideoClip>();
                    break;
            }

            Object temp = requestResourceObjectResult.result;
            gameObject.OnDestroyEvent(() => { ZGame.Resource.Release(temp); });
        }
    }
}