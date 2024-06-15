using System.Diagnostics;
using UnityEngine.Profiling;
using ZGame.Logger;
using Debug = UnityEngine.Debug;

namespace ZGame
{
    public sealed class Watcher : IReference
    {
        private string watchName;
        private Stopwatch stopwatch;

        public void Release()
        {
            OnStop();
        }

        private void OnStart()
        {
            stopwatch = new Stopwatch();
            Profiler.BeginSample(watchName);
            stopwatch.Start();
        }

        private void OnStop()
        {
            Profiler.EndSample();
            stopwatch.Stop();
            AppCore.Logger.Log($"[{nameof(Watcher).ToUpper()}] [{watchName}] complete. use time:{stopwatch.ElapsedMilliseconds} ms");
        }

        public static Watcher Start(string watchName)
        {
            Watcher watcher = RefPooled.Alloc<Watcher>();
            watcher.watchName = $"[Watch]{watchName}";
            watcher.OnStart();
            return watcher;
        }
    }
}