using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZGame;
using ZGame.Window;

namespace UI
{
    public class UIMsgBox : UIBase
    {
        private Action onYes;
        private Action onNo;

        public UIMsgBox(GameObject gameObject) : base(gameObject)
        {
        }

        public override void Enable(params object[] args)
        {
            this.onYes = (Action)(args[2]);
            this.onNo = (Action)(args[3]);
            BehaviourScriptable.instance.SetupKeyDown(KeyCode.Escape, OnBackup);
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
            }

            Button[] buttons = this.gameObject.GetComponentsInChildren<Button>(true);
            foreach (var VARIABLE in buttons)
            {
                if (VARIABLE.name.Equals("yes"))
                {
                    VARIABLE.onClick.AddListener(() => Switch(true));
                }

                if (VARIABLE.name.Equals("no"))
                {
                    VARIABLE.onClick.AddListener(() => Switch(false));
                }
            }
        }

        private void OnBackup(KeyEvent e)
        {
            e.Use();
            Switch(false);
        }

        private void Switch(bool state)
        {
            switch (state)
            {
                case true:
                    this.onYes?.Invoke();
                    break;
                case false:
                    this.onNo?.Invoke();
                    break;
            }

            BehaviourScriptable.instance.UnsetupKeyDown(KeyCode.Escape, OnBackup);
            this.Disable();
            OnShowMsgBox();
        }


        class MsgData
        {
            public string title;
            public string content;
            public Action onYes;
            public Action onNo;
        }

        private static UIMsgBox _instance;
        private static Queue<MsgData> _msgQueue = new Queue<MsgData>();

        private static void OnShowMsgBox()
        {
            if (_msgQueue.Count == 0)
            {
                if (_instance is not null)
                {
                    UIManager.instance.Close<UIMsgBox>();
                    _instance = null;
                }

                return;
            }

            MsgData data = _msgQueue.Dequeue();
            string resPath = $"Resources/{BasicConfig.instance.curEntry.entryName}/MsgBox";
            if (_instance is null)
            {
                _instance = UIManager.instance.Open<UIMsgBox>(resPath, data.title, data.content, data.onYes, data.onNo);
            }
            else
            {
                _instance.Enable(data.title, data.content, data.onYes, data.onNo);
            }
        }

        public static void Show(string title, string content, Action onYes, Action onNo)
        {
            _msgQueue.Enqueue(new MsgData()
            {
                title = title,
                content = content,
                onYes = onYes,
                onNo = onNo,
            });
            if (_instance is not null)
            {
                return;
            }

            OnShowMsgBox();
        }

        public static void Show(string content, Action onYes, Action onNo)
        {
            Show("提示", content, onYes, onNo);
        }

        public static void Show(string content, Action onYes)
        {
            Show("提示", content, onYes, null);
        }

        public static void Show(string content)
        {
            Show("提示", content, null, null);
        }
    }
}