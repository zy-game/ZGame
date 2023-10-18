using System;
using System.Collections.Generic;

namespace ZEngine.Window
{
    public interface IUIDataListViewBindPipeline : IUIComponentBindPipeline, IEnumerable<IUIViewDataTemplate>
    {
        string templatePath { get; }
        IUIViewDataTemplate this[int index] { get; }
        int count { get; }

        void Add(IUIViewDataTemplate template);
        void Remove(IUIViewDataTemplate template);
        void Remove(int index);

        public static IUIDataListViewBindPipeline Create(UIWindow window, string path)
        {
            return default;
        }
    }

    public interface IUIViewDataTemplate : IUIComponentBindPipeline, IDisposable, ICloneable
    {
        IUIDataPropertiesBindPipeline this[int index] { get; }
        IUIDataPropertiesBindPipeline this[string name] { get; }

        public static IUIViewDataTemplate Create(UIWindow window, string path)
        {
            return default;
        }
    }

    public interface IUIDataPropertiesBindPipeline : IUIComponentBindPipeline
    {
        public static IUIDataPropertiesBindPipeline Create(UIWindow window, string path)
        {
            return default;
        }
    }
}