using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZGame.VFS;

namespace ZGame
{
    public static partial class Extension
    {
        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="image">组件</param>
        /// <param name="spritePath">图片路径</param>
        public static void SetSprite(this Image image, string spritePath)
        {
            if (image == null || spritePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(spritePath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            image.sprite = res.GetAsset<Sprite>(image.gameObject);
        }

        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="image">组件</param>
        /// <param name="spritePath">图片路径</param>
        public static async UniTask SetSpriteAsync(this Image image, string spritePath)
        {
            if (image == null || spritePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject res = await GameFrameworkEntry.VFS.GetAssetAsync(spritePath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            image.sprite = res.GetAsset<Sprite>(image.gameObject);
        }

        /// <summary>
        /// 设置图片
        /// </summary>
        /// <param name="rawImage">组件</param>
        /// <param name="texturePath">图片路径</param>
        public static void SetTexture(this RawImage rawImage, string texturePath)
        {
            if (rawImage == null || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(texturePath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            rawImage.texture = res.GetAsset<Texture2D>(rawImage.gameObject);
        }

        /// <summary>
        /// 设置图片
        /// </summary>
        /// <param name="rawImage">组件</param>
        /// <param name="texturePath">图片路径</param>
        public static async UniTask SetTextureAsync(this RawImage rawImage, string texturePath)
        {
            if (rawImage == null || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject res = await GameFrameworkEntry.VFS.GetAssetAsync(texturePath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            rawImage.texture = res.GetAsset<Texture2D>(rawImage.gameObject);
        }

        /// <summary>
        /// 设置材质球图片
        /// </summary>
        /// <param name="material">材质球对象</param>
        /// <param name="gameObject">绑定引用对象</param>
        /// <param name="propertyName">材质球字段</param>
        /// <param name="texturePath">图片路径</param>
        public static void SetTexture(this Material material, GameObject gameObject, string propertyName, string texturePath)
        {
            if (material == null || gameObject == null || propertyName.IsNullOrEmpty() || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(texturePath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            material.SetTexture(propertyName, res.GetAsset<Texture2D>(gameObject));
        }

        /// <summary>
        /// 设置材质球图片
        /// </summary>
        /// <param name="material">材质球对象</param>
        /// <param name="gameObject">绑定引用对象</param>
        /// <param name="propertyName">材质球字段</param>
        /// <param name="texturePath">图片路径</param>
        public static async void SetTextureAsync(this Material material, GameObject gameObject, string propertyName, string texturePath)
        {
            if (material == null || gameObject == null || propertyName.IsNullOrEmpty() || texturePath.IsNullOrEmpty())
            {
                return;
            }

            ResObject res = await GameFrameworkEntry.VFS.GetAssetAsync(texturePath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            material.SetTexture(propertyName, res.GetAsset<Texture2D>(gameObject));
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="image">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static void SetMaterial(this Image image, string materialPath)
        {
            if (image == null)
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(materialPath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            image.material = res.GetAsset<Material>(image.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="image">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static async void SetMaterialAsync(this Image image, string materialPath)
        {
            if (image == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.VFS.GetAssetAsync(materialPath);
            if (resObject == null || resObject.IsSuccess() is false)
            {
                return;
            }

            image.material = resObject.GetAsset<Material>(image.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="rawImage">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static void SetMaterial(this RawImage rawImage, string materialPath)
        {
            if (rawImage == null)
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(materialPath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            rawImage.material = res.GetAsset<Material>(rawImage.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="rawImage">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static async void SetMaterialAsync(this RawImage rawImage, string materialPath)
        {
            if (rawImage == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.VFS.GetAssetAsync(materialPath);
            if (resObject == null || resObject.IsSuccess() is false)
            {
                return;
            }

            rawImage.material = resObject.GetAsset<Material>(rawImage.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="renderer">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static void SetMaterial(this Renderer renderer, string materialPath)
        {
            if (renderer == null)
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(materialPath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            renderer.material = res.GetAsset<Material>(renderer.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="renderer">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static async void SetMaterialAsync(this Renderer renderer, string materialPath)
        {
            if (renderer == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.VFS.GetAssetAsync(materialPath);
            if (resObject == null || resObject.IsSuccess() is false)
            {
                return;
            }

            renderer.material = resObject.GetAsset<Material>(renderer.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="meshRenderer">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static void SetMaterial(this MeshRenderer meshRenderer, string materialPath)
        {
            if (meshRenderer == null)
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(materialPath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            meshRenderer.material = res.GetAsset<Material>(meshRenderer.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="meshRenderer">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static async void SetMaterialAsync(this MeshRenderer meshRenderer, string materialPath)
        {
            if (meshRenderer == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.VFS.GetAssetAsync(materialPath);
            if (resObject == null || resObject.IsSuccess() is false)
            {
                return;
            }

            meshRenderer.material = resObject.GetAsset<Material>(meshRenderer.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="skinnedMeshRenderer">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static void SetMaterial(this SkinnedMeshRenderer skinnedMeshRenderer, string materialPath)
        {
            if (skinnedMeshRenderer == null)
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(materialPath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            skinnedMeshRenderer.material = res.GetAsset<Material>(skinnedMeshRenderer.gameObject);
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="skinnedMeshRenderer">渲染组件</param>
        /// <param name="materialPath">材质球路径</param>
        public static async void SetMaterialAsync(this SkinnedMeshRenderer skinnedMeshRenderer, string materialPath)
        {
            if (skinnedMeshRenderer == null)
            {
                return;
            }

            ResObject resObject = await GameFrameworkEntry.VFS.GetAssetAsync(materialPath);
            if (resObject == null || resObject.IsSuccess() is false)
            {
                return;
            }

            skinnedMeshRenderer.material = resObject.GetAsset<Material>(skinnedMeshRenderer.gameObject);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="audioSource">音效播放组件</param>
        /// <param name="audioClipPath">音效路径</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="volume">播放音效的音量</param>
        public static void OnPlayAudioClip(this AudioSource audioSource, string audioClipPath, bool loop = false, float volume = 1f)
        {
            if (audioSource == null)
            {
                return;
            }

            ResObject res = GameFrameworkEntry.VFS.GetAsset(audioClipPath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            audioSource.clip = res.GetAsset<AudioClip>(audioSource.gameObject);
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.Play();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="audioSource">音效播放组件</param>
        /// <param name="audioType">音效编码格式</param>
        /// <param name="audioClipPath">音效路径</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="volume">播放音效的音量</param>
        public static async void OnPlayAudioClipAsync(this AudioSource audioSource, AudioType audioType, string audioClipPath, bool loop = false, float volume = 1f)
        {
            if (audioSource == null)
            {
                return;
            }

            ResObject res = await GameFrameworkEntry.VFS.GetAssetAsync(audioClipPath);
            if (res == null || res.IsSuccess() is false)
            {
                return;
            }

            audioSource.clip = res.GetAsset<AudioClip>(audioSource.gameObject);
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}