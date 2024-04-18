using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame;
using ZGame.Config;
using ZGame.Game;
using ZGame.UI;

public class GameFrameworkStartup : MonoBehaviour
{
    private void Awake()
    {
        this.gameObject.AddComponent<AudioListener>();
        GameObject.DontDestroyOnLoad(this.gameObject);
        CoreAPI.Initialized();
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}