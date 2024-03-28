using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Resource;

namespace ZGame
{
    public static partial class Extension
    {
        public static void SetSprite(this Image image, string spritePath)
        {
            if (image == null || spritePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(spritePath);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            image.sprite = resObject.GetAsset<Sprite>(image.gameObject);
        }

        public static async UniTask SetSpriteAsync(this Image image, string spritePath)
        {
            if (image == null || spritePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(spritePath);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            image.sprite = resObject.GetAsset<Sprite>(image.gameObject);
        }

        public static void SetTexture(this RawImage rawImage, string texturePath)
        {
            if (rawImage == null || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(texturePath);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            rawImage.texture = resObject.GetAsset<Texture2D>(rawImage.gameObject);
        }

        public static async UniTask SetTextureAsync(this RawImage rawImage, string texturePath)
        {
            if (rawImage == null || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(texturePath);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            rawImage.texture = resObject.GetAsset<Texture2D>(rawImage.gameObject);
        }

        public static void SetTexture(this Material material, GameObject gameObject, string propertyName, string texturePath)
        {
            if (material == null || gameObject == null || propertyName.IsNullOrEmpty() || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(texturePath);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            material.SetTexture(propertyName, resObject.GetAsset<Texture2D>(gameObject));
        }

        public static async void SetTextureAsync(this Material material, GameObject gameObject, string propertyName, string texturePath)
        {
            if (material == null || gameObject == null || propertyName.IsNullOrEmpty() || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(texturePath);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            material.SetTexture(propertyName, resObject.GetAsset<Texture2D>(gameObject));
        }


        public static void SetMaterial(this Image gameObject, string materialPath)
        {
            if (gameObject == null)
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            Image image = gameObject.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.material = resObject.GetAsset<Material>(image.gameObject);
        }

        public static void SetMaterial(this RawImage rawImage, string materialPath)
        {
            if (rawImage == null)
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            RawImage image = rawImage.GetComponent<RawImage>();
            if (image == null)
            {
                return;
            }

            image.material = resObject.GetAsset<Material>(rawImage.gameObject);
        }

        public static async void SetMaterialAsync(this Image gameObject, string materialPath)
        {
            if (gameObject == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            Image image = gameObject.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.material = resObject.GetAsset<Material>(image.gameObject);
        }

        public static async void SetMaterialAsync(this RawImage rawImage, string materialPath)
        {
            if (rawImage == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            RawImage image = rawImage.GetComponent<RawImage>();
            if (image == null)
            {
                return;
            }

            image.material = resObject.GetAsset<Material>(rawImage.gameObject);
        }

        public static void SetMaterial(this Renderer renderer, string materialPath)
        {
            if (renderer == null)
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            renderer.material = resObject.GetAsset<Material>(renderer.gameObject);
        }

        public static async void SetMaterialAsync(this Renderer renderer, string materialPath)
        {
            if (renderer == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            renderer.material = resObject.GetAsset<Material>(renderer.gameObject);
        }

        public static void SetMaterial(this MeshRenderer meshRenderer, string materialPath)
        {
            if (meshRenderer == null)
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            meshRenderer.sharedMaterial = resObject.GetAsset<Material>(meshRenderer.gameObject);
        }

        public static async void SetMaterialAsync(this MeshRenderer meshRenderer, string materialPath)
        {
            if (meshRenderer == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            meshRenderer.sharedMaterial = resObject.GetAsset<Material>(meshRenderer.gameObject);
        }

        public static void SetMaterial(this SkinnedMeshRenderer skinnedMeshRenderer, string materialPath)
        {
            if (skinnedMeshRenderer == null)
            {
                return;
            }

            ResObject resObject = GameFrameworkEntry.Resource.LoadAsset(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            skinnedMeshRenderer.sharedMaterial = resObject.GetAsset<Material>(skinnedMeshRenderer.gameObject);
        }

        public static async void SetMaterialAsync(this SkinnedMeshRenderer skinnedMeshRenderer, string materialPath)
        {
            if (skinnedMeshRenderer == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.Resource.LoadAssetAsync(materialPath);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            skinnedMeshRenderer.sharedMaterial = resObject.GetAsset<Material>(skinnedMeshRenderer.gameObject);
        }
    }
}