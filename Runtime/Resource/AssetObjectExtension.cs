using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using ZEngine.Resource;

namespace ZEngine
{
    public static class AssetObjectExtension
    {
         public static GameObject Instantiate(this IRequestAssetObjectResult requestAssetObjectResult)
    {
        return requestAssetObjectResult.Instantiate(null, Vector3.zero, Vector3.zero, Vector3.zero);
    }

    public static GameObject Instantiate(this IRequestAssetObjectResult requestAssetObjectResult, GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (requestAssetObjectResult == null || requestAssetObjectResult.result == null || typeof(GameObject) != requestAssetObjectResult.result.GetType())
        {
            return default;
        }

        GameObject gameObject = GameObject.Instantiate(requestAssetObjectResult.result) as GameObject;
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
        Object temp = requestAssetObjectResult.result;
        gameObject.OnDestroyEvent(() => { ZGame.Resource.Release(temp); });
        return gameObject;
    }

    public static T GetObject<T>(this IRequestAssetObjectResult requestAssetObjectResult) where T : Object
    {
        if (requestAssetObjectResult == null || requestAssetObjectResult.result == null || typeof(T) != requestAssetObjectResult.result.GetType())
        {
            return default;
        }

        return (T)requestAssetObjectResult.result;
    }

    public static void SetAssetObject<T>(this IRequestAssetObjectResult requestAssetObjectResult, GameObject gameObject) where T : Component
    {
        if (requestAssetObjectResult == null || requestAssetObjectResult.result || requestAssetObjectResult.status is not Status.Success)
        {
            return;
        }

        T component = gameObject.GetComponent<T>();
        switch (component)
        {
            case AudioSource audioSource:
                audioSource.clip = requestAssetObjectResult.GetObject<AudioClip>();
                break;
            case Image spriteImage:
                spriteImage.sprite = requestAssetObjectResult.GetObject<Sprite>();
                break;
            case RawImage rawImage:
                rawImage.texture = requestAssetObjectResult.GetObject<Texture2D>();
                break;
            case VideoPlayer videoPlayer:
                videoPlayer.clip = requestAssetObjectResult.GetObject<VideoClip>();
                break;
        }

        Object temp = requestAssetObjectResult.result;
        gameObject.OnDestroyEvent(() => { ZGame.Resource.Release(temp); });
    }
    }
}