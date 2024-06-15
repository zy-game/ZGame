using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    public class TransformComponent : IComponent
    {
        public uint id { get; set; }
        public GameObject gameObject { get; set; }
        public Transform transform => gameObject.transform;

        /// <summary>
        /// 物体世界坐标
        /// </summary>
        public Vector3 position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        /// <summary>
        /// 物体局部坐标
        /// </summary>
        public Vector3 localPosition
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        /// <summary>
        /// 物体基于世界坐标的旋转
        /// </summary>
        public Quaternion rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }

        /// <summary>
        /// 物体基于局部坐标的旋转
        /// </summary>
        public Quaternion localRotation
        {
            get { return transform.localRotation; }
            set { transform.localRotation = value; }
        }

        /// <summary>
        /// 物体缩放
        /// </summary>
        public Vector3 localScale
        {
            get { return transform.localScale; }
            set { transform.localScale = value; }
        }

        public void Release()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}