using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ZGame.Notify;

namespace ZGame
{
    public sealed class AppQuitEventDatable : IGameEventArgs
    {
        public void Release()
        {
        }
    }

    public sealed class CancellationTokenSourceGroup : IDisposable
    {
        private List<CancellationTokenSource> tokens = new();

        public CancellationTokenSourceGroup(CancellationTokenSource cancellationTokenSource)
        {
            CoreAPI.Notify.Subscribe<AppQuitEventDatable>(KeyCode.Escape, OnHandleAppQuitEvent);
            if (cancellationTokenSource is null)
            {
                return;
            }

            cancellationTokenSource.Token.Register(this.Cancel);
        }

        private void OnHandleAppQuitEvent(AppQuitEventDatable datable)
        {
            Cancel();
        }

        public CancellationToken GetToken()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            tokens.Add(cancellationTokenSource);
            return cancellationTokenSource.Token;
        }

        public void Cancel()
        {
            foreach (var token in tokens)
            {
                token.Cancel();
            }

            Dispose();
        }

        public void Dispose()
        {
            CoreAPI.Notify.Unsubscribe<AppQuitEventDatable>(KeyCode.Escape, OnHandleAppQuitEvent);
            foreach (var token in tokens)
            {
                token.Dispose();
            }

            tokens.Clear();
        }
    }
}