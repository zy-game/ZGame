using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.UI
{
    public class UIToolbar : MonoBehaviour
    {
        public SubSceneTemplete current;
        public List<SubSceneTemplete> scenes;

        public void OnEnable()
        {
            if (current == null)
            {
                return;
            }

            Switch(current.sceneName);
        }

        public void AddScene(SubSceneTemplete scene)
        {
            scenes.Add(scene);
        }

        public void Switch(string sceneName)
        {
            if (scenes == null)
            {
                return;
            }

            foreach (var item in scenes)
            {
                item.gameObject.SetActive(item.sceneName == sceneName);
            }
        }
    }
}