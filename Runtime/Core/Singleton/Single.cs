public class Single<T> where T : Single<T>, new()
{
    private class SingletonHandle
    {
        private static T _instance;

        public static T GetInstance()
        {
            if (_instance is null)
            {
                _instance = new T();
            }

            return _instance;
        }
    }

    public static T instance => SingletonHandle.GetInstance();
}