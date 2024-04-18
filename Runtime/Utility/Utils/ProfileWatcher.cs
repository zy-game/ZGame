using System.Diagnostics;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace ZGame
{
    public sealed class ProfileWatcher : IReference
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
            Debug.Log($"watch complete. use time:{stopwatch.ElapsedMilliseconds} ms");
        }

        public static ProfileWatcher StartProfileWatcher(string watchName)
        {
            ProfileWatcher watcher = RefPooled.Spawner<ProfileWatcher>();
            watcher.watchName = watchName;
            watcher.OnStart();
            return watcher;
        }
    }
}