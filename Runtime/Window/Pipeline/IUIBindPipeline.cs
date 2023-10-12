using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Window
{
    public enum UIBindType : byte
    {
    }

    public interface IUIBindPipeline : IDisposable
    {
        object value { get; }
        void OnChangeValue(object args);

        public static IUIBindPipeline Create(GameObject gameObject, UIBindType type)
        {
            return default;
        }
    }


    public interface IUIDataListViewBindPipeline : IUIBindPipeline, IEnumerable<IUIViewDataItem>
    {
        IUIViewDataItem this[int index] { get; }
    }

    public interface IUIViewDataListSelector : IDisposable
    {
        IUIViewDataItem[] GetSelectors();

        public static IUIViewDataListSelector Create(IUIDataListViewBindPipeline pipeline)
        {
            return default;
        }
    }

    public interface IUIViewDataItemPropertiesBindPipeline : IUIBindPipeline
    {
    }

    public interface IUIViewDataItem : IDisposable, ICloneable
    {
    }
}