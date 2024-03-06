using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame;
using ZGame.Config;

namespace ZGame.UI
{
    [ResourceReference("Resources/MsgBox")]
    [UIOptions(UILayer.Notification, SceneType.Addition, CacheType.Permanent)]
    public class UIMsgBox : UIBase
    {
        private Action onYes;
        private Action onNo;

        public UIMsgBox(GameObject gameObject) : base(gameObject)
        {
        }

        public override void Enable(params object[] args)
        {
            base.Enable(args);
            this.onYes = (Action)(args[2]);
            this.onNo = (Action)(args[3]);

            BehaviourScriptable.instance.SetupKeyDownEvent(KeyCode.Escape, OnBackup);
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>(true);
            foreach (var VARIABLE in texts)
            {
                if (VARIABLE.name.Equals("title"))
                {
                    VARIABLE.SetText(args[0].ToString());
                }

                if (VARIABLE.name.Equals("content"))
                {
                    VARIABLE.SetText(args[1].ToString());
                }

                if (VARIABLE.name.Equals("text_yes"))
                {
                    VARIABLE.SetText(WorkApi.Language.Query("确定"));
                }

                if (VARIABLE.name.Equals("text_no"))
                {
                    VARIABLE.SetText(WorkApi.Language.Query("取消"));
                }
            }

            Button[] buttons = this.gameObject.GetComponentsInChildren<Button>(true);
            foreach (var VARIABLE in buttons)
            {
                if (VARIABLE.name.Equals("btn_yes"))
                {
                    VARIABLE.onClick.AddListener(() => Switch(true));
                }

                if (VARIABLE.name.Equals("btn_no"))
                {
                    VARIABLE.onClick.AddListener(() => Switch(false));
                }
            }
        }

        private void OnBackup(KeyEventData e)
        {
            e.Use();
            Switch(false);
        }

        private void Switch(bool state)
        {
            BehaviourScriptable.instance.UnsetupKeyDownEvent(KeyCode.Escape, OnBackup);
            WorkApi.UI.Inactive(this);
            switch (state)
            {
                case true:
                    this.onYes?.Invoke();
                    break;
                case false:
                    this.onNo?.Invoke();
                    break;
            }
        }

        public static void Show(string title, string content, Action onYes, Action onNo)
        {
            WorkApi.UI.Active<UIMsgBox>(new object[] { title, content, onYes, onNo });
        }

        public static void Show(string content, Action onYes, Action onNo)
        {
            Show(WorkApi.Language.Query("提示"), content, onYes, onNo);
        }

        public static void Show(string content, Action onYes)
        {
            Show(WorkApi.Language.Query("提示"), content, onYes, null);
        }

        public static void Show(string content)
        {
            Show(WorkApi.Language.Query("提示"), content, null, null);
        }
    }
}