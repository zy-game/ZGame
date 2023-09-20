using System;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class ani : MonoBehaviour
    {
        Animation animation;

        private void Awake()
        {
            animation = GetComponent<Animation>();
            animation.Play();
            AnimationState state = animation.PlayQueued("test1",QueueMode.PlayNow);
            Debug.Log((state == null) + " " + (animation.GetClip("test1") == null) + " --" + animation.clip.name + " ---");
        }
    }
}