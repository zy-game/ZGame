using System;
using System.Collections.Generic;

namespace ZGame.Runnable
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class RunnableManager : SingletonBehaviour<RunnableManager>
    {
        private List<RunnableHandle> runningList = new();
        private Queue<Tuple<RunnableHandle, object[]>> runningQueue = new();

        /// <summary>
        /// 执行逻辑单元
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        public void Run<T>(params object[] args) where T : RunnableHandle
        {
            Run(typeof(T), args);
        }

        /// <summary>
        /// 执行逻辑单元
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        public void Run(Type type, params object[] args)
        {
            if (type.IsSubclassOf(typeof(RunnableHandle)) is false)
            {
                throw new Exception("RunnableHandle is not subclass of RunnableHandle");
            }

            var runnable = (RunnableHandle)Activator.CreateInstance(type, args);
            runningQueue.Enqueue(new Tuple<RunnableHandle, object[]>(runnable, args));
        }

        /// <summary>
        /// 清理所有运行单元
        /// </summary>
        public void Clear()
        {
            runningList.ForEach(r => r.Dispose());
            runningList.Clear();
            while (runningQueue.Count > 0)
            {
                var dequeue = runningQueue.Dequeue();
                dequeue.Item1.Dispose();
            }

            runningQueue.Clear();
        }

        protected override void OnUpdate()
        {
            if (runningList.Count > 0)
            {
                for (int i = runningList.Count - 1; i >= 0; i--)
                {
                    runningList[i].OnUpdate();
                    if (runningList[i].IsCompletion() is false)
                    {
                        continue;
                    }

                    runningList[i].Dispose();
                    runningList.RemoveAt(i);
                }
            }

            if (runningQueue.Count > 0 && runningList.Count < GlobalConfig.instance.parallelRunnableCount)
            {
                var tuple = runningQueue.Dequeue();
                runningList.Add(tuple.Item1);
                tuple.Item1.OnStart(tuple.Item2);
            }
        }
    }
}