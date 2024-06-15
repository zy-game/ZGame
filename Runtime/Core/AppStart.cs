using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using ZGame;
using ZGame.Config;
using ZGame.Game;
using ZGame.Language;
using ZGame.UI;
using Screen = UnityEngine.Device.Screen;

namespace ZGame
{
    public class AppStart : MonoBehaviour
    {
        /// <summary>
        /// 是否开启日志
        /// </summary>
        public bool isDebug;

        /// <summary>
        /// 资源模式
        /// </summary>
        public ResourceMode resMode;

        public bool isFullScreen;
        public Vector2 resolution = new Vector2(1920, 1080);
        [HideInInspector] public ResourceServerOptions ossOptions;
        [HideInInspector] public GameServerOptions gameServerOptions;
        [HideInInspector] public SubGameOptions subGame;

        private void Awake()
        {
            gameObject.AddComponent<AudioListener>();
            DontDestroyOnLoad(gameObject);
            AppCore.Initialized(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnApplicationQuit();
            }
        }

        private void OnApplicationQuit()
        {
            AppCore.Quit();
        }
    }
}