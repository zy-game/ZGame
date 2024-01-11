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

        public override void Awake()
        {
        }

        public override void Enable(params object[] args)
        {
            this.onYes = (Action)(args[2]);
            this.onNo = (Action)(args[3]);
            TMP_Text[] texts = this.gameObject.GetComponentsInChildren<TMP_Text>();
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

            Button[] buttons = this.gameObject.GetComponentsInChildren<Button>();
            foreach (var VARIABLE in buttons)
            {
                if (VARIABLE.name.Equals("yes"))
                {
                    VARIABLE.onClick.AddListener(() =>
                    {
                        this.onYes();
                        this.Dispose();
                    });
                }

                if (VARIABLE.name.Equals("no"))
                {
                    VARIABLE.onClick.AddListener(() =>
                    {
                        this.onNo();
                        this.Dispose();
                    });
                }
            }
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
                return;
            }

            MsgData data = _msgQueue.Dequeue();
            string resPath = $"Resources/{BasicConfig.instance.curEntry.entryName}/MsgBox";
            UIMsgBox _instance = UIManager.instance.Open<UIMsgBox>(resPath);
            _instance.Enable(data.title, data.content, new Action(() =>
            {
                data.onYes();
                _instance.Dispose();
                _instance = null;
                OnShowMsgBox();
            }), new Action(() =>
            {
                data.onNo();
                _instance.Dispose();
                _instance = null;
                OnShowMsgBox();
            }));
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
            Show("Tips", content, onYes, onNo);
        }

        public static void Show(string content, Action onYes)
        {
            Show("Tips", content, onYes, null);
        }

        public static void Show(string content)
        {
            Show("Tips", content, null, null);
        }
    }
}