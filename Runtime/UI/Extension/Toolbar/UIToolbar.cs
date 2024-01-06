using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    public class UIToolbar : MonoBehaviour
    {
        public SubScene current;

        public List<Button> buttons;

        public List<SubScene> scenes;

        public void OnEnable()
        {
            if (current == null)
            {
                return;
            }

            Switch(current.sceneName);
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