using System;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    public interface IUIComponentGenerator : IDisposable
    {
        void Execute(IUICodeGenerator parent, GameObject gameObject, UIGeneratorWriter writer);
    }
}