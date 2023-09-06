using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZEngine.Editor
{
    public class CustomGraphView : IDisposable
    {
        private Background _background;
        private EditorWindow window;
        public Background background => _background;
        private List<CustomGraphNode> nodes = new List<CustomGraphNode>();

        public CustomGraphView(EditorWindow window)
        {
            this.window = window;
            _background = new Background(window);
            nodes.Add(new CustomGraphNode());
        }

        public void OnGUI(Rect rect)
        {
            GUI.BeginGroup(rect);
            // rect = new Rect(rect.x - 298, rect.y - 255, rect.width, rect.height);
            _background.OnGUI(rect);
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].OnGUI(rect, window);
            }

            GUI.EndGroup();
        }

        public void Dispose()
        {
        }
    }

    [Serializable]
    public class CustomGraphNode : IDisposable
    {
        private Rect rect;
        private bool isDrag;
        private Vector2 position;
        private Vector2 offset;
        private Vector2 scaleOffset;
        private float maxWidth = 400;
        private float maxHeight = 400;
        private float minWidth = 100;
        private float minHeight = 100;

        public void Dispose()
        {
        }

        public void OnGUI(Rect rect, EditorWindow window)
        {
            if (this.rect.Equals(Rect.zero))
            {
                this.rect = new Rect(rect.x, rect.y, minWidth, minHeight);
            }

            Rect boxRect = new Rect(this.rect.position + offset + scaleOffset, this.rect.size);
            GUI.Box(boxRect, "Test", "FrameBox");
            if (Event.current.type == EventType.ScrollWheel && rect.Contains(Event.current.mousePosition))
            {
                this.rect.width = Math.Clamp(this.rect.width - Event.current.delta.y * 10, minWidth, maxWidth);
                this.rect.height = Math.Clamp(this.rect.height - Event.current.delta.y * 10, minHeight, maxHeight);
                scaleOffset = new Vector2(Math.Clamp(scaleOffset.x - Event.current.delta.y * 5, 0, 150), Math.Clamp(scaleOffset.y - Event.current.delta.y * 5, 0, 150));
                window.Repaint();
            }

            if (Event.current.type == EventType.MouseDown && ((Event.current.button == 0 && boxRect.Contains(Event.current.mousePosition)) || Event.current.button == 2))
            {
                isDrag = true;
                position = Event.current.mousePosition;
            }

            if (Event.current.type == EventType.MouseDrag && ((Event.current.button == 0 && isDrag) || Event.current.button == 2))
            {
                if (Vector2.zero.Equals(position))
                {
                    return;
                }

                offset += Event.current.mousePosition - position;
                position = Event.current.mousePosition;
                window.Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                isDrag = false;
                position = Vector2.zero;
            }
        }
    }

    public class Background
    {
        private float _size = 20f;
        private EditorWindow window;
        public float gridSize => _size;
        private Vector2 offset;
        private Vector2 position;
        private Vector2 detle;
        private Color main = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        private Color sub = new Color(0.1f, 0.1f, 0.1f, 0.3f);

        public Background(EditorWindow window)
        {
            this.window = window;
        }

        public void SetGridSize(float size)
        {
            this._size = size;
        }

        public void OnGUI(Rect rect)
        {
            rect = new Rect(rect.x - 298, rect.y - 255, rect.width, rect.height);
            int x = (int)(rect.width / _size);
            int y = (int)(rect.height / _size);

            if (y % 5 != 0)
            {
                y += 5 - y % 5;
            }

            if (x % 5 != 0)
            {
                x += 5 - x % 5;
            }

            float height = y * _size;
            float width = x * _size;
            for (int i = 0; i < x; i++)
            {
                float offset_x = rect.x + i * _size + offset.x;
                if (offset_x > width)
                {
                    offset_x = offset_x % width;
                }

                if (offset_x < 0)
                {
                    offset_x = width - (Math.Abs(offset_x) % width);
                }

                window.BeginColor(i % 5 == 0 ? main : sub);
                {
                    GUI.Box(new Rect(offset_x, rect.y, 1, rect.height), String.Empty, EngineCustomEditor.GUI_STYLE_LINE);
                    window.EndColor();
                }
            }

            for (int i = 0; i < y; i++)
            {
                float offset_y = rect.y + i * _size + offset.y;
                if (offset_y > height)
                {
                    offset_y = (offset_y % height);
                }

                if (offset_y < 0)
                {
                    offset_y = height - (Math.Abs(offset_y) % height);
                }

                window.BeginColor(i % 5 == 0 ? main : sub);
                {
                    GUI.Box(new Rect(rect.x, offset_y, rect.width, 1), String.Empty, EngineCustomEditor.GUI_STYLE_LINE);
                    window.EndColor();
                }
            }

            if (Event.current.type == EventType.ScrollWheel && rect.Contains(Event.current.mousePosition))
            {
                _size -= Event.current.delta.y;
                _size = Math.Clamp(_size, 20, 50);
                window.Repaint();
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
            {
                position = Event.current.mousePosition;
            }

            if (Event.current.type == EventType.MouseDrag && rect.Contains(Event.current.mousePosition) && Event.current.button == 2)
            {
                if (Vector2.zero.Equals(position))
                {
                    return;
                }

                offset += Event.current.mousePosition - position;
                position = Event.current.mousePosition;
                window.Repaint();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 2)
            {
                position = Vector2.zero;
            }
        }
    }
}