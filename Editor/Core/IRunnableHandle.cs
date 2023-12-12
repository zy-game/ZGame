using System;
using System.Threading.Tasks;

namespace ZGame.Editor
{
    public interface IRunnableHandle<T> : IDisposable
    {
        Task<T> Execute(params object[] args);
    }
}