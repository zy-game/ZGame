using System.Collections;

namespace ZEngine
{
    public interface IDialog<T> : IReference
    {
        T result { get; }

        IEnumerator EnsureDialogComplete();
    }
}