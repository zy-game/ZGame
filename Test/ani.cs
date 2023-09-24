using System;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.Game;

namespace Test
{
    public class ani : MonoBehaviour
    {
        Animation animation;

        private void Awake()
        {
            animation = GetComponent<Animation>();
            animation.Play();
            AnimationState state = animation.PlayQueued("test1", QueueMode.PlayNow);
            Debug.Log((state == null) + " " + (animation.GetClip("test1") == null) + " --" + animation.clip.name + " ---");
        }
    }

    public class DefaultPlayerOptions : IPlayerOptions
    {
        public int id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string prefab { get; set; }
    }
}