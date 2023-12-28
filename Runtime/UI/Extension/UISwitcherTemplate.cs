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