using System;
using UnityEngine;

namespace ZGame.Window
{
    public class Templete<T> : UIBase where T : Templete<T>
    {
        public Templete(GameObject gameObject) : base(gameObject)
        {

        }

        public override void Enable()
        {
            this.gameObject.SetActive(true);
        }

        public override void Disable()
        {
            this.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            GameObject.DestroyImmediate(this.gameObject);
        }

        public T Instantiate(params object[] args)
        {
            GameObject temp = GameObject.Instantiate(this.gameObject);
            temp.transform.parent = this.gameObject.transform.parent;
            T templete = (T)Activator.CreateInstance(typeof(T), new object[] { temp });
            templete.Awake(args);
            return templete;
        }

        public T2 Instantiate<T2>(params object[] args) where T2 : UIForm
        {
            GameObject temp = GameObject.Instantiate(gameObject);
            temp.transform.parent = gameObject.transform.parent;
            T2 templete = (T2)Activator.CreateInstance(typeof(T2), new object[] { temp });
            templete.Awake(args);
            return templete;
        }
    }
}