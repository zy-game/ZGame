using System;
using System.Collections.Generic;
using System.Threading;
using ZGame.Notify;

namespace ZGame
{
    public sealed class AppQuitEventArgs : INotifyArgs
    {
        public void Dispose()
        {
        }
    }

    public sealed class CancellationTokenSourceGroup : IDisposable
    {
        private List<CancellationTokenSource> tokens = new();

        public CancellationTokenSourceGroup(CancellationTokenSource cancellationTokenSource)
        {
            WorkApi.Notify.Subscribe<AppQuitEventArgs>(OnHandleAppQuitEvent);
            if (cancellationTokenSource is null)
            {
                return;
            }

            cancellationTokenSource.Token.Register(this.Cancel);
        }

        private void OnHandleAppQuitEvent(AppQuitEventArgs args)
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
            WorkApi.Notify.Unsubscribe<AppQuitEventArgs>(OnHandleAppQuitEvent);
            foreach (var token in tokens)
            {
                token.Dispose();
            }

            tokens.Clear();
        }
    }
}