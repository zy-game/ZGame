using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Resource;

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

            image.sprite = GameFrameworkEntry.VFS.GetSpriteSync(spritePath, image.gameObject);
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

            image.sprite = await GameFrameworkEntry.VFS.GetSpriteAsync(spritePath, image.gameObject);
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

            rawImage.texture = GameFrameworkEntry.VFS.GetTexture2DSync(texturePath, rawImage.gameObject);
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

            rawImage.texture = await GameFrameworkEntry.VFS.GetTexture2DAsync(texturePath, rawImage.gameObject);
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

            material.SetTexture(propertyName, GameFrameworkEntry.VFS.GetTexture2DSync(texturePath, gameObject));
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

            material.SetTexture(propertyName, await GameFrameworkEntry.VFS.GetTexture2DAsync(texturePath, gameObject));
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

            image.material = GameFrameworkEntry.VFS.GetMaterialSync(materialPath, image.gameObject);
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

            image.material = await GameFrameworkEntry.VFS.GetMaterialAsync(materialPath, image.gameObject);
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

            rawImage.material = GameFrameworkEntry.VFS.GetMaterialSync(materialPath, rawImage.gameObject);
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

            rawImage.material = await GameFrameworkEntry.VFS.GetMaterialAsync(materialPath, rawImage.gameObject);
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

            renderer.material = GameFrameworkEntry.VFS.GetMaterialSync(materialPath, renderer.gameObject);
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

            renderer.material = await GameFrameworkEntry.VFS.GetMaterialAsync(materialPath, renderer.gameObject);
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

            meshRenderer.sharedMaterial = GameFrameworkEntry.VFS.GetMaterialSync(materialPath, meshRenderer.gameObject);
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

            meshRenderer.sharedMaterial = await GameFrameworkEntry.VFS.GetMaterialAsync(materialPath, meshRenderer.gameObject);
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

            skinnedMeshRenderer.sharedMaterial = GameFrameworkEntry.VFS.GetMaterialSync(materialPath, skinnedMeshRenderer.gameObject);
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

            skinnedMeshRenderer.sharedMaterial = await GameFrameworkEntry.VFS.GetMaterialAsync(materialPath, skinnedMeshRenderer.gameObject);
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

            audioSource.clip = GameFrameworkEntry.VFS.GetAudioClipSync(audioClipPath, audioSource.gameObject);
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

            audioSource.clip = await GameFrameworkEntry.VFS.GetAudioClipAsync(audioClipPath, audioType, audioSource.gameObject);
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}