using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    public interface ITiming : IDisposable
    {
        float runtime { get; }
        int frameRate { get; }

        void DelayCall(float time, Action callback);

        public static ITiming Default { get; internal set; }

        public static ITiming Create(int frameRate)
        {
            return new Timing(frameRate);
        }

        class Timing : ITiming
        {
            public float runtime { get; set; }
            public int frameRate { get; set; }
            private List<TimingTask> tasks = new List<TimingTask>();

            public Timing(int frameRate)
            {
                this.frameRate = frameRate;
                Application.targetFrameRate = this.frameRate;
                Behaviour.OnUpdate(Update);
            }

            private void Update()
            {
                this.runtime = Time.realtimeSinceStartup;
                for (int i = tasks.Count - 1; i >= 0; i--)
                {
                    if (this.runtime < tasks[i].end)
                    {
                        continue;
                    }

                    tasks[i].action?.Invoke();
                    // ZGame.Console.Log($"Timing Task Start Time:{tasks[i].start} Trigger Time:{tasks[i].end}");
                    tasks.Remove(tasks[i]);
                }
            }

            public void DelayCall(float time, Action callback)
            {
                tasks.Add(new TimingTask()
                {
                    start = this.runtime,
                    end = this.runtime + time,
                    action = callback
                });
            }

            public void Dispose()
            {
                Behaviour.RemoveUpdate(Update);
            }

            class TimingTask
            {
                public float start;
                public float end;
                public Action action;
            }
        }
    }
}