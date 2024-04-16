using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.UI
{
    [RefPath("Resources/Loading")]
    [UIOptions(UILayer.Notification, SceneType.Overlap, CacheType.Permanent)]
    public class UILoading : UIBase, IProgress<float>
    {
        private Slider slider;
        private TMP_Text progres;
        private TMP_Text title;


        public override void Awake()
        {
            slider = this.gameObject.GetComponentInChildren<Slider>();
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("content"))
                {
                    title = VARIABLE;
                }

                if (slider == null && VARIABLE.name.Equals("progres"))
                {
                    progres = VARIABLE;
                }
            }
        }

        public void Report(float value)
        {
            progres?.SetText(Mathf.FloorToInt(value * 100).ToString() + "%");
            slider?.SetValueWithoutNotify(value);
        }

        public void SetText(string title)
        {
            this.title?.SetText(title);
        }


        private static UILoading _instance;


        /// <summary>
        /// 设置加载界面信息，如果加载界面未加载则会加载UI并显示
        /// </summary>
        /// <param name="progress"></param>
        public static void SetProgress(float progress)
        {
            if (_instance is null)
            {
                Show();
            }

            _instance?.Report(progress);
        }

        /// <summary>
        /// 设置加载界面信息，如果加载界面未加载则会加载UI并显示
        /// </summary>
        /// <param name="title"></param>
        public static void SetTitle(string title)
        {
            if (_instance is null)
            {
                Show();
            }

            _instance?.SetText(title);
        }

        /// <summary>
        /// 显示加载界面
        /// </summary>
        /// <returns></returns>
        public static UILoading Show()
        {
            if (_instance is not null)
            {
                return _instance;
            }

            return _instance = ZG.UI.Active<UILoading>();
        }

        /// <summary>
        /// 隐藏加载界面
        /// </summary>
        public static void Hide()
        {
            if (_instance is null)
            {
                return;
            }

            ZG.UI.Inactive<UILoading>();
            _instance = null;
        }
    }
}