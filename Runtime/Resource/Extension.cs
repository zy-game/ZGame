﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ZGame.Resource
{
    public static class Extension
    {
        public static GameObject Instantiate(this ResObject resObject)
        {
            if (resObject is null)
            {
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.Require<GameObject>());
            GameObjectDestoryCallback.Create(gameObject, () => { CoreApi.Resource.Release(resObject); });
            return gameObject;
        }

        public static GameObject Instantiate(this ResObject loadResourceObjectResult, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = loadResourceObjectResult.Instantiate();
            if (gameObject != null)
            {
                if (parent != null)
                {
                    gameObject.transform.SetParent(parent.transform);
                }

                gameObject.transform.position = pos;
                gameObject.transform.rotation = Quaternion.Euler(rot);
                gameObject.transform.localScale = scale;
            }

            return gameObject;
        }

        public static void SetSprite(this ResObject resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            Image image = gameObject.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.sprite = resObject.Require<Sprite>();
            GameObjectDestoryCallback.Create(gameObject, () => { CoreApi.Resource.Release(resObject); });
        }

        public static void SetRawImage(this ResObject resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            RawImage image = gameObject.GetComponent<RawImage>();
            if (image == null)
            {
                return;
            }

            image.texture = resObject.Require<Texture2D>();
            GameObjectDestoryCallback.Create(gameObject, () => { CoreApi.Resource.Release(resObject); });
        }

        public static void SetSound(this ResObject resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            AudioSource component = gameObject.GetComponent<AudioSource>();
            if (component == null)
            {
                return;
            }

            component.clip = resObject.Require<AudioClip>();
            GameObjectDestoryCallback.Create(gameObject, () => { CoreApi.Resource.Release(resObject); });
        }

        public static void SetVideoClip(this ResObject resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            VideoPlayer component = gameObject.GetComponent<VideoPlayer>();
            if (component == null)
            {
                return;
            }

            component.clip = resObject.Require<VideoClip>();
            GameObjectDestoryCallback.Create(gameObject, () => { CoreApi.Resource.Release(resObject); });
        }
    }
}