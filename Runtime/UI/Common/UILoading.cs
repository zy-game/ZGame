using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.UI
{
    public class UILoading : UIBase, IProgress<float>
    {
        private Slider slider;
        private TMP_Text progres;
        private TMP_Text title;

        public UILoading(GameObject gameObject) : base(gameObject)
        {
        }

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

        public static void SetProgress(float progress)
        {
            if (_instance is null)
            {
                Show();
            }

            _instance?.Report(progress);
        }

        public static void SetTitle(string title)
        {
            if (_instance is null)
            {
                Show();
            }

            _instance?.SetText(title);
        }

        public static UILoading Show()
        {
            if (_instance is not null)
            {
                return _instance;
            }

            string resPath = $"Resources/Loading";
            return _instance = UIManager.instance.Open<UILoading>(resPath);
        }

        public static async void Show(IUIRunning running, params object[] args)
        {
            await running.Run(Show(), args);
        }

        public static void Hide()
        {
            if (_instance is null)
            {
                return;
            }

            UIManager.instance.Inactive<UILoading>();
            _instance = null;
        }
    }


    public interface IUIRunning : IDisposable
    {
        UniTask Run(UILoading loading, params object[] args);
    }

    public interface IUIRunning<T> : IUIRunning
    {
        UniTask<T> Run(UILoading loading, params object[] args);
    }
}