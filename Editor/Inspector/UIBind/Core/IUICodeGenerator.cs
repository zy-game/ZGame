using System;
using ZGame.UI;

namespace ZGame.Editor.UIBind
{
    /// <summary>
    /// UI代码生成器
    /// </summary>
    public interface IUICodeGenerator : IDisposable
    {
        UI.UIDocment docment { get; }

        /// <summary>
        /// 执行生成器
        /// </summary>
        /// <param name="docment">UI 绑定数据</param>
        /// <param name="writerOverride">是否生成重载代码</param>
        void Execute(UI.UIDocment docment, UIGeneratorWriter writer);
    }
}