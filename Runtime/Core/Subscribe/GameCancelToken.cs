using System;

namespace ZEngine
{
    public class GameCancelToken : IGameCancelToken
    {
        private Action cancelCallback;
        private bool isCanceled = false;

        internal void Register(Action cancelCallback)
        {
            this.cancelCallback += cancelCallback;
        }

        public void Release()
        {
            cancelCallback = null;
        }

        public void Cancel()
        {
            if (isCanceled)
            {
                Engine.Console.Error(GameEngineException.Create("Cannot Canceled A already cancel token"));
                return;
            }

            isCanceled = true;
            cancelCallback?.Invoke();
        }

        public bool TryCancel()
        {
            if (isCanceled)
            {
                return false;
            }
            Cancel();
            return isCanceled;
        }
    }
}