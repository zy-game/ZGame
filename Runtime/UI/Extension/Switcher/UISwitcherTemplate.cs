using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    public class UISwitcherTemplate : Template
    {
        public SwitchType2 type;
        public SwitchOptions options;
        private Image image;
        private Text text;
        private TextMeshProUGUI textMeshProUGUI;
        public bool isSelect;
        public ParamType paramType;
        public int _v1;
        public float _v2;
        public string _v3;
        public bool _v4;
        public Vector2 _v5;
        public Vector3 _v6;
        public Vector4 _v7;
        public Color _v8;

        public object param
        {
            get
            {
                return paramType switch
                {
                    ParamType.Int => _v1,
                    ParamType.Float => _v2,
                    ParamType.String => _v3,
                    ParamType.Bool => _v4,
                    ParamType.Vector2 => _v5,
                    ParamType.Vector3 => _v6,
                    ParamType.Vector4 => _v7,
                    ParamType.Color => _v8,
                    _ => null
                };
            }
        }

        private void Awake()
        {
            if (type == SwitchType2.Sprite)
            {
                image = GetComponent<Image>();
            }
        }

        public void Select()
        {
            isSelect = true;
            if (type == SwitchType2.Sprite)
            {
                if (image != null)
                {
                    image.sprite = options.activeSprite;
                }
            }
            else if (type == SwitchType2.Text)
            {
                if (text != null)
                {
                    text.text = options.activeText;
                }

                if (textMeshProUGUI != null)
                {
                    textMeshProUGUI.text = options.activeText;
                }
            }
            else if (type == SwitchType2.GameObject)
            {
                if (options.gameObject != null)
                {
                    options.gameObject.SetActive(true);
                }
            }
        }

        public void Unselect()
        {
            isSelect = false;
            if (type == SwitchType2.Sprite)
            {
                if (image != null)
                {
                    image.sprite = options.inactiveSprite;
                }
            }
            else if (type == SwitchType2.Text)
            {
                if (text != null)
                {
                    text.text = options.inactiveText;
                }

                if (textMeshProUGUI != null)
                {
                    textMeshProUGUI.text = options.inactiveText;
                }
            }
            else if (type == SwitchType2.GameObject)
            {
                if (options.gameObject != null)
                {
                    options.gameObject.SetActive(false);
                }
            }
        }
    }
}