using System.Collections;

namespace ZEngine
{
    public interface IDialogHandle<T> : IExecuteHandle<T>
    {
        T result { get; }
        void Setitle(string title);
        void SetMessage(string message);
    }
}