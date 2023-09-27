using System;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源数据
    /// </summary>
    [Serializable]
    public sealed class GameAssetObjectManifest
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string name;

        /// <summary>
        /// 文件GUID
        /// </summary>
        public string guid;

        /// <summary>
        /// 文件路径
        /// Assets/.../.../
        /// </summary>
        public string path;

        public static GameAssetObjectManifest Create(string name, string path, string guid)
        {
            GameAssetObjectManifest gameAssetObjectManifest = new GameAssetObjectManifest();
            gameAssetObjectManifest.name = name;
            gameAssetObjectManifest.guid = guid;
            gameAssetObjectManifest.path = path;
            return gameAssetObjectManifest;
        }
    }
}