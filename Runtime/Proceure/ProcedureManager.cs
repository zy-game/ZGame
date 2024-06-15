using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZGame
{
    public sealed class ProcedureManager : GameManager
    {
        private Dictionary<object, CancellationTokenSource> sourceList = new();

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args">参数</param>
        /// <typeparam name="TProceure">过程对象类型</typeparam>
        public void Execute<TProceure>(params object[] args) where TProceure : IProcedure
        {
            Execute<object, TProceure>(args);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args">参数</param>
        /// <typeparam name="T">返回类型</typeparam>
        /// <typeparam name="TProceure">过程对象类型</typeparam>
        /// <returns></returns>
        public TResult Execute<TResult, TProceure>(params object[] args) where TProceure : IProcedure<TResult>
        {
            using (TProceure proceure = RefPooled.Alloc<TProceure>())
            {
                return proceure.Execute(args);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="key">取消令牌</param>
        /// <param name="args">参数</param>
        /// <typeparam name="TProceure">过程对象类型</typeparam>
        public async UniTask Execute<TProceure>(object key, params object[] args) where TProceure : IProcedureAsync
        {
            await Execute<object, TProceure>(args);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="key">取消令牌</param>
        /// <param name="args">参数</param>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <typeparam name="TProceure">过程对象类型</typeparam>
        /// <returns></returns>
        public async UniTask<TResult> Execute<TResult, TProceure>(object key, params object[] args) where TProceure : IProcedureAsync<TResult>
        {
            using (TProceure proceure = RefPooled.Alloc<TProceure>())
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                sourceList.Add(key, cancellationTokenSource);
                var result = await proceure.Execute(args).AttachExternalCancellation(cancellationTokenSource.Token);
                sourceList.Remove(key);
                return result;
            }
        }


        /// <summary>
        /// 取消正在执行的命令
        /// </summary>
        /// <param name="key">取消令牌</param>
        public void Cancel(object key)
        {
            if (sourceList.TryGetValue(key, out CancellationTokenSource cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// 取消所有正在执行的命令
        /// </summary>
        public void CancelAll()
        {
            foreach (var VARIABLE in sourceList.Values)
            {
                VARIABLE.Cancel();
            }

            sourceList.Clear();
        }
    }
}